/* -*- Mode: C; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */

/*  xing-mp3-encoder.c - A GStreamer plugin wrapper for the Xing MP3 encoder.
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
 
#ifdef HAVE_CONFIG_H
# include "config.h"
#endif
#include <stdlib.h>
#include <string.h>
#include <time.h>

#include "xhead.h"

#include "gst-xing-encoder.h"

#define INPUT_BUFFER_SIZE  16384
#define OUTPUT_BUFFER_SIZE 8192

static GstPadTemplate *xing_mp3_encoder_src_template;
static GstPadTemplate *xing_mp3_encoder_sink_template;

GstElementDetails xing_mp3_encoder_details =
{
  "Xing MP3 Encoder",
  "Codec/Encoder/Audio",
  "Encodes audio to MP3 format",
  "Hans Petter Jansson <hpj@novell.com>"
};

enum
{
  LAST_SIGNAL
};

enum
{
  ARG_0,
  ARG_BITRATE,
  ARG_LAST_MESSAGE
};

#define BITRATE_DEFAULT -1

static const GstFormat *
xing_mp3_encoder_get_formats (GstPad *pad)
{
  static const GstFormat src_formats [] =
  {
    GST_FORMAT_BYTES,
    GST_FORMAT_TIME,
    0
  };
  static const GstFormat sink_formats [] =
  {
    GST_FORMAT_BYTES,
    GST_FORMAT_DEFAULT,
    GST_FORMAT_TIME,
    0
  };

  return (GST_PAD_IS_SRC (pad) ? src_formats : sink_formats);
}

static void                  xing_mp3_encoder_base_init    (gpointer g_class);
static void                  xing_mp3_encoder_class_init   (XingMp3EncoderClass *klass);
static void                  xing_mp3_encoder_init         (XingMp3Encoder *encoder);

static void                  xing_mp3_encoder_chain        (GstPad *pad, GstData *_data);
static gboolean              xing_mp3_encoder_setup        (XingMp3Encoder *encoder);

static void                  xing_mp3_encoder_get_property (GObject *object, guint prop_id, GValue * value, GParamSpec * pspec);
static void                  xing_mp3_encoder_set_property (GObject *object, guint prop_id, const GValue * value, GParamSpec * pspec);
static GstElementStateReturn xing_mp3_encoder_change_state (GstElement *element);

static GstElementClass *parent_class = NULL;

/*static guint xing_mp3_encoder_signals[LAST_SIGNAL] = { 0 }; */

GType
xing_mp3_encoder_get_type (void)
{
  static GType xing_mp3_encoder_type = 0;

  if (!xing_mp3_encoder_type)
  {
    static const GTypeInfo xing_mp3_encoder_info =
    {
      sizeof (XingMp3EncoderClass),
      xing_mp3_encoder_base_init,
      NULL,
      (GClassInitFunc) xing_mp3_encoder_class_init,
      NULL,
      NULL,
      sizeof (XingMp3Encoder),
      0,
      (GInstanceInitFunc) xing_mp3_encoder_init,
    };

    xing_mp3_encoder_type = g_type_register_static (GST_TYPE_ELEMENT, "XingMp3Encoder",
                                                    &xing_mp3_encoder_info, 0);
  }

  return xing_mp3_encoder_type;
}

static GstCaps *
mp3_caps_factory (void)
{
  return gst_caps_new_simple ("application/mp3", NULL);
}

static GstCaps *
raw_caps_factory (void)
{
  return gst_caps_new_simple ("audio/x-raw-int",
                              "endianness", G_TYPE_INT,         G_BYTE_ORDER,
                              "signed",     G_TYPE_BOOLEAN,     TRUE,
                              "width",      G_TYPE_INT,         16,
                              "depth",      G_TYPE_INT,         16,
                              "rate",       GST_TYPE_INT_RANGE, 8000, 50000,
                              "channels",   GST_TYPE_INT_RANGE, 1, 2, NULL);
}

