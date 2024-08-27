#define _GNU_SOURCE

#include "spdk_io_device.h"

#include <locale.h>
#include <rte_ring.h>
#include <threads.h>
#include <time.h>

#define NS_MAX_NUM 8
#define IO_BATCH_NUM 8
#define POLL_TIME 64
#define SIZE_1G (1 * 1024 * 1024 * 1024)

struct spdk_ns_entry {
    struct spdk_nvme_ctrlr *ctrlr;
    struct spdk_nvme_ns *ns;
    uint64_t ns_size;
    uint32_t ns_sector_size;
    int32_t nsid;
};

struct spdk_io_device {
    pthread_t poller_thread_id;
    volatile bool is_polling;
    struct rte_ring *io_ring;
    struct spdk_nvme_qpair *qp;
    struct spdk_ns_entry *ns;
};

struct internal_io_context {
    struct spdk_io_device *io_device;
    uint64_t target_lba;
    uint32_t lba_count;
    void *buffer;
    void *read_dest;
    AsyncIOCallback complete_callback;
    void *complete_callback_context;
};

volatile static bool initted = false;
static pthread_mutex_t module_init_lock = PTHREAD_MUTEX_INITIALIZER;
volatile static bool attach_error = 0;

volatile static int32_t g_ns_num = 0;
static struct spdk_ns_entry g_spdk_ns_list[NS_MAX_NUM];

static int32_t register_ns(struct spdk_nvme_ctrlr *ctrlr,
                           struct spdk_nvme_ns *ns, int nsid)
{
    struct spdk_ns_entry *entry;

    if (g_ns_num >= NS_MAX_NUM) {
        fprintf(stderr, "Too many NS, the max number is:%d\n", NS_MAX_NUM);
        return EOVERFLOW;
    }

    if (!spdk_nvme_ns_is_active(ns)) {
        return 0;
    }

    entry = &g_spdk_ns_list[g_ns_num++];
    entry->ctrlr = ctrlr;
    entry->ns = ns;
    entry->ns_sector_size = spdk_nvme_ns_get_sector_size(ns);
    entry->ns_size = entry->ns_sector_size * spdk_nvme_ns_get_num_sectors(ns);
    entry->nsid = nsid;
    return 0;
}

static bool probe_cb(void *cb_ctx, const struct spdk_nvme_transport_id *trid,
                     struct spdk_nvme_ctrlr_opts *opts)
{
    return true;
}

static void attach_cb(void *cb_ctx, const struct spdk_nvme_transport_id *trid,
                      struct spdk_nvme_ctrlr *ctrlr,
                      const struct spdk_nvme_ctrlr_opts *opts)
{
    int nsid;
    struct spdk_nvme_ns *ns;
    if (attach_error != 0) {
        return;
    }

    for (nsid = spdk_nvme_ctrlr_get_first_active_ns(ctrlr); nsid != 0;
         nsid = spdk_nvme_ctrlr_get_next_active_ns(ctrlr, nsid)) {
        ns = spdk_nvme_ctrlr_get_ns(ctrlr, nsid);
        if (ns == NULL) {
            continue;
        }
        attach_error = register_ns(ctrlr, ns, nsid);
    }
}

static void cleanup()
{
    struct spdk_nvme_detach_ctx *detach_ctx = NULL;

    for (int i = g_ns_num; i >= 0; i--) {
        spdk_nvme_detach_async(g_spdk_ns_list[i].ctrlr, &detach_ctx);
        g_ns_num--;
    }

    if (detach_ctx) {
        spdk_nvme_detach_poll(detach_ctx);
    }
}

