/*  bbiosetup.h
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

#ifndef _BBIOSETUP_H
#define _BBIOSETUP_H

/* PCM Byte Stream Output */

typedef struct {
    char *author;
    char *title;
    char *copyright;
    char *abstract;
} PCMFileHeader;

typedef struct {
    unsigned long samples_per_second;
    unsigned long average_bit_rate;
    unsigned long channels;
    unsigned long bits_per_sample;
    unsigned long duration;
} PCMStreamHeader;

typedef struct {
    PCMStreamHeader header;
    unsigned long data_size;
    unsigned char *data_buffer;
} PCMDataBuffer;

typedef int (* BBOutputFileHeaderCallback)(const PCMFileHeader *header, void *userdata);
typedef void (* BBOutputStreamHeaderCallback)(const PCMStreamHeader *header, void *userdata);
typedef int (* BBOutputWriteCallback)(const PCMDataBuffer *buffer, void *userdata);
typedef void (* BBOutputStreamDoneCallback)(void *userdata);

typedef struct {
    BBOutputFileHeaderCallback FileHeader;
    BBOutputStreamHeaderCallback StreamHeader;
    BBOutputWriteCallback Write;
    BBOutputStreamDoneCallback StreamDone;
} BBOutputCallbacks;

/* Raw Byte Stream Input */

#define HX_FILESTATUS_PHYSICAL_EOF    (0)
#define HX_FILESTATUS_ERROR          (-1)
#define HX_FILESTATUS_LOGICAL_EOF  (-111)
#define HX_FILESTATUS_DATA_PENDING (-222)
#define HX_FILESTATUS_FATAL_ERROR  (-333)

typedef enum {
    BB_INPUT_TYPE_MP3,
    BB_INPUT_TYPE_AAC
} BB_INPUT_TYPE;

typedef int (* BBInputOpenCallback)(void *userdata);
typedef int (* BBInputStatCallback)(struct stat *buffer, void *userdata);
typedef long (* BBInputReadCallback)(unsigned char *buffer, unsigned long count, void *userdata);
typedef int (* BBInputSeekCallback)(unsigned long offset, unsigned short whence, void *userdata);
typedef int (* BBInputTellCallback)(void *userdata);
typedef int (* BBInputCloseCallback)(void *userdata);
typedef int (* BBInputIsOpenCallback)(void *userdata);

typedef struct {
    BBInputOpenCallback Open;
    BBInputStatCallback Stat;
    BBInputReadCallback Read;
    BBInputSeekCallback Seek;
    BBInputTellCallback Tell;
    BBInputCloseCallback Close;
    BBInputIsOpenCallback IsOpen;
} BBInputCallbacks;

#endif /* _BBIOSETUP_H */