static void
xing_mp3_encoder_base_init (gpointer g_class)
{
  GstElementClass *element_class = GST_ELEMENT_CLASS (g_class);
  GstCaps         *raw_caps;
  GstCaps         *mp3_caps;

  raw_caps = raw_caps_factory ();
  mp3_caps = mp3_caps_factory ();

  xing_mp3_encoder_sink_template = gst_pad_template_new ("sink", GST_PAD_SINK, GST_PAD_ALWAYS, raw_caps);
  xing_mp3_encoder_src_template  = gst_pad_template_new ("src", GST_PAD_SRC, GST_PAD_ALWAYS, mp3_caps);
  gst_element_class_add_pad_template (element_class, xing_mp3_encoder_sink_template);
  gst_element_class_add_pad_template (element_class, xing_mp3_encoder_src_template);
  gst_element_class_set_details (element_class, &xing_mp3_encoder_details);
}

static void
xing_mp3_encoder_class_init (XingMp3EncoderClass *klass)
{
  GObjectClass    *gobject_class;
  GstElementClass *gstelement_class;

  gobject_class    = (GObjectClass *) klass;
  gstelement_class = (GstElementClass *) klass;

  g_object_class_install_property (G_OBJECT_CLASS (klass), ARG_BITRATE,
                                   g_param_spec_int ("bitrate", "Target Bitrate",
                                                     "Specify a target constant bitrate (in bps).",
                                                     -1, G_MAXINT, BITRATE_DEFAULT, G_PARAM_READWRITE));
  g_object_class_install_property (G_OBJECT_CLASS (klass), ARG_LAST_MESSAGE,
                                   g_param_spec_string ("last-message", "last-message",
                                                        "The last status message", NULL, G_PARAM_READABLE));

  parent_class = g_type_class_ref (GST_TYPE_ELEMENT);

  gobject_class->set_property = xing_mp3_encoder_set_property;
  gobject_class->get_property = xing_mp3_encoder_get_property;

  gstelement_class->change_state = xing_mp3_encoder_change_state;
}

static GstPadLinkReturn
xing_mp3_encoder_link (GstPad *pad, const GstCaps *caps)
{
  XingMp3Encoder *encoder;
  GstStructure   *structure;

  encoder = XING_MP3_ENCODER (gst_pad_get_parent (pad));
  encoder->is_initialized = FALSE;

  structure = gst_caps_get_structure (caps, 0);
  gst_structure_get_int (structure, "channels", &encoder->channels);
  gst_structure_get_int (structure, "rate", &encoder->frequency);

  xing_mp3_encoder_setup (encoder);

  if (encoder->is_initialized)
    return GST_PAD_LINK_OK;

  return GST_PAD_LINK_REFUSED;
}

static gboolean
xing_mp3_encoder_convert_src (GstPad *pad, GstFormat src_format,
                              gint64 src_value, GstFormat *dest_format, gint64 *dest_value)
{
  gboolean        res = TRUE;
  XingMp3Encoder *encoder;
  gint64          avg;

  encoder = XING_MP3_ENCODER (gst_pad_get_parent (pad));

  if (encoder->samples_in == 0 || encoder->bytes_out == 0 || encoder->frequency == 0)
    return FALSE;

  avg = (encoder->bytes_out * encoder->frequency) / (encoder->samples_in);

  switch (src_format)
  {
    case GST_FORMAT_BYTES:
      switch (*dest_format)
      {
        case GST_FORMAT_TIME:
          *dest_value = src_value * GST_SECOND / avg;
          break;

        default:
          res = FALSE;
      }
      break;

    case GST_FORMAT_TIME:
      switch (*dest_format)
        {
        case GST_FORMAT_BYTES:
          *dest_value = src_value * avg / GST_SECOND;
          break;

        default:
          res = FALSE;
      }
      break;

    default:
      res = FALSE;
      break;
  }
  return res;
}

