#include <spdk/stdinc.h>

#include <spdk/env.h>
#include <spdk/log.h>
#include <spdk/nvme.h>
#include <spdk/nvme_zns.h>
#include <spdk/string.h>
#include <spdk/vmd.h>

#define EXPORTED_SYMBOL __attribute__((visibility("default")))
#define MAX_DEVICE_NAME_LEN

typedef void (*AsyncIOCallback)(void *context, int32_t result,
                                uint32_t bytes_transferred);

struct spdk_io_device;

EXPORTED_SYMBOL int32_t init();

EXPORTED_SYMBOL uint64_t spdk_io_device_get_ns_size(int32_t nsid);

EXPORTED_SYMBOL uint32_t spdk_io_device_get_ns_sector_size(int32_t nsid);

EXPORTED_SYMBOL struct spdk_io_device *spdk_io_device_create(uint32_t nsid);

EXPORTED_SYMBOL void spdk_io_device_destroy(struct spdk_io_device *io_device);

EXPORTED_SYMBOL int32_t spdk_io_device_read_async(
    struct spdk_io_device *io_device, uint64_t source, void *dest,
    uint32_t length, AsyncIOCallback callback, void *callback_context);

EXPORTED_SYMBOL int32_t spdk_io_device_write_async(
    struct spdk_io_device *io_device, const void *source, uint64_t dest,
    uint32_t length, AsyncIOCallback callback, void *callback_context);

EXPORTED_SYMBOL int32_t spdk_io_device_poll(struct spdk_io_device *io_device,
                                            uint32_t batch_num);

EXPORTED_SYMBOL int32_t
spdk_io_device_begin_poll(struct spdk_io_device *io_device, int32_t affinity);

EXPORTED_SYMBOL void spdk_io_device_stop_poll(struct spdk_io_device *io_device);
