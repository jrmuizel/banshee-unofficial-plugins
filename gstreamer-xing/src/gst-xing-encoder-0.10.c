/* -*- Mode: C; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */

/*  xing-mp3-encoder-0.10.c - GStreamer 0.10 plugin for the Xing MP3 encoder.
 *
 *  Copyright (C) 2005-2006 Novell, Inc.
 *  Written by Hans Petter Jansson <hpj@novell.com>
 *             Aaron Bockover <abockover@novell.com> 
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

GstElementDetails xing_mp3_encoder_details = {
    "Xing MP3 Encoder",
    "Codec/Encoder/Audio",
    "Encodes audio to MP3 format",
    "Hans Petter Jansson <hpj@novell.com>, Aaron Bockover <abockover@novell.com>"
};

enum {
    LAST_SIGNAL
};

enum {
    ARG_0,
    ARG_BITRATE,
    ARG_LAST_MESSAGE
};

#define BITRATE_DEFAULT -1

static void                  xing_mp3_encoder_base_init    (gpointer g_class);
static void                  xing_mp3_encoder_class_init   (XingMp3EncoderClass *klass);
static void                  xing_mp3_encoder_init         (XingMp3Encoder *encoder);

static GstFlowReturn         xing_mp3_encoder_chain        (GstPad *pad, GstBuffer *buf);
static gboolean              xing_mp3_encoder_setup        (XingMp3Encoder *encoder);

static void                  xing_mp3_encoder_get_property (GObject *object, guint prop_id, GValue * value, GParamSpec * pspec);
static void                  xing_mp3_encoder_set_property (GObject *object, guint prop_id, const GValue * value, GParamSpec * pspec);
static GstStateChangeReturn  xing_mp3_encoder_change_state (GstElement *element, GstStateChange transition);

static GstElementClass *parent_class = NULL;

/*static guint xing_mp3_encoder_signals[LAST_SIGNAL] = { 0 }; */

GType
xing_mp3_encoder_get_type()
{
    static GType xing_mp3_encoder_type = 0;

    if(!xing_mp3_encoder_type) {
        static const GTypeInfo xing_mp3_encoder_info = {
            sizeof(XingMp3EncoderClass),
            xing_mp3_encoder_base_init,
            NULL,
            (GClassInitFunc)xing_mp3_encoder_class_init,
            NULL,
            NULL,
            sizeof(XingMp3Encoder),
            0,
            (GInstanceInitFunc)xing_mp3_encoder_init,
        };

        xing_mp3_encoder_type = g_type_register_static(
            GST_TYPE_ELEMENT, "XingMp3Encoder", &xing_mp3_encoder_info, 0);
    }

    return xing_mp3_encoder_type;
}

static GstCaps *
mp3_caps_factory()
{
    return gst_caps_new_simple("application/mp3", NULL);
}

static GstCaps *
raw_caps_factory()
{
    return gst_caps_new_simple("audio/x-raw-int",
                               "endianness", G_TYPE_INT,         G_BYTE_ORDER,
                               "signed",     G_TYPE_BOOLEAN,     TRUE,
                               "width",      G_TYPE_INT,         16,
                               "depth",      G_TYPE_INT,         16,
                               "rate",       GST_TYPE_INT_RANGE, 8000, 50000,
                               "channels",   GST_TYPE_INT_RANGE, 1, 2, NULL);
}

static void
xing_mp3_encoder_base_init(gpointer g_class)
{
    GstElementClass *element_class = GST_ELEMENT_CLASS(g_class);
    GstCaps *raw_caps;
    GstCaps *mp3_caps;

    raw_caps = raw_caps_factory();
    mp3_caps = mp3_caps_factory();

    xing_mp3_encoder_sink_template = gst_pad_template_new("sink", GST_PAD_SINK, GST_PAD_ALWAYS, raw_caps);
    xing_mp3_encoder_src_template  = gst_pad_template_new("src", GST_PAD_SRC, GST_PAD_ALWAYS, mp3_caps);
    gst_element_class_add_pad_template(element_class, xing_mp3_encoder_sink_template);
    gst_element_class_add_pad_template(element_class, xing_mp3_encoder_src_template);
    gst_element_class_set_details(element_class, &xing_mp3_encoder_details);
}

