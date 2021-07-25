all: unmanaged

unmanaged: unmanaged.o client.o
	$(CC) -o unmanaged unmanaged.o client.o -lpthread -lrt -luuid

clean:
	rm -f unmanaged unmanaged.o client.o

.c.o:
	$(CC) -c $<
