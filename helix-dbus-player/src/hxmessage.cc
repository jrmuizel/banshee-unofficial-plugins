/***************************************************************************
 *  hxmessage.cc
 *
 *  Copyright (C) 2006 Novell, Inc
 *  Written by Aaron Bockover <aaron@abock.org>
 ****************************************************************************/

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
 
#include <memory.h>
#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>
#include <glib.h>

#include "hxmessage.h"

struct HxMessage {
    HxMessageType type;
    GList *args;
};

static const gchar * HX_MESSAGE_TYPES [] = {
    "HX_MESSAGE_NONE",
    "HX_MESSAGE_VISUAL_STATE",
    "HX_MESSAGE_IDEAL_SIZE",
    "HX_MESSAGE_LENGTH",
    "HX_MESSAGE_TITLE",
    "HX_MESSAGE_GROUPS",
    "HX_MESSAGE_GROUP_STARTED",
    "HX_MESSAGE_CONTACTING",
    "HX_MESSAGE_BUFFERING",
    "HX_MESSAGE_CONTENT_CONCLUDED",
    "HX_MESSAGE_CONTENT_STATE",
    "HX_MESSAGE_STATUS",
    "HX_MESSAGE_VOLUME",
    "HX_MESSAGE_MUTE",
    "HX_MESSAGE_CLIP_BANDWIDTH",
    "HX_MESSAGE_ERROR",
    "HX_MESSAGE_GOTO_URL",
    "HX_MESSAGE_REQUEST_AUTHENTICATION",
    "HX_MESSAGE_REQUEST_UPGRADE",
    "HX_MESSAGE_HAS_COMPONENT",
    NULL
};

HxMessage *
hxmessage_new(HxMessageType type)
{
    HxMessage *message = g_new0(HxMessage, 1);
    
    if(message == NULL) {
        return NULL;
    }
    
    message->type = type;
    message->args = NULL;
    
    return message;
}

HxMessage *
hxmessage_new_args(HxMessageType type, HxMessageSegment *segment, ...)
{
    HxMessage *message;
    HxMessageSegment *next_segment;
    va_list argp;
    
    if(segment == NULL) {
        return NULL;
    }
    
    message = hxmessage_new(type);

    va_start(argp, segment);
    
    hxmessage_append_segment(message, segment);
    
    while((next_segment = va_arg(argp, HxMessageSegment *)) != NULL) {
        hxmessage_append_segment(message, next_segment);
    }
    
    va_end(argp);
    
    return message;
}

static void
hxmessage_free_segment(gpointer segment, gpointer user_data)
{
    hxmessage_segment_free((HxMessageSegment *)segment);
}

void 
hxmessage_free(HxMessage *message)
{
    if(message == NULL) {
        return;
    }
    
    if(message->args != NULL) {
        g_list_foreach(message->args, hxmessage_free_segment, NULL);
        g_list_free(message->args);
        message->args = NULL;
    }
    
    g_free(message);
    message = NULL;
}

HxMessageType 
hxmessage_get_type(HxMessage *message)
{
    if(message == NULL) {
        return HX_MESSAGE_NONE;
    }
    
    return message->type;
}

GList *
hxmessage_get_segment_list(HxMessage *message)
{
    if(message == NULL) {
        return NULL;
    }
    
    return message->args;
}

void
hxmessage_append_segment(HxMessage *message, HxMessageSegment *segment)
{
    if(message == NULL || segment == NULL) {
        return;
    }
    
    message->args = g_list_append(message->args, segment);
}

void
hxmessage_print(HxMessage *message)
{
    gint count, i;
    
    if(message == NULL) {
        return;
    }
    
    printf("Message Type: %s\n", HX_MESSAGE_TYPES[message->type]);
    
    for(i = 0, count = g_list_length(message->args); i < count; i++) {
        HxMessageSegment *segment = (HxMessageSegment *)g_list_nth_data(message->args, i);
        if(segment == NULL) {
            break;
        }
        
        hxmessage_segment_print(segment);
    }
}
