#include "spdk_io_device.h"
#include <error.h>
#include <semaphore.h>
#include <string.h>

#define SIZE_4K 4 * 1024

char g_data_buff[SIZE_4K];
char g_data_read[SIZE_4K];

int32_t g_io_error = 0;

sem_t mutex;

void io_callback(void *context, int32_t result, uint32_t bytes_transferred)
{
    g_io_error = result;
    sem_post(&mutex);
}

int main()
{
    int rc = 0;
    struct spdk_io_device *io_device = NULL;

    sem_init(&mutex, 0, 0);

    rc = init();

    io_device = spdk_io_device_create(1);
    if (io_device == NULL) {
        fprintf(stderr, "device is NULL\n");
        rc = -1;
        goto exit;
    }

    spdk_io_device_begin_poll(io_device, -1);

    const char *data = "hello world";
    strcpy(g_data_buff, data);
    rc = spdk_io_device_write_async(io_device, g_data_buff, 0, SIZE_4K,
                                    io_callback, NULL);
    if (rc != 0) {
        fprintf(stderr, "I/O write error %d.\n", rc);
        goto exit;
    }
    sem_wait(&mutex);
    if (g_io_error != 0) {
        fprintf(stderr, "I/O write error %d.\n", g_io_error);
        rc = g_io_error;
        goto exit;
    }

    rc = spdk_io_device_read_async(io_device, 0, g_data_read, SIZE_4K,
                                   io_callback, NULL);
    if (rc != 0) {
        fprintf(stderr, "I/O read error %d.\n", rc);
        goto exit;
    }
    sem_wait(&mutex);
    if (g_io_error != 0) {
        fprintf(stderr, "I/O read error %d.\n", g_io_error);
        rc = g_io_error;
        goto exit;
    }
    if (strcmp(g_data_buff, g_data_read) != 0) {
        fprintf(stderr, "excepted %s but got %s\n", g_data_buff, g_data_read);
        rc = -1;
        goto exit;
    }

exit:
    return rc;
}