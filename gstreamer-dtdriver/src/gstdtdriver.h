/*  gstdtdriver.h
 *
 *  Copyright (C) 2005-2006 Novell, Inc.
 *  Written by Aaron Bockover <aaron@abock.org> 
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
 
#ifndef __GST_DTDRIVER_H__
#define __GST_DTDRIVER_H__

#include <gst/gst.h>

#include "hxfftdriver.h"

G_BEGIN_DECLS

#define DTDRIVER_DECODER_NAME "dtdrdec"

#define GST_TYPE_DTDRIVER (gst_dtdriver_get_type())
#define GST_DTDRIVER(obj) (G_TYPE_CHECK_INSTANCE_CAST((obj),GST_TYPE_DTDRIVER,GstDTDriver))
#define GST_DTDRIVER_CLASS(klass) (G_TYPE_CHECK_CLASS_CAST((klass),GST_TYPE_DTDRIVER,GstDTDriver))
#define GST_IS_DTDRIVER(obj) (G_TYPE_CHECK_INSTANCE_TYPE((obj),GST_TYPE_DTDRIVER))
#define GST_IS_DTDRIVER_CLASS(obj) (G_TYPE_CHECK_CLASS_TYPE((klass),GST_TYPE_DTDRIVER))

typedef struct _GstDTDriver GstDTDriver;
typedef struct _GstDTDriverClass GstDTDriverClass;

struct _GstDTDriver {
    GstElement element;
    HXFFTDriver *decoder;
    
    const gchar *codec;

    GstPad *sinkpad;
    GstPad *srcpad;

    gboolean init;
    gboolean need_newsegment;

    guint64 offset;

    GstSegment segment;
    GstFlowReturn last_flow;
    
    gint channels;
    gint depth;
    gint width;
    gint sample_rate;
    gint64 duration;
};

struct _GstDTDriverClass {
    GstElementClass parent_class;
};

GType gst_dtdriver_get_type(void);

G_END_DECLS

#endif /* __GST_DTDRIVER_H__ */
