all: testv4l2

LIBS+=-lavformat -lavutil -lavcodec -lswresample -lavdevice -lavfilter

clean:
	rm -f *.o testv4l2

testv4l2: testv4l2.o
	$(CC) $(LDFLAGS) -o testv4l2 testv4l2.o -L. -Wl,-rpath,. $(LIBS)

.c.o:
	$(CC) -c $(CFLAGS) $<