int init()
{
    setlocale(LC_ALL, "");
    int rc = 0;
    pthread_mutex_lock(&module_init_lock);
    if (initted) {
        goto exit;
    }
    spdk_log_set_print_level(SPDK_LOG_DEBUG);
    struct spdk_env_opts opts;
    spdk_env_opts_init(&opts);
    opts.name = "garnet_spdk_device";
    rc = spdk_env_init(&opts);
    if (rc != 0) {
        fprintf(stderr, "ERROR: Unable to initialize SPDK env error:%d.\n", rc);
        goto exit;
    }

    spdk_unaffinitize_thread();

    rc = spdk_nvme_probe(NULL, NULL, probe_cb, attach_cb, NULL);
    if (rc != 0) {
        fprintf(stderr, "ERROR: spdk_nvme_probe() failed.\n");
        goto exit;
    }
    if (attach_error != 0) {
        fprintf(stderr, "ERROR: some ns register failed with error code: %d\n.",
                attach_error);
        rc = attach_error;
        goto exit;
    }

    if (g_ns_num == 0) {
        fprintf(stderr, "ERROR: no NVMe namespace found.\n");
        rc = 1;
        goto exit;
    }

exit:
    if (rc == 0) {
        initted = true;
    } else {
        cleanup();
    }
    pthread_mutex_unlock(&module_init_lock);
    return rc;
}

static struct spdk_ns_entry *ns_lookup(int32_t nsid)
{
    struct spdk_ns_entry *ns_entry = NULL;
    for (int i = 0; i < g_ns_num; i++) {
        if (g_spdk_ns_list[i].nsid == nsid) {
            ns_entry = &g_spdk_ns_list[i];
            break;
        }
    }
    return ns_entry;
}

static struct internal_io_context *
get_internal_io_context(struct spdk_io_device *io_device,
                        uint64_t target_address, uint32_t io_length,
                        void *read_dest, AsyncIOCallback callback,
                        void *call_back_context)
{
    struct internal_io_context *io_context = NULL;

    io_context = malloc(sizeof(struct internal_io_context));
    if (io_context == NULL) {
        goto exit;
    }
    io_context->buffer =
        spdk_malloc(io_length, io_device->ns->ns_sector_size, NULL,
                    SPDK_ENV_SOCKET_ID_ANY, SPDK_MALLOC_DMA);
    if (io_context->buffer == NULL) {
        free(io_context);
        io_context = NULL;
        goto exit;
    }
    io_context->target_lba = target_address / io_device->ns->ns_sector_size;
    io_context->lba_count = io_length / io_device->ns->ns_sector_size;
    io_context->read_dest = read_dest;
    io_context->complete_callback = callback;
    io_context->complete_callback_context = call_back_context;
    io_context->io_device = io_device;

exit:
    return io_context;
}

void put_internal_io_context(struct internal_io_context **io_context)
{
    spdk_free((*io_context)->buffer);
    free(*io_context);
    (*io_context) = NULL;
}

static void io_complete(void *arg, const struct spdk_nvme_cpl *completion)
{
    struct internal_io_context *io_context = (struct internal_io_context *)arg;
    int16_t rc = 0;
    uint32_t bytes_transferred = 0;
    AsyncIOCallback callback = io_context->complete_callback;
    void *callback_context = io_context->complete_callback_context;

    if (spdk_nvme_cpl_is_error(completion)) {
        spdk_nvme_print_completion(
            spdk_nvme_qpair_get_id(io_context->io_device->qp),
            (struct spdk_nvme_cpl *)completion);
        fprintf(stderr, "I/O error status: %s\n",
                spdk_nvme_cpl_get_status_string(&completion->status));
        rc = completion->status_raw;
    } else {
        if (io_context->read_dest != NULL) {
            memcpy(io_context->read_dest, io_context->buffer,
                   io_context->lba_count *
                       io_context->io_device->ns->ns_sector_size);
        }
        bytes_transferred =
            io_context->lba_count * io_context->io_device->ns->ns_sector_size;
    }
    callback(callback_context, (int32_t)rc, bytes_transferred);
    put_internal_io_context(&io_context);
}

uint64_t spdk_io_device_get_ns_size(int32_t nsid)
{
    struct spdk_ns_entry *ns = ns_lookup(nsid);
    if (ns == NULL) {
        return 0;
    } else {
        return ns->ns_size;
    }
}

