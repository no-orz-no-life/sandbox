#include "unmanaged.h"
#define WIDTH 1920
#define HEIGHT 1080

static mqd_t queue = -1;

static shared_t* share = NULL;
static int share_size;

#include <uuid/uuid.h>

static void cleanup()
{
    if(queue == -1)
    {
        mq_close(queue);
        mq_unlink(QUEUE_NAME);
    }
    if(share != NULL)
    {
        sem_destroy(&share->sem_ack);
        sem_destroy(&share->sem_response);
        munmap(share, share_size);
        share = NULL;
        shm_unlink(SHAREDMEMORY_NAME);
    }
}

void hexdump(void* p, int offset, int size)
{
    char* ptr = p;
    for(int i = 0; i < size; i++)
    {
        printf("%02x ", ptr[i + offset]);
        if(i % 16 == 0)
        {
            printf("\n");
        }
    }
}

static void server() {
    printf("server\n");
    
    size_t size = WIDTH * HEIGHT * sizeof(uint32_t);
    share_size = sizeof(shared_t) + size * 2;

    int fd = shm_open(SHAREDMEMORY_NAME, O_RDWR|O_CREAT, 0644);
    ftruncate(fd, share_size);
    if(fd == -1) {
        perror("shm_open");
        exit(1);
    }
    share = mmap(NULL, share_size, PROT_READ|PROT_WRITE, MAP_SHARED, fd, 0);
    close(fd);

    share->width = WIDTH;
    share->height = HEIGHT;
    char* reqbuf = share->pointer;
    char* resbuf = share->pointer + size;

    queue = mq_open(QUEUE_NAME, O_RDWR|O_CREAT, 0600, NULL);
    if(queue == -1)
    {
        perror("mq_open");
        exit(1);
    }
    if(sem_init(&share->sem_ack, 1, 0))
    {
        perror("sem_init(ack)");
        exit(1);
    }
    if(sem_init(&share->sem_response, 1, 0))
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
            parameter_t param;
            strcpy(param.shm_name, SHAREDMEMORY_NAME);
            param.size = share_size;

            mq_send(queue, (char*)&param, sizeof(parameter_t), 0);

            struct timespec timeout_ack;
            timespec_get(&timeout_ack, TIME_UTC);
            timeout_ack.tv_sec ++;

            printf("wait for ACK....\n");
            ret = sem_timedwait(&share->sem_ack, &timeout_ack);
            if(ret == 0)
            {
                printf("received ACK.\n");
            }
            else if(ret < 0)
            {
                if(errno == ETIMEDOUT)
                {
                    printf("ack timed out.\n");
                }
                else
                {
                    perror("sem_timedwait(ack)");
                    break;
                }
            }
        }
        ret = sem_trywait(&share->sem_response);
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

extern void client();

int main(int argc, char* argv[])
{
    printf("sizeof(void*) = %ld\n", sizeof(void*));
    printf("sizeof(size_t) = %ld\n", sizeof(size_t));
    printf("sizeof(off_t) = %ld\n", sizeof(off_t));
    printf("sizeof(time_t) = %ld\n", sizeof(time_t));
    printf("sizeof(sem_t) = %ld\n", sizeof(sem_t));
    printf("sizeof(parameter_t) = %ld\n", sizeof(parameter_t));
    printf("sizeof(ssize_t) = %ld\n", sizeof(ssize_t));

    uuid_t uuid;
    uuid_generate(uuid);
    char buf[64];
    uuid_unparse(uuid, buf);
    printf("UUID: %s\n", buf);
    if(argc > 1) {
        client();
    } else {
        atexit(cleanup);
        server();
    }
    printf("exit.\n");
}