static gboolean
xing_mp3_encoder_convert_sink (GstPad *pad, GstFormat src_format,
                               gint64 src_value, GstFormat *dest_format, gint64 *dest_value)
{
  gboolean        res = TRUE;
  guint           scale = 1;
  gint            bytes_per_sample;
  XingMp3Encoder *encoder;

  encoder = XING_MP3_ENCODER (gst_pad_get_parent (pad));

  bytes_per_sample = encoder->channels * 2;

  switch (src_format)
  {
    case GST_FORMAT_BYTES:
      switch (*dest_format)
      {
        case GST_FORMAT_DEFAULT:
          if (bytes_per_sample == 0)
            return FALSE;
          *dest_value = src_value / bytes_per_sample;
          break;

        case GST_FORMAT_TIME:
        {
          gint byterate = bytes_per_sample * encoder->frequency;

          if (byterate == 0)
            return FALSE;
          *dest_value = src_value * GST_SECOND / byterate;
          break;
        }

        default:
          res = FALSE;
          break;
      }
      break;

    case GST_FORMAT_DEFAULT:
      switch (*dest_format)
      {
        case GST_FORMAT_BYTES:
          *dest_value = src_value * bytes_per_sample;
          break;

        case GST_FORMAT_TIME:
          if (encoder->frequency == 0)
            return FALSE;
          *dest_value = src_value * GST_SECOND / encoder->frequency;
          break;

        default:
          res = FALSE;
          break;
      }
      break;

    case GST_FORMAT_TIME:
      switch (*dest_format)
      {
        case GST_FORMAT_BYTES:
          scale = bytes_per_sample;
          /* fallthrough */

        case GST_FORMAT_DEFAULT:
          *dest_value = src_value * scale * encoder->frequency / GST_SECOND;
          break;

        default:
          res = FALSE;
          break;
      }
      break;

    default:
      res = FALSE;
      break;
  }

  return res;
}

static const GstQueryType *
xing_mp3_encoder_get_query_types (GstPad *pad)
{
  static const GstQueryType xing_mp3_encoder_src_query_types [] =
  {
    GST_QUERY_TOTAL,
    GST_QUERY_POSITION,
    0
  };

  return xing_mp3_encoder_src_query_types;
}

static gboolean
xing_mp3_encoder_src_query (GstPad *pad, GstQueryType type, GstFormat *format, gint64 *value)
{
  gboolean        res = TRUE;
  XingMp3Encoder *encoder;

  encoder = XING_MP3_ENCODER (gst_pad_get_parent (pad));

  switch (type)
  {
    case GST_QUERY_TOTAL:
    {
      switch (*format)
      {
        case GST_FORMAT_BYTES:
        case GST_FORMAT_TIME:
        {
          gint64           peer_value;
          const GstFormat *peer_formats;

          res = FALSE;
          peer_formats = gst_pad_get_formats (GST_PAD_PEER (encoder->sinkpad));

          while (peer_formats && *peer_formats && !res)
          {
            GstFormat peer_format = *peer_formats;

            if (gst_pad_query (GST_PAD_PEER (encoder->sinkpad),
                               GST_QUERY_TOTAL, &peer_format, &peer_value))
            {
              GstFormat conv_format;

              conv_format = GST_FORMAT_TIME;
              res = gst_pad_convert (encoder->sinkpad,  peer_format, peer_value,
                                     &conv_format, value);

              res &= gst_pad_convert (pad, GST_FORMAT_TIME, *value, format, value);
            }

            peer_formats++;
          }

          break;
        }

        default:
          res = FALSE;
          break;
      }
      break;
    }

    case GST_QUERY_POSITION:
      switch (*format)
      {
        default:
        {
          res = gst_pad_convert (pad, GST_FORMAT_BYTES, encoder->bytes_out, format, value);
          break;
        }
      }
      break;

    default:
      res = FALSE;
      break;
  }

  return res;
}

