#ifndef __UNMANAGED_H
#define __UNMANAGED_H
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <unistd.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <fcntl.h>

#include <sys/select.h>
#include <sys/time.h>
#include <sys/types.h>

#include <pthread.h>
#include <semaphore.h>
#include <mqueue.h>

#include <errno.h>

#define QUEUE_NAME "/frei0r.memorymap.mq"
#define SHAREDMEMORY_NAME "/frei0r.memorymap.shmem"

extern void hexdump(void* p, int offset, int size);

typedef struct {
    unsigned int width;
    unsigned int height;
    sem_t sem_ack;
    sem_t sem_response;
    char pointer[0];
} shared_t;

typedef struct {
    size_t size;
    char shm_name[256];
} parameter_t;

#endif
