/***************************************************************************
 *  hxmessagesegment.h
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
 
#ifndef _HXMESSAGESEGMENT_H
#define _HXMESSAGESEGMENT_H

#include <glib.h>

typedef struct HxMessageSegment HxMessageSegment;

typedef enum {
    HX_MESSAGE_SEGMENT_NONE = 0,
    HX_MESSAGE_SEGMENT_INT32,
    HX_MESSAGE_SEGMENT_UINT32,
    HX_MESSAGE_SEGMENT_STRING
} HxMessageSegmentType;

HxMessageSegment *hxmessage_segment_new(HxMessageSegmentType type, const gchar *name, gpointer value);
HxMessageSegment *hxmessage_segment_new_string(const gchar *name, const gchar *value);
HxMessageSegment *hxmessage_segment_new_int32(const gchar *name, gint value);
HxMessageSegment *hxmessage_segment_new_uint32(const gchar *name, guint value);
void hxmessage_segment_free(HxMessageSegment *segment);
const gchar *hxmessage_segment_get_name(HxMessageSegment *segment);
HxMessageSegmentType hxmessage_segment_get_type(HxMessageSegment *segment);
guint hxmessage_segment_get_uint32(HxMessageSegment *segment);
gint hxmessage_segment_get_int32(HxMessageSegment *segment);
const gchar *hxmessage_segment_get_string(HxMessageSegment *segment);
gpointer hxmessage_segment_get_value(HxMessageSegment *segment);
void hxmessage_segment_print(HxMessageSegment *segment);

#endif /* _HXMESSAGESEGMENT_H */