static void
xing_mp3_encoder_init (XingMp3Encoder *encoder)
{
  /* Sink pad */
  encoder->sinkpad = gst_pad_new_from_template (xing_mp3_encoder_sink_template, "sink");
  gst_pad_set_chain_function   (encoder->sinkpad, xing_mp3_encoder_chain);
  gst_pad_set_link_function    (encoder->sinkpad, xing_mp3_encoder_link);
  gst_pad_set_convert_function (encoder->sinkpad, GST_DEBUG_FUNCPTR (xing_mp3_encoder_convert_sink));
  gst_pad_set_formats_function (encoder->sinkpad, GST_DEBUG_FUNCPTR (xing_mp3_encoder_get_formats));
  gst_element_add_pad (GST_ELEMENT (encoder), encoder->sinkpad);

  /* Source pad */
  encoder->srcpad = gst_pad_new_from_template (xing_mp3_encoder_src_template, "src");
  gst_pad_set_query_function      (encoder->srcpad, GST_DEBUG_FUNCPTR (xing_mp3_encoder_src_query));
  gst_pad_set_query_type_function (encoder->srcpad, GST_DEBUG_FUNCPTR (xing_mp3_encoder_get_query_types));
  gst_pad_set_convert_function    (encoder->srcpad, GST_DEBUG_FUNCPTR (xing_mp3_encoder_convert_src));
  gst_pad_set_formats_function    (encoder->srcpad, GST_DEBUG_FUNCPTR (xing_mp3_encoder_get_formats));
  gst_element_add_pad (GST_ELEMENT (encoder), encoder->srcpad);

  encoder->channels         = -1;
  encoder->frequency        = -1;

  encoder->use_cbr          = FALSE;
  encoder->cbr_bitrate      = BITRATE_DEFAULT;
  encoder->last_message     = NULL;

  encoder->is_initialized   = FALSE;
  encoder->at_end_of_stream = FALSE;
  encoder->header_sent      = FALSE;

  GST_FLAG_SET (encoder, GST_ELEMENT_EVENT_AWARE);
}

static void
update_start_message (XingMp3Encoder *encoder)
{
  gchar *constraints;

  g_free (encoder->last_message);

  if (encoder->use_cbr)
  {
    if (encoder->cbr_bitrate != BITRATE_DEFAULT)
      encoder->last_message = g_strdup_printf ("Encoding at constant bitrate");
    else
      encoder->last_message = g_strdup_printf ("Encoding at constant bitrate of %d bps",
                                               encoder->cbr_bitrate);
  }
  else  /* vbr */
  {
    encoder->last_message = g_strdup_printf ("Encoding at variable bitrate");
  }

  g_object_notify (G_OBJECT (encoder), "last_message");
}

static gboolean
xing_mp3_encoder_setup (XingMp3Encoder *encoder)
{
  E_CONTROL *control;

  update_start_message (encoder);

  control = g_new0 (E_CONTROL, 1);

  if (encoder->channels == 1)
    control->mode                = 3;     /* mode-0 stereo=0 mode-1 stereo=1 dual=2 mono=3 */
  else /* encoder->channels == 2 */
    control->mode                = 0;     /* mode-0 stereo=0 mode-1 stereo=1 dual=2 mono=3 */
  control->samprate              = encoder->frequency;  /* sample rate e.g 44100 */
  control->nsbstereo             = -1;    /* mode-1 only, stereo bands, 3-32 , for default set =-1 */
  control->filter_select         = -1;    /* input filter selection - set to -1 for default */
  control->freq_limit            = 24000; /* special purpose, set to 24000 */
  control->nsb_limit             = -1;    /* special purpose, set to -1 */
  control->layer                 = 3;     /* 3=Layer III. Set to 3 */
  control->cr_bit                = 0;     /* header copyright bit setting */
  control->original              = 1;     /* header original/copy bit setting, original=1 */
  control->hf_flag               = 24000; /* MPEG1 high frequency */
  control->vbr_mnr               = 50;    /* 0-150, suggested setting is 50 */
  control->vbr_br_limit          = 160;   /* reserved for per chan vbr bitrate limit set to 160 */
  control->vbr_delta_mnr         = 0;     /* special, set 0 default */
  control->chan_add_f0           = 24000; /* channel adder start freq - set 24000 default */
  control->chan_add_f1           = 24000; /* channel adder final freq - set 24000 default */
  control->sparse_scale          = -1;    /* reserved set, to -1 (encoder chooses) */
  /* control->mnr_adjust[21]; */          /* special, set 0 default */
  control->cpu_select            = 0;     /* 0=generic, 1=3DNow reserved, 2=Pentium III */
  control->quick                 = -1;    /* 0 = base, 1 = fast, -1 = encoder chooses */
  control->test1                 = -1;    /* special test, set to -1 */
  control->test2                 = 0;     /* special test, set to 0 */
  control->test3                 = 0;     /* special test, set to 0 */
  control->short_block_threshold = 700;   /* short_block_threshold, default 700 */

  if (encoder->use_cbr)
  {
    control->vbr_flag = FALSE;

    if (encoder->cbr_bitrate != BITRATE_DEFAULT)
      control->bitrate = encoder->cbr_bitrate / 2;  /* Per channel bitrate in 1000s */
    else
      control->bitrate = -1;
  }
  else
  {
    control->vbr_flag = TRUE;  /* 1=vbr, 0=cbr  */
    control->bitrate = -1;     /* Per channel bitrate in 1000s, CBR only. -1 = default. */
  }

  encoder->xing_encoder = hx_mp3enc_new ();
  encoder->bytes_per_frame =
    hx_mp3enc_mp3_init (encoder->xing_encoder, control,
                        0 /* 16 bits per sample */,
                        0 /* upsample for... default */,
                        0 /* convert to mono? */);

  encoder->input_buffer = g_malloc (INPUT_BUFFER_SIZE);
  encoder->input_buffer_pos = 0;

  encoder->is_initialized = TRUE;

  return TRUE;
}

