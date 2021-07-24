#include "unmanaged.h"

static void* get_ptr(const char* name, size_t size)
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

    mqd_t queue = mq_open(QUEUE_NAME, O_RDWR, 0, NULL);
    if(queue == -1)
    {
        perror("mq_open");
        exit(1);
    }
    char* req = NULL;
    char* res = NULL;

    fd_set readfds;
    struct timeval timeout;
    char buf[0x100];
    char message[8192];
    int ret;
    parameter_t* pParam;
    sem_t* sem_response;
    sem_t* sem_ack;
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
        if(mq_receive(queue, message, sizeof(message), NULL) == -1)
        {
            perror("mq_receive");
            exit(1);
        }
        pParam = (parameter_t*)message;

        if(req == NULL & res == NULL)
        {
            shared_t* shared = get_ptr(pParam->shm_name, pParam->size);
            printf("%d %d\n", shared->width, shared->height);
            size_t size = shared->width * shared->height * sizeof(uint32_t);
            req = shared->pointer;
            res = req + size;
            sem_ack = &shared->sem_ack;
            sem_response = &shared->sem_response;
        }
        printf("received request. sending ACK.\n");
        sem_post(sem_ack);
        printf("request: %s\n", req);
        memmove(res, "abcde\0", 6);
        sem_post(sem_response);
    }
}