uint32_t spdk_io_device_get_ns_sector_size(int32_t nsid)
{
    struct spdk_ns_entry *ns = ns_lookup(nsid);
    if (ns == NULL) {
        return 0;
    } else {
        return ns->ns_sector_size;
    }
}

struct spdk_io_device *spdk_io_device_create(uint32_t nsid)
{
    struct spdk_io_device *device = NULL;

    struct spdk_ns_entry *ns = ns_lookup(nsid);
    if (ns == NULL) {
        fprintf(stderr, "ERROR: can't find ns:%d\n", nsid);
        goto exit;
    }

    device = malloc(sizeof(struct spdk_io_device));
    if (device == NULL) {
        fprintf(stderr, "ERROR: can't create spdk_device. Out of memory.\n");
        goto exit;
    }

    device->ns = ns;
    device->qp = spdk_nvme_ctrlr_alloc_io_qpair(ns->ctrlr, NULL, 0);
    if (device->qp == NULL) {
        fprintf(stderr, "ERROR: spdk_nvme_ctrlr_alloc_io_qpair() failed.\n");
        free(device);
        device = NULL;
        goto exit;
    }
    device->poller_thread_id = 0;
    device->is_polling = false;
    device->io_ring = NULL;

exit:
    return device;
}

void spdk_io_device_destroy(struct spdk_io_device *io_device)
{
    if (io_device->poller_thread_id) {
        spdk_io_device_stop_poll(io_device);
    }

    spdk_nvme_ctrlr_free_io_qpair(io_device->qp);
    free(io_device);
}

int32_t spdk_io_device_read_async(struct spdk_io_device *io_device,
                                  uint64_t source, void *dest, uint32_t length,
                                  AsyncIOCallback callback,
                                  void *callback_context)
{
    struct internal_io_context *io_context = NULL;
    int rc = 0;

    uint32_t ns_sector_size = io_device->ns->ns_sector_size;
    if (length % ns_sector_size != 0) {
        fprintf(stderr,
                "ERROR: io size %d is not sector align, sector size is %d.\n",
                length, ns_sector_size);
        rc = EINVAL;
        goto exit;
    }

    io_context = get_internal_io_context(io_device, source, length, dest,
                                         callback, callback_context);
    if (io_context == NULL) {
        rc = ENOMEM;
        goto exit;
    }

    if (io_device->poller_thread_id != 0) {
        rc = rte_ring_enqueue(io_device->io_ring, (void *)io_context);
    } else {
        rc = spdk_nvme_ns_cmd_read(io_device->ns->ns, io_device->qp,
                                   io_context->buffer, io_context->target_lba,
                                   io_context->lba_count, io_complete,
                                   (void *)io_context, 0);
    }

exit:
    if (rc != 0) {
        if (io_context != NULL) {
            put_internal_io_context(&io_context);
        }
    }

    return rc;
}

int32_t spdk_io_device_write_async(struct spdk_io_device *io_device,
                                   const void *source, uint64_t dest,
                                   uint32_t length, AsyncIOCallback callback,
                                   void *callback_context)
{
    struct internal_io_context *io_context = NULL;
    int rc = 0;

    uint32_t ns_sector_size = io_device->ns->ns_sector_size;
    if (length % ns_sector_size != 0) {
        fprintf(stderr,
                "ERROR: io size %d is not sector align, sector size is %d.\n",
                length, ns_sector_size);
        rc = EINVAL;
        goto exit;
    }

    io_context = get_internal_io_context(io_device, dest, length, NULL,
                                         callback, callback_context);
    if (io_context == NULL) {
        rc = ENOMEM;
        goto exit;
    }
    memcpy(io_context->buffer, source, length);

    if (io_device->poller_thread_id) {
        rc = rte_ring_enqueue(io_device->io_ring, (void *)io_context);
    } else {
        rc = spdk_nvme_ns_cmd_write(io_device->ns->ns, io_device->qp,
                                    io_context->buffer, io_context->target_lba,
                                    io_context->lba_count, io_complete,
                                    (void *)io_context, 0);
    }

exit:
    if (rc != 0) {
        if (io_context != NULL) {
            put_internal_io_context(&io_context);
        }
    }

    return rc;
}