static void
xing_mp3_encoder_class_init(XingMp3EncoderClass *klass)
{
    GObjectClass *gobject_class;
    GstElementClass *gstelement_class;
    
    gobject_class = (GObjectClass *)klass;
    gstelement_class = (GstElementClass *)klass;

    gobject_class->set_property = xing_mp3_encoder_set_property;
    gobject_class->get_property = xing_mp3_encoder_get_property;
  
    g_object_class_install_property(G_OBJECT_CLASS(klass), ARG_BITRATE,
        g_param_spec_int("bitrate", "Target Bitrate",
            "Specify a target constant bitrate (in bps).",
            -1, G_MAXINT, BITRATE_DEFAULT, G_PARAM_READWRITE));
    
    g_object_class_install_property(G_OBJECT_CLASS(klass), ARG_LAST_MESSAGE, 
        g_param_spec_string("last-message", "last-message",
        "The last status message", NULL, G_PARAM_READABLE));

    parent_class = g_type_class_ref(GST_TYPE_ELEMENT);

    gstelement_class->change_state = xing_mp3_encoder_change_state;
}

static gboolean
xing_mp3_encoder_sink_setcaps(GstPad *pad, GstCaps *caps)
{
    XingMp3Encoder *encoder;
    GstStructure *structure;

    encoder = XING_MP3_ENCODER(gst_pad_get_parent(pad));
    encoder->is_initialized = FALSE;

    structure = gst_caps_get_structure(caps, 0);
    gst_structure_get_int(structure, "channels", &encoder->channels);
    gst_structure_get_int(structure, "rate", &encoder->frequency);

    xing_mp3_encoder_setup(encoder);

    return encoder->is_initialized;
}

