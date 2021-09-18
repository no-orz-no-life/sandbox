all: testv4l2

LIBS+=-lavformat -lavutil -lavcodec

testv4l2: testv4l2.o
	$(CC) $(LDFLAGS) -o testv4l2 testv4l2.o $(LIBS)

.c.o:
	$(CC) -c $(CFLAGS) $<
