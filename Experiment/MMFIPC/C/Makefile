all: unmanaged

unmanaged: unmanaged.o
	$(CC) -o unmanaged unmanaged.o -lpthread -lrt 

clean:
	rm -f unmanaged unmanaged.o

.c.o:
	$(CC) -c $<