static gboolean
xing_mp3_encoder_convert_src(GstPad *pad, GstFormat src_format,
    gint64 src_value, GstFormat *dest_format, gint64 *dest_value)
{
    gboolean res = TRUE;
    XingMp3Encoder *encoder;
    gint64 avg;

    encoder = XING_MP3_ENCODER(gst_pad_get_parent(pad));

    if(encoder->samples_in == 0 || encoder->bytes_out == 0 || encoder->frequency == 0) {
        return FALSE;
    }

    avg = (encoder->bytes_out * encoder->frequency) / (encoder->samples_in);

    switch(src_format) {
        case GST_FORMAT_BYTES:
            switch(*dest_format) {
                case GST_FORMAT_TIME:
                    *dest_value = src_value * GST_SECOND / avg;
                    break;

                default:
                    res = FALSE;
                    break;
            }
            break;

        case GST_FORMAT_TIME:
            switch(*dest_format) {
                case GST_FORMAT_BYTES:
                    *dest_value = src_value * avg / GST_SECOND;
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

static gboolean
xing_mp3_encoder_convert_sink(GstPad *pad, GstFormat src_format,
    gint64 src_value, GstFormat *dest_format, gint64 *dest_value)
{
    gboolean res = TRUE;
    guint scale = 1;
    gint bytes_per_sample;
    XingMp3Encoder *encoder;

    encoder = XING_MP3_ENCODER(gst_pad_get_parent(pad));

    bytes_per_sample = encoder->channels * 2;

    switch(src_format) {
        case GST_FORMAT_BYTES:
            switch(*dest_format) {
                case GST_FORMAT_DEFAULT:
                    if(bytes_per_sample == 0) {
                        return FALSE;
                    }
                    *dest_value = src_value / bytes_per_sample;
                    break;

                case GST_FORMAT_TIME: {
                    gint byterate = bytes_per_sample * encoder->frequency;

                    if(byterate == 0) {
                        return FALSE;
                    }
                    
                    *dest_value = src_value * GST_SECOND / byterate;
                    break;
                }

                default:
                    res = FALSE;
                    break;
            }
            break;

        case GST_FORMAT_DEFAULT:
            switch(*dest_format) {
                case GST_FORMAT_BYTES:
                    *dest_value = src_value * bytes_per_sample;
                    break;

                case GST_FORMAT_TIME:
                    if(encoder->frequency == 0) {
                        return FALSE;
                    }
                    *dest_value = src_value * GST_SECOND / encoder->frequency;
                    break;

                default:
                    res = FALSE;
                    break;
            }
            break;

        case GST_FORMAT_TIME:
            switch(*dest_format) {
                case GST_FORMAT_BYTES:
                    scale = bytes_per_sample;
                    // fallthrough 

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
xing_mp3_encoder_get_query_types(GstPad *pad)
{
    static const GstQueryType xing_mp3_encoder_src_query_types [] = {
        GST_QUERY_DURATION,
        GST_QUERY_POSITION,
        GST_QUERY_CONVERT,
        0
    };

    return xing_mp3_encoder_src_query_types;
}

static gboolean
xing_mp3_encoder_src_query(GstPad * pad, GstQuery * query)
{
    gboolean res = TRUE;
    XingMp3Encoder *encoder;
    GstPad *peerpad;

    encoder = XING_MP3_ENCODER(gst_pad_get_parent(pad));
    peerpad = gst_pad_get_peer(GST_PAD(encoder->sinkpad));
  
    switch(GST_QUERY_TYPE(query)) {
        case GST_QUERY_DURATION: {
            GstFormat fmt, req_fmt;
            gint64 dur, val;

            gst_query_parse_duration(query, &req_fmt, NULL);
            if((res = gst_pad_query_duration(peerpad, &req_fmt, &val))) {
                gst_query_set_duration(query, req_fmt, val);
                break;
            }

            fmt = GST_FORMAT_TIME;
            if(!(res = gst_pad_query_duration (peerpad, &fmt, &dur))) {
                break;
            }
            
            if((res = gst_pad_query_convert(peerpad, fmt, dur, &req_fmt, &val))) {
                gst_query_set_duration(query, req_fmt, val);
            }
            break;
        }

        case GST_QUERY_POSITION: {
            GstFormat fmt, req_fmt;
            gint64 pos, val;

            gst_query_parse_position(query, &req_fmt, NULL);
            if((res = gst_pad_query_position(peerpad, &req_fmt, &val))) {
                gst_query_set_position (query, req_fmt, val);
                break;
            }

            fmt = GST_FORMAT_TIME;
            if(!(res = gst_pad_query_position(peerpad, &fmt, &pos))) {
                break;
            }
            
            if((res = gst_pad_query_convert(peerpad, fmt, pos, &req_fmt, &val))) {
                gst_query_set_position(query, req_fmt, val);
            }
            break;
        }
        
        case GST_QUERY_CONVERT: {
            GstFormat src_fmt, dest_fmt;
            gint64 src_val, dest_val;

            gst_query_parse_convert(query, &src_fmt, &src_val, &dest_fmt, &dest_val);
            if(!(res = xing_mp3_encoder_convert_src(pad, src_fmt, src_val, &dest_fmt, &dest_val))) {
                gst_object_unref(peerpad);
                gst_object_unref(encoder);
            }
            
            gst_query_set_convert(query, src_fmt, src_val, dest_fmt, dest_val);
            break;
        }
        
        default:
            res = gst_pad_query_default(pad, query);
            break;
    }

    return res;
}

static gboolean
xing_mp3_encoder_sink_query(GstPad * pad, GstQuery * query)
{
    gboolean res = TRUE;
    XingMp3Encoder *encoder;
  
    encoder = XING_MP3_ENCODER(gst_pad_get_parent(pad));
  
    switch(GST_QUERY_TYPE(query)) {
        case GST_QUERY_CONVERT: {
            GstFormat src_fmt, dest_fmt;
            gint64 src_val, dest_val;

            gst_query_parse_convert(query, &src_fmt, &src_val, &dest_fmt, &dest_val);
            if(!(res = xing_mp3_encoder_convert_sink(pad, src_fmt, src_val, &dest_fmt, &dest_val))) {
                return res;
            }
            
            gst_query_set_convert(query, src_fmt, src_val, dest_fmt, dest_val);
            break;
        }
        
        default:
            res = gst_pad_query_default (pad, query);
            break;
    }
  
    return res;
}

static void
xing_mp3_encoder_init (XingMp3Encoder *encoder)
{
    // Sink pad
    encoder->sinkpad = gst_pad_new_from_template(xing_mp3_encoder_sink_template, "sink");
    gst_pad_set_chain_function(encoder->sinkpad, xing_mp3_encoder_chain);
    gst_pad_set_setcaps_function(encoder->sinkpad, xing_mp3_encoder_sink_setcaps);
    gst_pad_set_query_function(encoder->sinkpad, GST_DEBUG_FUNCPTR(xing_mp3_encoder_sink_query));
    gst_element_add_pad(GST_ELEMENT(encoder), encoder->sinkpad);

    // Source pad
    encoder->srcpad = gst_pad_new_from_template(xing_mp3_encoder_src_template, "src");
    gst_pad_set_query_function(encoder->srcpad, GST_DEBUG_FUNCPTR(xing_mp3_encoder_src_query));
    gst_pad_set_query_type_function(encoder->srcpad, GST_DEBUG_FUNCPTR(xing_mp3_encoder_get_query_types)); 
    gst_element_add_pad(GST_ELEMENT(encoder), encoder->srcpad);

    encoder->channels         = -1;
    encoder->frequency        = -1;

    encoder->use_cbr          = FALSE;
    encoder->cbr_bitrate      = BITRATE_DEFAULT;
    encoder->last_message     = NULL;

    encoder->is_initialized   = FALSE;
    encoder->at_end_of_stream = FALSE;
    encoder->header_sent      = FALSE;
}

static void
update_start_message(XingMp3Encoder *encoder)
{
    gchar *constraints;

    if(encoder->last_message != NULL) {
        g_free(encoder->last_message);
        encoder->last_message = NULL;
    }
    
    if(encoder->use_cbr) {
        if(encoder->cbr_bitrate != BITRATE_DEFAULT) {
            encoder->last_message = g_strdup_printf("Encoding at constant bitrate");
        } else {
            encoder->last_message = g_strdup_printf("Encoding at constant bitrate of %d bps", 
                encoder->cbr_bitrate);
        }
    } else {
        encoder->last_message = g_strdup_printf("Encoding at variable bitrate");
    }

    g_object_notify(G_OBJECT(encoder), "last_message");
}

static gboolean
xing_mp3_encoder_setup(XingMp3Encoder *encoder)
{
    E_CONTROL *control;

    update_start_message(encoder);

    control = g_new0(E_CONTROL, 1);

    if(encoder->channels == 1) {
        control->mode                = 3;     /* mode-0 stereo=0 mode-1 stereo=1 dual=2 mono=3 */
    } else { /* encoder->channels == 2 */
        control->mode                = 0;     /* mode-0 stereo=0 mode-1 stereo=1 dual=2 mono=3 */
    }
    
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

    if(encoder->use_cbr) {
        control->vbr_flag = FALSE;
        control->bitrate = (encoder->cbr_bitrate != BITRATE_DEFAULT)
            ? encoder->cbr_bitrate / 2  // Per channel bitrate in 1000s
            : -1;
    } else {
        control->vbr_flag = TRUE;  // 1=vbr, 0=cbr  
        control->bitrate = -1;     // Per channel bitrate in 1000s, CBR only. -1 = default. 
    } 

    encoder->xing_encoder = hx_mp3enc_new();
    encoder->bytes_per_frame =
        hx_mp3enc_mp3_init(encoder->xing_encoder, control,
                        0 /* 16 bits per sample */,
                        0 /* upsample for... default */,
                        0 /* convert to mono? */);

    encoder->input_buffer = g_malloc(INPUT_BUFFER_SIZE);
    encoder->input_buffer_pos = 0;

    encoder->is_initialized = TRUE;

    return TRUE;
}

static GstFlowReturn
xing_mp3_encoder_push_buffer(XingMp3Encoder *encoder, gconstpointer buf, gint buf_len)
{
    GstBuffer *outbuf;

    outbuf = gst_buffer_new_and_alloc(buf_len);
    encoder->bytes_out += GST_BUFFER_SIZE(outbuf);

    memcpy(GST_BUFFER_DATA(outbuf), buf, buf_len);
    return gst_pad_push(encoder->srcpad, outbuf);
}

static GstFlowReturn
xing_mp3_encoder_chain(GstPad *pad, GstBuffer *buf)
{
    XingMp3Encoder *encoder;
    GstFlowReturn ret = GST_FLOW_OK;
  
    g_return_if_fail(pad != NULL);
    g_return_if_fail(GST_IS_PAD(pad));
    g_return_if_fail(buf != NULL);

    encoder = XING_MP3_ENCODER(gst_pad_get_parent(pad));

    if(GST_IS_EVENT(buf)) {
        GstEvent *event = GST_EVENT(buf);

        switch(GST_EVENT_TYPE(event)) {
            case GST_EVENT_EOS:
                encoder->at_end_of_stream = TRUE;
                gst_event_unref(event);
                break;

            default:
                gst_pad_event_default(pad, event);
                return ret;
        }
    } else {
        guchar *data;
        gulong size;
        gulong i, j;
        float **buffer;
        gint buf_size;
        gint buf_pos;

        if(!encoder->is_initialized) {
            gst_buffer_unref(buf);
            GST_ELEMENT_ERROR(encoder, CORE, NEGOTIATION, (NULL), ("Encoder not initialized"));
            return GST_FLOW_UNEXPECTED;
        }

        if(!encoder->header_sent) {
            E_CONTROL control;
            MPEG_HEAD head;
            guchar output_buffer[OUTPUT_BUFFER_SIZE];
            gint buf_len;

            if(!encoder->use_cbr) {
                hx_mp3enc_l3_info_ec(encoder->xing_encoder, &control);
                hx_mp3enc_l3_info_head(encoder->xing_encoder, &head);

                buf_len = XingHeader(control.samprate, head.mode, control.cr_bit, control.original,
                    VBR_SCALE_FLAG /* FRAMES_FLAG | BYTES_FLAG | TOC_FLAG */, 0, 0,
                    control.vbr_flag ? control.vbr_mnr : -1, NULL,
                    output_buffer, 0, 0, 0);
            
                if(xing_mp3_encoder_push_buffer(encoder, output_buffer, buf_len) != GST_FLOW_OK) {
                    if(GST_IS_OBJECT(output_buffer)) {
                        gst_object_unref(output_buffer);
                    }
                }
            }
            
            encoder->header_sent = TRUE;
        }

        data = (guchar *)GST_BUFFER_DATA(buf);
        buf_size = GST_BUFFER_SIZE(buf);
        buf_pos = 0;

        /* Transfer incoming data to internal buffer.
         * TODO: Use a ring buffer, avoid memmove () */

        while(buf_pos < buf_size) {
            gint gulp = MIN(buf_size - buf_pos, INPUT_BUFFER_SIZE - encoder->input_buffer_pos);

            memcpy(encoder->input_buffer + encoder->input_buffer_pos, data + buf_pos, gulp);

            encoder->samples_in += gulp / (2 * encoder->channels);
            encoder->input_buffer_pos += gulp;
            buf_pos += gulp;

            /* Pass data on to encoder */
            while(encoder->input_buffer_pos >= encoder->bytes_per_frame) {
                guchar output_buffer[OUTPUT_BUFFER_SIZE];
                IN_OUT x;

                x = hx_mp3enc_mp3_encode_frame(encoder->xing_encoder, 
                    encoder->input_buffer, output_buffer);

                if(x.in_bytes == 0 && x.out_bytes == 0) {
                    break;
                }

                memmove(encoder->input_buffer, encoder->input_buffer + x.in_bytes,
                    encoder->input_buffer_pos - x.in_bytes);
                encoder->input_buffer_pos -= x.in_bytes;

                /* Accept output from encoder and pass it on.
                 * TODO: Do this less often and save CPU */
                if(x.out_bytes > 0) {
                    if(xing_mp3_encoder_push_buffer(encoder, output_buffer, x.out_bytes) != GST_FLOW_OK) {
                        gst_object_unref(output_buffer);
                    }
                }
            }
        }

        gst_buffer_unref(buf);
    }

    if(encoder->at_end_of_stream) {
        gst_pad_push_event(encoder->srcpad, gst_event_new_eos());
    }
  
    return ret;
}

static void
xing_mp3_encoder_get_property(GObject *object, guint prop_id, GValue *value, GParamSpec *pspec)
{
    XingMp3Encoder *encoder;

    g_return_if_fail(XING_IS_MP3_ENCODER(object));

    encoder = XING_MP3_ENCODER(object);

    switch(prop_id) {
        case ARG_BITRATE:
            g_value_set_int(value, encoder->cbr_bitrate);
            break;
      
        case ARG_LAST_MESSAGE:
            g_value_set_string(value, encoder->last_message);
            break;

        default:
            G_OBJECT_WARN_INVALID_PROPERTY_ID(object, prop_id, pspec);
            break;
    }
}

static void
xing_mp3_encoder_set_property(GObject *object, guint prop_id, const GValue *value, GParamSpec *pspec)
{
    XingMp3Encoder *encoder;

    g_return_if_fail(XING_IS_MP3_ENCODER(object));

    encoder = XING_MP3_ENCODER(object);

    switch(prop_id) {
        case ARG_BITRATE:
            encoder->cbr_bitrate = g_value_get_int (value);
      
            if(encoder->cbr_bitrate != -1 && encoder->cbr_bitrate < 2) {
                g_warning("Encoding bitrate cannot be set lower than 2.");
                encoder->cbr_bitrate = 2;
            }
            
            encoder->use_cbr = TRUE;
            break;
      
        default:
            G_OBJECT_WARN_INVALID_PROPERTY_ID(object, prop_id, pspec);
            break;
    }
}

static GstStateChangeReturn
xing_mp3_encoder_change_state(GstElement * element, GstStateChange transition)
{
    XingMp3Encoder *encoder = XING_MP3_ENCODER(element);

    switch(transition) {
        case GST_STATE_CHANGE_NULL_TO_READY:
        case GST_STATE_CHANGE_READY_TO_PAUSED:
            encoder->at_end_of_stream = FALSE;
            break;

        case GST_STATE_CHANGE_PAUSED_TO_PLAYING:
        case GST_STATE_CHANGE_PLAYING_TO_PAUSED:
            break;

        case GST_STATE_CHANGE_PAUSED_TO_READY:
            encoder->is_initialized = FALSE;
            encoder->header_sent = FALSE;
            break;

        case GST_STATE_CHANGE_READY_TO_NULL:
        default:
            break;
    }

    if(GST_ELEMENT_CLASS(parent_class)->change_state) {
        return GST_ELEMENT_CLASS(parent_class)->change_state(element, transition);
    }
    
    return GST_STATE_CHANGE_SUCCESS;
}

static gboolean
plugin_init(GstPlugin *plugin)
{
    return gst_element_register(plugin, XING_MP3_ENCODER_NAME, 
        GST_RANK_NONE, XING_TYPE_MP3_ENCODER);
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