static void
write_out (XingMp3Encoder *encoder, gconstpointer buf, gint buf_len)
{
  GstBuffer *outbuf;

  outbuf = gst_buffer_new_and_alloc (buf_len);
  encoder->bytes_out += GST_BUFFER_SIZE (outbuf);

  memcpy (GST_BUFFER_DATA (outbuf), buf, buf_len);

  if (GST_PAD_IS_USABLE (encoder->srcpad))
    gst_pad_push (encoder->srcpad, GST_DATA (outbuf));
  else
    gst_buffer_unref (outbuf);
}

static void
xing_mp3_encoder_chain (GstPad *pad, GstData *_data)
{
  GstBuffer      *buf = GST_BUFFER (_data);
  XingMp3Encoder *encoder;

  g_return_if_fail (pad != NULL);
  g_return_if_fail (GST_IS_PAD (pad));
  g_return_if_fail (buf != NULL);

  encoder = XING_MP3_ENCODER (gst_pad_get_parent (pad));

  if (GST_IS_EVENT (buf))
  {
    GstEvent *event = GST_EVENT (buf);

    switch (GST_EVENT_TYPE (event))
    {
      case GST_EVENT_EOS:
        encoder->at_end_of_stream = TRUE;
        gst_event_unref (event);
        break;

      default:
        gst_pad_event_default (pad, event);
        return;
    }
  }
  else
  {
    guchar  *data;
    gulong   size;
    gulong   i, j;
    float  **buffer;
    gint     buf_size;
    gint     buf_pos;

    if (!encoder->is_initialized)
    {
      gst_buffer_unref (buf);
      GST_ELEMENT_ERROR (encoder, CORE, NEGOTIATION, (NULL), ("Encoder not initialized"));
      return;
    }

    if (!encoder->header_sent)
    {
      E_CONTROL control;
      MPEG_HEAD head;
      guchar    output_buffer [OUTPUT_BUFFER_SIZE];
      gint      buf_len;

      hx_mp3enc_l3_info_ec   (encoder->xing_encoder, &control);
      hx_mp3enc_l3_info_head (encoder->xing_encoder, &head);

      buf_len = XingHeader (control.samprate, head.mode, control.cr_bit, control.original,
                            VBR_SCALE_FLAG /* FRAMES_FLAG | BYTES_FLAG | TOC_FLAG */, 0, 0,
                            control.vbr_flag ? control.vbr_mnr : -1, NULL,
                            output_buffer, 0, 0, 0);
      write_out (encoder, output_buffer, buf_len);

      encoder->header_sent = TRUE;
    }

    data = (guchar *) GST_BUFFER_DATA (buf);
    buf_size = GST_BUFFER_SIZE (buf);
    buf_pos = 0;

    /* Transfer incoming data to internal buffer.
     * TODO: Use a ring buffer, avoid memmove () */

    while (buf_pos < buf_size)
    {
      gint gulp = MIN (buf_size - buf_pos, INPUT_BUFFER_SIZE - encoder->input_buffer_pos);

      memcpy (encoder->input_buffer + encoder->input_buffer_pos,
              data + buf_pos, gulp);

      encoder->samples_in += gulp / (2 * encoder->channels);

      encoder->input_buffer_pos += gulp;
      buf_pos += gulp;

      /* Pass data on to encoder */

      while (encoder->input_buffer_pos >= encoder->bytes_per_frame)
      {
        guchar output_buffer [OUTPUT_BUFFER_SIZE];
        IN_OUT x;

        x = hx_mp3enc_mp3_encode_frame (encoder->xing_encoder, encoder->input_buffer,
                                        output_buffer);

        if (x.in_bytes == 0 && x.out_bytes == 0)
          break;

        memmove (encoder->input_buffer, encoder->input_buffer + x.in_bytes,
                 encoder->input_buffer_pos - x.in_bytes);
        encoder->input_buffer_pos -= x.in_bytes;

        /* Accept output from encoder and pass it on.
         * TODO: Do this less often and save CPU */

        if (x.out_bytes > 0)
          write_out (encoder, output_buffer, x.out_bytes);
      }
    }

    gst_buffer_unref (buf);
  }

  if (encoder->at_end_of_stream)
  {
    gst_pad_push (encoder->srcpad, GST_DATA (gst_event_new (GST_EVENT_EOS)));
    gst_element_set_eos (GST_ELEMENT (encoder));
  }
}

