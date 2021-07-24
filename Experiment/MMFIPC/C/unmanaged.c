#include "unmanaged.h"
#define WIDTH 1920
#define HEIGHT 1080

static mqd_t queue = -1;

typedef struct {
    const char* name;
    size_t size;
    void* pointer;
} shared_memory;

static parameter_t param;

static shared_memory request;
static shared_memory response;

static char* reqbuf = NULL;
static char* resbuf = NULL;

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

static void cleanup()
{
    sem_destroy(&param.sem_ack);
    sem_destroy(&param.sem_response);
    deallocate(&request);
    deallocate(&response);
    if(queue == -1)
    {
        mq_close(queue);
        mq_unlink(QUEUE_NAME);
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
    param.width = WIDTH;
    param.height = HEIGHT;

    size_t size = WIDTH * HEIGHT * sizeof(uint32_t);

    allocate(&request, "frei0r.memorymap.request", size);
    allocate(&response, "frei0r.memorymap.response", size);
    reqbuf = request.pointer;
    resbuf = response.pointer;

    queue = mq_open(QUEUE_NAME, O_RDWR|O_CREAT, 0600, NULL);
    if(queue == -1)
    {
        perror("mq_open");
        exit(1);
    }
    if(sem_init(&param.sem_ack, 1, 0))
    {
        perror("sem_init(ack)");
        exit(1);
    }
    if(sem_init(&param.sem_response, 1, 0))
    {
        perror("sem_init(response)");
        exit(1);
    }

    printf("sem_ack: \n"); hexdump(&param.sem_ack, 0, sizeof(sem_t));
    printf("\nsem_response: \n"); hexdump(&param.sem_response, 0, sizeof(sem_t));
    printf("\n");

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
            strcpy(param.shm_name_req, "frei0r.memorymap.request");
            strcpy(param.shm_name_res, "frei0r.memorymap.response");

            mq_send(queue, (char*)&param, sizeof(parameter_t), 0);

            struct timespec timeout_ack;
            timespec_get(&timeout_ack, TIME_UTC);
            timeout_ack.tv_sec ++;

            printf("wait for ACK....\n");
            ret = sem_timedwait(&param.sem_ack, &timeout_ack);
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
        ret = sem_trywait(&param.sem_response);
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
    if(argc > 1) {
        client();
    } else {
        atexit(cleanup);
        server();
    }
    printf("exit.\n");
}