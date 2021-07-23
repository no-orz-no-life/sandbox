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

#include <errno.h>

#define WIDTH 1920
#define HEIGHT 1080

typedef struct {
    const char* name;
    size_t size;
    void* pointer;
} shared_memory;

typedef struct {
    unsigned int width;
    unsigned int height;
    sem_t sem_request;
    sem_t sem_ack;
    sem_t sem_response;
} parameter_t;

shared_memory parameter;
shared_memory request;
shared_memory response;

char* reqbuf = NULL;
char* resbuf = NULL;

static void allocate(shared_memory* p, const char* name, size_t size)
{
    p->name = name;
    p->size = size;
    int fd = shm_open(name, O_RDWR|O_CREAT, 0644);
    ftruncate(fd, size);
    if(fd == -1) {
        perror("shm_open");
        exit(1);
    }
    p->pointer = mmap(NULL, size, PROT_READ|PROT_WRITE, MAP_SHARED, fd, 0);
    close(fd);
}

static void deallocate(shared_memory* p)
{
    if(p->pointer != NULL)
    {
        munmap(p->pointer, p->size);
        p->pointer = NULL;
    }
    if(p->name != NULL)
    {
        shm_unlink(p->name);
        p->name = NULL;
    }
}

void cleanup()
{
    parameter_t* param = (parameter_t*)parameter.pointer;
    sem_destroy(&param->sem_ack);
    sem_destroy(&param->sem_request);
    sem_destroy(&param->sem_response);
    deallocate(&parameter);
    deallocate(&request);
    deallocate(&response);
}

void* get_ptr(const char* name, size_t size)
{
    int fd;
    fd = shm_open(name, O_RDWR, 0);
    printf("shm_open: %d\n", fd);

    void* ptr = mmap(NULL, size, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    printf("ptr: %p\n", ptr);
    close(fd);
    return ptr;
}
void client() {
    printf("client\n");

    parameter_t* param = (parameter_t*)get_ptr("frei0r.memorymap.parameter", sizeof(parameter_t));
    printf("%d %d\n", param->width, param->height);
    size_t size = param->width * param->height * sizeof(uint32_t);
    char* req = get_ptr("frei0r.memorymap.request", size);
    char* res = get_ptr("frei0r.memorymap.response", size);
    fd_set readfds;
    struct timeval timeout;
    char buf[0x100];
    int ret;
    while(1)
    {
        FD_ZERO(&readfds);
        FD_SET(0, &readfds);
        timeout.tv_sec = 0;
        timeout.tv_usec = 0;
        int nfds = select(1, &readfds, NULL, NULL, &timeout);
        if(nfds < 0)
        {
            perror("select");
            break;
        }
        if(nfds > 0)
        {
            if(fgets(buf, sizeof(buf) - 1, stdin) == NULL) break;
            if(strncasecmp(buf, "quit", 4) == 0) break;
        }
        ret = sem_trywait(&param->sem_request);
        if(ret == 0)
        {
            printf("received request. sending ACK.\n");
            sem_post(&param->sem_ack);
            printf("request: %s\n", req);
            memmove(res, "abcde\0", 6);
            sem_post(&param->sem_response);
        }
    }
}

void server() {
    printf("server\n");
    allocate(&parameter, "frei0r.memorymap.parameter", sizeof(parameter_t));
    parameter_t* param = (parameter_t*)parameter.pointer;
    printf("%p\n", param);
    param->width = WIDTH;
    param->height = HEIGHT;

    size_t size = WIDTH * HEIGHT * sizeof(uint32_t);

    allocate(&request, "frei0r.memorymap.request", size);
    allocate(&response, "frei0r.memorymap.response", size);
    reqbuf = request.pointer;
    resbuf = response.pointer;

    if(sem_init(&param->sem_ack, 1, 0))
    {
        perror("sem_init(ack)");
        exit(1);
    }
    if(sem_init(&param->sem_request, 1, 0))
    {
        perror("sem_init(request)");
        exit(1);
    }
    if(sem_init(&param->sem_response, 1, 0))
    {
        perror("sem_init(response)");
        exit(1);
    }
    fd_set readfds;
    struct timeval timeout;
    char buf[0x100];
    int ret;
    while(1)
    {
        FD_ZERO(&readfds);
        FD_SET(0, &readfds);
        timeout.tv_sec = 0;
        timeout.tv_usec = 0;
        int nfds = select(1, &readfds, NULL, NULL, &timeout);
        if(nfds < 0)
        {
            perror("select");
            break;
        }
        if(nfds > 0)
        {
            if(fgets(buf, sizeof(buf) - 1, stdin) == NULL) break;
            if(strncasecmp(buf, "quit", 4) == 0) break;
            
            strcpy(reqbuf, buf);
            sem_post(&param->sem_request);
            struct timespec timeout_ack;
            timespec_get(&timeout_ack, TIME_UTC);
            timeout_ack.tv_sec ++;

            printf("wait for ACK....\n");
            ret = sem_timedwait(&param->sem_ack, &timeout_ack);
            if(ret == 0)
            {
                printf("received ACK.\n");
            }
            else if(ret < 0)
            {
                if(errno == ETIMEDOUT)
                {
                    printf("ack timed out.\n");
                    // reset req
                    sem_trywait(&param->sem_request);
                }
                else
                {
                    perror("sem_timedwait(ack)");
                    break;
                }
            }
        }
        ret = sem_trywait(&param->sem_response);
        if(ret == 0)
        {
            // response received.
            printf("response: %s\n", resbuf);
        }
        else if(errno != EAGAIN)
        {
            perror("sem_trywait(response)");
            break;
        }
    }
}
int main(int argc, char* argv[])
{
    printf("sizeof(void*) = %ld\n", sizeof(void*));
    printf("sizeof(size_t) = %ld\n", sizeof(size_t));
    printf("sizeof(off_t) = %ld\n", sizeof(off_t));
    printf("sizeof(time_t) = %ld\n", sizeof(time_t));
    printf("sizeof(sem_t) = %ld\n", sizeof(sem_t));
    printf("sizeof(parameter_t) = %ld\n", sizeof(parameter_t));
    if(argc > 1) {
        client();
    } else {
        atexit(cleanup);
        server();
    }
    printf("exit.\n");
}