int32_t spdk_io_device_poll(struct spdk_io_device *io_device,
                            uint32_t batch_num)
{
    return spdk_nvme_qpair_process_completions(io_device->qp, batch_num);
}

void *spdk_io_device_poller(void *arg)
{
    struct spdk_io_device *io_device = (struct spdk_io_device *)arg;

    static struct internal_io_context *pending_io[256];

    bool l_is_polling = io_device->is_polling;
    int poll_count = 0;

    while (l_is_polling) {
        // submit IO.
        int rc;
        uint32_t pending_io_num = rte_ring_dequeue_burst(
            io_device->io_ring, (void **)pending_io,
            sizeof(pending_io) / sizeof(struct internal_io_context *), NULL);

        for (int i = 0; i < pending_io_num; i++) {
            struct internal_io_context *io = pending_io[i];
            if (io->read_dest) {
                rc = spdk_nvme_ns_cmd_read(
                    io_device->ns->ns, io_device->qp, io->buffer,
                    io->target_lba, io->lba_count, io_complete, (void *)io, 0);
            } else {
                rc = spdk_nvme_ns_cmd_write(
                    io_device->ns->ns, io_device->qp, io->buffer,
                    io->target_lba, io->lba_count, io_complete, (void *)io, 0);
            }
            if (rc != 0) {
                io->complete_callback(io->complete_callback_context, rc, 0);
                put_internal_io_context(&io);
            }
        }

        // poll completion.
        int complete_io_num =
            spdk_nvme_qpair_process_completions(io_device->qp, IO_BATCH_NUM);
        poll_count += 1;
        if (poll_count == 300) {
            poll_count = 0;
            l_is_polling = io_device->is_polling;
        }
    }

    fprintf(stderr, "polling thread exit\n");
}

int32_t spdk_io_device_begin_poll(struct spdk_io_device *io_device,
                                  int32_t affinity)
{
    int rc = 0;
    if (io_device->poller_thread_id != 0) {
        fprintf(stderr, "ERROR: can't reattach spdk_device.\n");
        rc = -1;
        goto exit;
    }
    char ring_name[7 + 14 + 1];
    snprintf(ring_name, sizeof(ring_name), "io_ring0x%" PRIx64,
             (uint64_t)io_device);
    io_device->io_ring =
        rte_ring_create(ring_name, 1024, SOCKET_ID_ANY, RING_F_SC_DEQ);
    if (io_device->io_ring == NULL) {
        fprintf(stderr, "ERROR: can't create io queue.\n");
        rc = -1;
        goto exit;
    }
    io_device->is_polling = true;
    rc = pthread_create(&io_device->poller_thread_id, NULL,
                        spdk_io_device_poller, (void *)io_device);
    if (rc != 0) {
        fprintf(stderr, "ERROR: create poller thread failed with %d.\n", rc);
        io_device->poller_thread_id = 0;
        goto exit;
    }

    if (affinity >= 0) {
        cpu_set_t cpu_set;
        CPU_ZERO(&cpu_set);
        CPU_SET(affinity, &cpu_set);
        pthread_setaffinity_np(io_device->poller_thread_id, sizeof(cpu_set),
                               &cpu_set);
    }
    pthread_setname_np(io_device->poller_thread_id, "poller");

exit:
    if (rc != 0) {
        if (io_device->poller_thread_id != 0) {
            io_device->is_polling = false;
            __sync_synchronize();
            pthread_join(io_device->poller_thread_id, NULL);
        }
    }
    return rc;
}

void spdk_io_device_stop_poll(struct spdk_io_device *io_device)
{
    if (io_device->poller_thread_id == 0) {
        return;
    }

    io_device->is_polling = false;
    __sync_synchronize();
    pthread_join(io_device->poller_thread_id, NULL);
    io_device->poller_thread_id = 0;
    rte_ring_free(io_device->io_ring);
    io_device->io_ring = NULL;
}
