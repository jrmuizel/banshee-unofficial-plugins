/***************************************************************************
 *  hxmessage.h
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
 
#ifndef _HXMESSAGE_H
#define _HXMESSAGE_H

#include <glib.h>
#include <stdarg.h>

#include "hxmessagesegment.h"

typedef struct HxMessage HxMessage;

typedef enum {
    HX_MESSAGE_NONE = 0,
    HX_MESSAGE_VISUAL_STATE,
    HX_MESSAGE_IDEAL_SIZE,
    HX_MESSAGE_LENGTH,
    HX_MESSAGE_TITLE,
    HX_MESSAGE_GROUPS,
    HX_MESSAGE_GROUP_STARTED,
    HX_MESSAGE_CONTACTING,
    HX_MESSAGE_BUFFERING,
    HX_MESSAGE_CONTENT_CONCLUDED,
    HX_MESSAGE_CONTENT_STATE,
    HX_MESSAGE_STATUS,
    HX_MESSAGE_VOLUME,
    HX_MESSAGE_MUTE,
    HX_MESSAGE_CLIP_BANDWIDTH,
    HX_MESSAGE_ERROR,
    HX_MESSAGE_GOTO_URL,
    HX_MESSAGE_REQUEST_AUTHENTICATION,
    HX_MESSAGE_REQUEST_UPGRADE,
    HX_MESSAGE_HAS_COMPONENT
} HxMessageType;

HxMessage *hxmessage_new(HxMessageType type);
HxMessage *hxmessage_new_args(HxMessageType type, HxMessageSegment *segment, ...);
void hxmessage_free(HxMessage *message);

HxMessageType hxmessage_get_type(HxMessage *message);
GList *hxmessage_get_segment_list(HxMessage *message);

void hxmessage_append_segment(HxMessage *message, HxMessageSegment *segment);
void hxmessage_print(HxMessage *message);

#endif /* _HXMESSAGE_H */
