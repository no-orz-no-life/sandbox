#include <stdio.h>
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>


int main(void)
{
    int ret;

    av_log_set_level(AV_LOG_TRACE);
    av_log_set_flags(AV_LOG_PRINT_LEVEL);
    AVDictionary *opts = NULL;
    AVFormatContext* format =  avformat_alloc_context();
    format->interrupt_callback.callback = NULL;
    format->interrupt_callback.opaque = NULL;

    AVInputFormat* inputFormat = av_find_input_format("video4linux2");
    printf("inputFormat: %p\n", inputFormat);

    //av_dict_set(&opts, "truncate", "0", 0);
    //av_dict_set(&opts, "seekable", "0", 0);
    av_dict_set(&opts, "video_size", "640x480", 0);
    av_dict_set(&opts, "input_format", "YUYV", 0);

    ret = avformat_open_input(&format, "/dev/video0", inputFormat, &opts);
    if (ret != 0)
    {
        char errbuf[0x1000];
        av_make_error_string(errbuf, sizeof(errbuf), ret);
        printf("ret = %d. %s", ret, errbuf);
    }
    else 
    {
        printf("OK.\n");
    }
    av_dict_free(&opts);
}