static void
xing_mp3_encoder_get_property (GObject *object, guint prop_id, GValue *value, GParamSpec *pspec)
{
  XingMp3Encoder *encoder;

  g_return_if_fail (XING_IS_MP3_ENCODER (object));

  encoder = XING_MP3_ENCODER (object);

  switch (prop_id)
  {
    case ARG_BITRATE:
      g_value_set_int (value, encoder->cbr_bitrate);
      break;
      
    case ARG_LAST_MESSAGE:
      g_value_set_string (value, encoder->last_message);
      break;

    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
  }
}

static void
xing_mp3_encoder_set_property (GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
  XingMp3Encoder *encoder;

  g_return_if_fail (XING_IS_MP3_ENCODER (object));

  encoder = XING_MP3_ENCODER (object);

  switch (prop_id)
  {
    case ARG_BITRATE:
      encoder->cbr_bitrate = g_value_get_int (value);
      
      if (encoder->cbr_bitrate != -1 && encoder->cbr_bitrate < 2)
      {
        g_warning ("Encoding bitrate cannot be set lower than 2.");
        encoder->cbr_bitrate = 2;
      }
      
      encoder->use_cbr = encoder->cbr_bitrate != -1;
      break;
      
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
  }
}

static GstElementStateReturn
xing_mp3_encoder_change_state (GstElement * element)
{
  XingMp3Encoder *encoder = XING_MP3_ENCODER (element);

  switch (GST_STATE_TRANSITION (element))
  {
    case GST_STATE_NULL_TO_READY:
    case GST_STATE_READY_TO_PAUSED:
      encoder->at_end_of_stream = FALSE;
      break;

    case GST_STATE_PAUSED_TO_PLAYING:
    case GST_STATE_PLAYING_TO_PAUSED:
      break;

    case GST_STATE_PAUSED_TO_READY:
      encoder->is_initialized = FALSE;
      encoder->header_sent = FALSE;
      break;

    case GST_STATE_READY_TO_NULL:
    default:
      break;
  }

  if (GST_ELEMENT_CLASS (parent_class)->change_state)
    return GST_ELEMENT_CLASS (parent_class)->change_state (element);

  return GST_STATE_SUCCESS;
}

static gboolean
plugin_init (GstPlugin *plugin)
{
  return gst_element_register (plugin, XING_MP3_ENCODER_NAME,
                               GST_RANK_NONE,
                               XING_TYPE_MP3_ENCODER);
}

GST_PLUGIN_DEFINE (
  GST_VERSION_MAJOR,
  GST_VERSION_MINOR,
  XING_MP3_ENCODER_NAME,
  "Xing MP3 Encoder",
  plugin_init,
  VERSION,
  "LGPL",
  "Banshee",
  "http://banshee-project.org/"
)
