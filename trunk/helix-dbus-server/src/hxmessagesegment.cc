/***************************************************************************
 *  hxmessagesegment.cc
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
#include <glib.h>

#include "hxmessagesegment.h"

struct HxMessageSegment {
    HxMessageSegmentType type;
    gchar *name;
    gpointer value;
};

HxMessageSegment *
hxmessage_segment_new(HxMessageSegmentType type, const gchar *name, gpointer value)
{
    HxMessageSegment *segment = g_new(HxMessageSegment, 1);
    
    if(segment == NULL) {
        return NULL;
    }
    
    segment->name = g_strdup(name);
    segment->type = type;
    segment->value = type == HX_MESSAGE_SEGMENT_STRING ? g_strdup((const gchar *)value) : value;
    
    return segment;
}

HxMessageSegment *
hxmessage_segment_new_string(const gchar *name, const gchar *value)
{
    return hxmessage_segment_new(HX_MESSAGE_SEGMENT_STRING, name, (gpointer)value);
}

HxMessageSegment *
hxmessage_segment_new_int32(const gchar *name, gint value)
{
    return hxmessage_segment_new(HX_MESSAGE_SEGMENT_INT32, name, (gpointer)value);
}

HxMessageSegment *
hxmessage_segment_new_uint32(const gchar *name, guint value)
{
    return hxmessage_segment_new(HX_MESSAGE_SEGMENT_UINT32, name, (gpointer)value);
}

void
hxmessage_segment_free(HxMessageSegment *segment)
{
    if(segment == NULL) {
        return;
    }
    
    if(segment->name != NULL) {
        g_free(segment->name);
        segment->name = NULL;
    }
    
    if(segment->type == HX_MESSAGE_SEGMENT_STRING && segment->value != NULL) {
        g_free(segment->value);
    }

    segment->value = NULL;
    segment->type = HX_MESSAGE_SEGMENT_NONE;
    
    g_free(segment);
    segment = NULL;
}

HxMessageSegmentType 
hxmessage_segment_get_type(HxMessageSegment *segment)
{
    if(segment == NULL) {
        return HX_MESSAGE_SEGMENT_NONE;
    }
    
    return segment->type;
}

const gchar *
hxmessage_segment_get_name(HxMessageSegment *segment)
{
    if(segment == NULL) {
        return NULL;
    }
    
    return (const gchar *)segment->name;
}

gpointer
hxmessage_segment_get_value(HxMessageSegment *segment)
{
    if(segment == NULL) {
        return NULL;
    }
    
    return segment->value;
}

const gchar *
hxmessage_segment_get_string(HxMessageSegment *segment)
{
    if(segment == NULL || segment->type != HX_MESSAGE_SEGMENT_STRING) {
        return NULL;
    }
    
    return (const gchar *)segment->value;
}

gint
hxmessage_segment_get_int32(HxMessageSegment *segment)
{
    if(segment == NULL || segment->type != HX_MESSAGE_SEGMENT_INT32) {
        return 0;
    }
    
    return (gint)segment->value;
}

guint
hxmessage_segment_get_uint32(HxMessageSegment *segment)
{
    if(segment == NULL || segment->type != HX_MESSAGE_SEGMENT_UINT32) {
        return 0;
    }
    
    return (guint)segment->value;
}

void
hxmessage_segment_print(HxMessageSegment *segment)
{
    if(segment == NULL) {
        return;
    }
    
    printf("    %s: ", segment->name);
    
    switch(segment->type) {
        case HX_MESSAGE_SEGMENT_INT32:
            printf("%d (INT32)\n", (gint)segment->value);
            break;
        case HX_MESSAGE_SEGMENT_UINT32:
            printf("%d (UINT32)\n", (guint)segment->value);
            break;
        case HX_MESSAGE_SEGMENT_STRING:
            printf("%s (STRING)\n", (const gchar *)segment->value);
            break;
        case HX_MESSAGE_SEGMENT_NONE:
        default:
            printf("(NONE)\n");
            break;
    }
}

