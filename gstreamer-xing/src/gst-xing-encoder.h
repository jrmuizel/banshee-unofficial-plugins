/* -*- Mode: C; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */

/*  xing-mp3-encoder.h - A GStreamer plugin wrapper for the Xing MP3 encoder.
 *
 *  Copyright (C) 2005 Novell, Inc.
 *  Written by Hans Petter Jansson <hpj@novell.com> */

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
 
#ifndef __XING_MP3_ENCODER_H__
#define __XING_MP3_ENCODER_H__

#include <gst/gst.h>
#include "encapp.h"

G_BEGIN_DECLS

#define XING_MP3_ENCODER_NAME "xingenc"

#define XING_TYPE_MP3_ENCODER          (xing_mp3_encoder_get_type ())
#define XING_MP3_ENCODER(obj)          (G_TYPE_CHECK_INSTANCE_CAST ((obj), XING_TYPE_MP3_ENCODER, XingMp3Encoder))
#define XING_MP3_ENCODER_CLASS(klass)  (G_TYPE_CHECK_CLASS_CAST((klass), XING_TYPE_MP3_ENCODER, XingMp3EncoderClass))
#define XING_IS_MP3_ENCODER(obj)       (G_TYPE_CHECK_INSTANCE_TYPE((obj), XING_TYPE_MP3_ENCODER))
#define XING_IS_MP3_ENCODER_CLASS(obj) (G_TYPE_CHECK_CLASS_TYPE((klass), XING_TYPE_MP3_ENCODER))

typedef struct _XingMp3Encoder      XingMp3Encoder;
typedef struct _XingMp3EncoderClass XingMp3EncoderClass;

struct _XingMp3Encoder
{
  /* Everything in this struct is private */

  GstElement  element;

  GstPad     *sinkpad;
  GstPad     *srcpad;

  XMp3Enc    *xing_encoder;

  /* We need to hand off data to the encoder in appropriately-sized chunks.
   * Therefore, we must buffer our input. */
  guchar     *input_buffer;
  gint        input_buffer_pos;
  gint        bytes_per_frame;

  /* Input settings */
  gint        channels;
  gint        frequency;

  /* Output settings */
  gboolean    use_cbr;      /* TRUE = Constant bitrate, FALSE = variable bitrate */
  gint        cbr_bitrate;  /* Bitrate, for CBR only */

  /* Statistics */
  guint64     samples_in;
  guint64     bytes_out;

  /* Internal state */
  guint       at_end_of_stream : 1;
  guint       is_initialized   : 1;
  guint       header_sent      : 1;

  gchar	     *last_message;
};

struct _XingMp3EncoderClass
{
  GstElementClass parent_class;
};

GType xing_mp3_encoder_get_type (void);

G_END_DECLS

#endif /* __XING_MP3_ENCODER_H__ */
