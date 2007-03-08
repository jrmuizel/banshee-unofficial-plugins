/*  main.cpp
 *
 *  Copyright (C) 2006 Novell, Inc.
 *  Written by Aaron Bockover <abockover@novell.com> 
 */

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>

#include "hxfftdriver.h"

static int fd = -1;
static const char *in_file = NULL;
static const char *out_file = NULL;

static int input_open(void *userdata)
{
    fd = open(in_file, O_RDONLY);
    return fd > 0 ? 1 : 0;
}

static int input_close(void *userdata)
{
    close(fd);
    fd = -1;
    return 1;
}

static int input_isopen(void *userdata)
{
    return fd > 0;
}

static int input_seek(unsigned long offset, unsigned short whence, void *userdata)
{
    lseek(fd, offset, whence);
    return 1;
}

static size_t input_read(unsigned char *buffer, unsigned long count, void *userdata)
{
    return read(fd, buffer, count);
}

static int output_open(void *userdata)
{
    return 1;
}

static void output_stream_header(const PCMStreamHeader *header, void *userdata)
{

}

static int output_write(unsigned char *buffer, unsigned long count, void *userdata)
{
    return 0;
}

static void output_stream_done(void *userdata)
{
}

static BBInputCallbacks input_callbacks = {
    input_open,
    input_read,
    input_seek,
    input_close,
    input_isopen
};

static BBOutputCallbacks output_callbacks = {
    output_open,
    output_stream_header,
    output_write,
    output_stream_done
};

int main(int argc, char **argv)
{
    hx_set_dll_access_paths("/usr/lib/RealPlayer10/plugins", "/usr/lib/RealPlayer10/codecs");

    in_file = argv[1];
    out_file = argv[2];
    
    HXFFTDriver *driver = hxfftdriver_new(NULL);
    
    if(!hxfftdriver_initialize_external_io(driver, "Novell", "Banshee", 
        &input_callbacks, &output_callbacks)) {
        printf("Could not initialize\n");
        exit(1);
    }
    
    printf("Transcoder initialized ok\n");
    printf("Starting transcode\n");
    
    hxfftdriver_drive_external_io(driver, BB_INPUT_TYPE_MP3);
    
    printf("Transcode finished\n");
    
    hxfftdriver_free(driver);
    
    return 0;
}

