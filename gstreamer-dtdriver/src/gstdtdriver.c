/*  gstdtdriver.c
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
 
#ifdef HAVE_CONFIG_H
#include "config.h"
#endif

#include <string.h>

#include <gst/gsttagsetter.h>
#include <gst/tag/tag.h>

#include "gstdtdriver.h"


GST_DEBUG_CATEGORY_STATIC(dtdriver_debug);
#define GST_CAT_DEFAULT dtdriver_debug

static void gst_dtdriver_finalize(GObject *object);

static void gst_dtdriver_loop(GstPad *pad);
static GstStateChangeReturn gst_dtdriver_change_state(GstElement *element,
    GstStateChange transition);
static const GstQueryType *gst_dtdriver_get_src_query_types(GstPad *pad);
static gboolean gst_dtdriver_src_query(GstPad *pad, GstQuery *query);
static gboolean gst_dtdriver_convert_src(GstPad *pad, GstFormat src_format,
    gint64 src_value, GstFormat *dest_format, gint64 *dest_value);
static gboolean gst_dtdriver_src_event(GstPad *pad, GstEvent *event);
static gboolean gst_dtdriver_sink_activate(GstPad *sinkpad);
static gboolean gst_dtdriver_sink_activate_pull(GstPad *sinkpad,
    gboolean active);
static void gst_dtdriver_send_newsegment(GstDTDriver *dtdriver,
    gboolean update);

static gint hxinput_open(gpointer userdata);
static gint hxinput_stat(struct stat *buffer, gpointer userdata);
static gint hxinput_close(gpointer userdata);
static gint hxinput_isopen(gpointer userdata);
static gint hxinput_seek(gulong offset, gushort whence, gpointer userdata);
static gint hxinput_tell(gpointer userdata);
static glong hxinput_read(guchar *buffer, gulong count, gpointer userdata);

static gint hxoutput_file_header(const PCMFileHeader *header, gpointer userdata);
static void hxoutput_stream_header(const PCMStreamHeader *header, gpointer userdata);
static gint hxoutput_write(const PCMDataBuffer *buffer, gpointer userdata);
static void hxoutput_eos(gpointer userdata);

static BBInputCallbacks hxinput_callbacks = {
    hxinput_open,
    hxinput_stat,
    hxinput_read,
    hxinput_seek,
    hxinput_tell,
    hxinput_close,
    hxinput_isopen
};

static BBOutputCallbacks hxoutput_callbacks = {
    hxoutput_file_header,
    hxoutput_stream_header,
    hxoutput_write,
    hxoutput_eos
};

GST_BOILERPLATE(GstDTDriver, gst_dtdriver, GstElement, GST_TYPE_ELEMENT)

static GstStaticPadTemplate gst_dtdriver_src_factory =
GST_STATIC_PAD_TEMPLATE("src",
    GST_PAD_SRC,
    GST_PAD_ALWAYS,
    GST_STATIC_CAPS("audio/x-raw-int,"
        "endianness = (int) " G_STRINGIFY (G_BYTE_ORDER) ", "
        "signed = (boolean) true, "
        "width = (int) 16, depth = (int) 16, "
        "rate = (int) { 8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000 }, "
        "channels = (int) [ 1, 2 ]")
);
  
static GstStaticPadTemplate gst_dtdriver_sink_factory =
GST_STATIC_PAD_TEMPLATE("sink",
    GST_PAD_SINK,
    GST_PAD_ALWAYS,
    GST_STATIC_CAPS("audio/mpeg, mpegversion=(int) 1, "
        "layer = (int) [ 1, 3 ], "
        "rate = (int) { 8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000 }, "
        "channels = (int) [ 1, 2 ]; audio/x-m4a")
);

static void gst_dtdriver_base_init(gpointer g_class)
{
    static GstElementDetails dtdriver_details = {
        "DTDriver decoder",
        "Codec/Decoder/Audio",
        "Decodes audio through the Helix DTDriver engine",
        "Aaron Bockover <aaron@abock.org>",
    };

    GstElementClass *element_class = GST_ELEMENT_CLASS(g_class);
    
    gst_element_class_add_pad_template(element_class, 
        gst_static_pad_template_get(&gst_dtdriver_src_factory));
    gst_element_class_add_pad_template(element_class, 
        gst_static_pad_template_get(&gst_dtdriver_sink_factory));
        
    gst_element_class_set_details(element_class, &dtdriver_details);

    GST_DEBUG_CATEGORY_INIT(dtdriver_debug, "dtdrdec", 0, "Helix DTDriver decoder");
}

static void
gst_dtdriver_class_init(GstDTDriverClass *klass)
{
    GstElementClass *gstelement_class;
    GObjectClass *gobject_class;

    gstelement_class = (GstElementClass *)klass;
    gobject_class = (GObjectClass *)klass;

    gobject_class->finalize = gst_dtdriver_finalize;
    gstelement_class->change_state = GST_DEBUG_FUNCPTR(gst_dtdriver_change_state);
}

static void
gst_dtdriver_init(GstDTDriver *dtdriver, GstDTDriverClass *klass)
{
    dtdriver->sinkpad = gst_pad_new_from_template(gst_static_pad_template_get(
        &gst_dtdriver_sink_factory), "sink");
    gst_pad_set_activate_function(dtdriver->sinkpad, gst_dtdriver_sink_activate);
    gst_pad_set_activatepull_function(dtdriver->sinkpad, gst_dtdriver_sink_activate_pull);
    gst_element_add_pad(GST_ELEMENT(dtdriver), dtdriver->sinkpad);

    dtdriver->srcpad = gst_pad_new_from_template(gst_static_pad_template_get(
        &gst_dtdriver_src_factory), "src");
    gst_pad_set_query_type_function(dtdriver->srcpad, gst_dtdriver_get_src_query_types);
    gst_pad_set_query_function(dtdriver->srcpad, gst_dtdriver_src_query);
    gst_pad_set_event_function(dtdriver->srcpad, gst_dtdriver_src_event);
    gst_pad_use_fixed_caps(dtdriver->srcpad);
    gst_element_add_pad(GST_ELEMENT(dtdriver), dtdriver->srcpad);

    dtdriver->decoder = hxfftdriver_new(dtdriver);

    dtdriver->segment.last_stop = 0;
    dtdriver->init = TRUE;
}

static void
gst_dtdriver_finalize(GObject *object)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(object);

    if(dtdriver->decoder != NULL) {
        hxfftdriver_free(dtdriver->decoder);
        dtdriver->decoder = NULL;
    }
    
    G_OBJECT_CLASS(parent_class)->finalize(object);
}

static void
gst_dtdriver_loop(GstPad *sinkpad)
{
    GstDTDriver *dtdriver;
    GstBuffer *buffer;
    guint8 *buffer_data; 
    BB_INPUT_TYPE input_type;
    
    dtdriver = GST_DTDRIVER(GST_OBJECT_PARENT(sinkpad));

    if(dtdriver->init) {
        const char *plugin_path = HELIX_PLUGINS_PATH;
        const char *codec_path = HELIX_CODECS_PATH;
        const char *temp;
        
        temp = getenv("HELIX_PLUGINS");
        if(temp != NULL) {
            plugin_path = temp;
        }
        
        temp = getenv("HELIX_CODECS");
        if(temp != NULL) {
            codec_path = temp;
        }
        
        hx_set_dll_access_paths(plugin_path, codec_path);
    
        if(!hxfftdriver_initialize_external_io(dtdriver->decoder, "Novell", "Banshee", 
            &hxinput_callbacks, &hxoutput_callbacks)) {
        }
        
        dtdriver->init = FALSE;
        
        if(gst_pad_pull_range(dtdriver->sinkpad, 0, 8, &buffer) != GST_FLOW_OK) {
            return;
        }
        
        buffer_data = GST_BUFFER_DATA(buffer);
        
        if(buffer_data[4] == 'f' && buffer_data[5] == 't' 
            && buffer_data[6] == 'y' && buffer_data[7] == 'p') {
            input_type = BB_INPUT_TYPE_AAC;
            dtdriver->codec = "AAC";
        } else {
            input_type = BB_INPUT_TYPE_MP3;
            dtdriver->codec = "MP3";
        }
        
        gst_buffer_unref(buffer);
        
        if(!hxfftdriver_drive_external_io(dtdriver->decoder, input_type)) {
            dtdriver->last_flow = GST_FLOW_ERROR;
            GST_ELEMENT_ERROR(dtdriver, LIBRARY, INIT, NULL,
                ("Failed to load Helix plugin or initialize driver.", NULL));
            gst_pad_pause_task(sinkpad);
            return;
        }
    }

    dtdriver->last_flow = GST_FLOW_OK;
    gst_pad_pause_task(sinkpad);
    
    return;
}

static gboolean
gst_dtdriver_convert_src(GstPad *pad, GstFormat src_format, gint64 src_value,
    GstFormat *dest_format, gint64 *dest_value)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(GST_PAD_PARENT(pad));
    gboolean result = TRUE;
    guint bytes_per_sample;

    bytes_per_sample = dtdriver->depth / 8;

    if(dtdriver->sample_rate == 0 || bytes_per_sample == 0) {
        return FALSE;
    }
  
    switch(src_format) {
        case GST_FORMAT_BYTES:
            switch(*dest_format) {
                case GST_FORMAT_TIME: // Bytes to time 
                    *dest_value = gst_util_uint64_scale(src_value, GST_SECOND, 
                        (guint64)(bytes_per_sample) * 
                        dtdriver->sample_rate);
                    result = TRUE;
                    break;
                case GST_FORMAT_DEFAULT: // Bytes to samples 
                    *dest_value = src_value / bytes_per_sample;
                    result = TRUE;
                    break;
                default:
                    break;
            }
            break;
        case GST_FORMAT_TIME:
            switch(*dest_format) {
                case GST_FORMAT_BYTES: // Time to bytes 
                    *dest_value = gst_util_uint64_scale(src_value, 
                        (guint64)(bytes_per_sample) * 
                        dtdriver->sample_rate, GST_SECOND);
                    result = TRUE;
                    break;
                case GST_FORMAT_DEFAULT: // Time to samples 
                    *dest_value = gst_util_uint64_scale(src_value, 
                        dtdriver->sample_rate, GST_SECOND);
                    result = TRUE;
                    break;
                default:
                    break;
            }
            break;
        case GST_FORMAT_DEFAULT: // Samples 
            switch(*dest_format) {
                case GST_FORMAT_BYTES: // Samples to bytes 
                    *dest_value = src_value * bytes_per_sample;
                    result = TRUE;
                    break;
                case GST_FORMAT_TIME: // Samples to time
                    *dest_value = gst_util_uint64_scale(src_value, GST_SECOND, 
                        dtdriver->sample_rate);
                    result = TRUE;
                    break;
                default:
                    break;
            }
            break;
        default:
            break;
    }
  
    return result;
}

static const GstQueryType *
gst_dtdriver_get_src_query_types(GstPad *pad)
{
    static const GstQueryType types [] = {
        GST_QUERY_POSITION,
        GST_QUERY_DURATION,
        0,
    };

    return types;
}

static gboolean
gst_dtdriver_src_query(GstPad *pad, GstQuery *query)
{
    GstDTDriver *dtdriver;
    gint64 current;
    GstPad *peer;
    GstFormat format;

    dtdriver = GST_DTDRIVER(gst_pad_get_parent(pad));

    if((peer = gst_pad_get_peer(dtdriver->sinkpad)) == NULL) {
        return FALSE;
    }

    switch(GST_QUERY_TYPE (query)) {
        case GST_QUERY_POSITION: {
            gst_query_parse_position(query, &format, NULL);

            if(format != GST_FORMAT_BYTES && gst_pad_query(peer, query)) {
                gst_object_unref(peer);
                return TRUE;
            }
            
            gst_object_unref(peer);

            current = dtdriver->segment.last_stop;

            if(format != GST_FORMAT_DEFAULT) {
                if(!gst_dtdriver_convert_src(dtdriver->srcpad, GST_FORMAT_DEFAULT,
                    current, &format, &current)) {
                    gst_query_set_position(query, format, -1);
                    return FALSE;
                }
            }
            
            gst_query_set_position(query, format, current);
            break;
        }
        
        case GST_QUERY_DURATION: {
            gst_query_parse_duration(query, &format, NULL);

            if(format != GST_FORMAT_BYTES && gst_pad_query(peer, query)) {
                gst_object_unref(peer);
                return TRUE;
            }
            
            gst_object_unref(peer);
            peer = NULL;
      
            current = dtdriver->duration;
        
            if(current != -1) {
                if(format != GST_FORMAT_TIME && !gst_dtdriver_convert_src(pad, 
                    GST_FORMAT_TIME, current, &format, &current)) {
                    gst_query_set_duration(query, format, -1);
                    return FALSE;
                }
            }

            gst_query_set_duration(query, format, current);
            break;
        }
        
        default:
            return FALSE;
    }
    
    return TRUE;
}

static void
gst_dtdriver_send_newsegment(GstDTDriver *dtdriver, gboolean update)
{
    GstSegment *segment = &dtdriver->segment;
    GstFormat target_format = GST_FORMAT_TIME;
    gint64 stop_time = GST_CLOCK_TIME_NONE;
    gint64 start_time = 0;

    if(!gst_dtdriver_convert_src(dtdriver->srcpad, GST_FORMAT_DEFAULT,
        segment->start, &target_format, &start_time)) {
        GST_WARNING_OBJECT(dtdriver, "failed to convert segment start %lld to TIME",
            segment->start);
        return;
    }

    if(segment->stop != -1 && !gst_dtdriver_convert_src(dtdriver->srcpad,
        GST_FORMAT_DEFAULT, segment->stop, &target_format, &stop_time)) {
        GST_WARNING_OBJECT(dtdriver, "failed to convert segment stop to TIME");
        return;
    }

    GST_DEBUG_OBJECT(dtdriver, "sending newsegment from %" GST_TIME_FORMAT
        " to %" GST_TIME_FORMAT, GST_TIME_ARGS(start_time),
        GST_TIME_ARGS(stop_time));

    gst_pad_push_event(dtdriver->srcpad, gst_event_new_new_segment(update, 
        segment->rate, GST_FORMAT_TIME, start_time, stop_time, start_time));
}

static gboolean
gst_dtdriver_src_event(GstPad *pad, GstEvent *event)
{
    gboolean result = TRUE;
    GstDTDriver *dtdriver = GST_DTDRIVER(gst_pad_get_parent(pad));

    switch(GST_EVENT_TYPE(event)) {
        default:
            result = gst_pad_event_default(pad, event);
            break;
    }

    gst_object_unref(dtdriver);

    return result;
}

static gboolean
gst_dtdriver_sink_activate(GstPad *sinkpad)
{
    if(gst_pad_check_pull_range(sinkpad)) {
        return gst_pad_activate_pull(sinkpad, TRUE);
    }
    
    return FALSE;
}

static gboolean
gst_dtdriver_sink_activate_pull(GstPad *sinkpad, gboolean active)
{
    if(active) {
        GST_DTDRIVER(GST_OBJECT_PARENT(sinkpad))->offset = 0;
        return gst_pad_start_task(sinkpad, (GstTaskFunction)gst_dtdriver_loop,
            sinkpad);
    } else {
        return gst_pad_stop_task(sinkpad);
    }
}

static GstStateChangeReturn
gst_dtdriver_change_state(GstElement *element, GstStateChange transition)
{
    GstStateChangeReturn result = GST_STATE_CHANGE_SUCCESS;
    GstDTDriver *dtdriver = GST_DTDRIVER(element);

    switch(transition) {
        case GST_STATE_CHANGE_READY_TO_PAUSED:
            dtdriver->segment.last_stop = 0;
            dtdriver->need_newsegment = TRUE;
            dtdriver->channels = 0;
            dtdriver->depth = 0;
            dtdriver->width = 0;
            dtdriver->sample_rate = 0;
            dtdriver->codec = NULL;
            
            if(!dtdriver->init && dtdriver->decoder != NULL) {
                hxfftdriver_stop(dtdriver->decoder);
            }
            
            gst_segment_init(&dtdriver->segment, GST_FORMAT_DEFAULT);
            break;
        default:
            break;
    }

    result = GST_ELEMENT_CLASS(parent_class)->change_state(element, transition);
    if(result == GST_STATE_CHANGE_FAILURE) {
        return result;
    }
  
    switch(transition) {
        case GST_STATE_CHANGE_PAUSED_TO_READY:
            gst_segment_init(&dtdriver->segment, GST_FORMAT_UNDEFINED);
            break;
        default:
            break;
    }

    return result;
}

/* Helix DTDriver Callbacks */

static gint 
hxinput_open(gpointer userdata)
{
    return TRUE;
}

static gint
hxinput_stat(struct stat *buffer, gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    gint64 duration = 0;
    GstFormat format = GST_FORMAT_BYTES;
    GstPad *peer;
    
    peer = gst_pad_get_peer(dtdriver->sinkpad);
    
    if(!gst_pad_query_duration(peer, &format, &duration)) {
        return FALSE;
    }
    
    memset(buffer, 0, sizeof(struct stat));
    buffer->st_size = duration;
    
    return TRUE;
}

static gint 
hxinput_tell(gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    return dtdriver->offset;
}

static gint 
hxinput_close(gpointer userdata)
{
    return TRUE;
}

static gint 
hxinput_isopen(gpointer userdata)
{
    return TRUE;
}

static gint 
hxinput_seek(gulong offset, gushort whence, gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    dtdriver->offset = offset;
    return TRUE;
}

static glong 
hxinput_read(guchar *out_buffer, gulong count, gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    GstBuffer *buffer;
    guint buffer_size = 0;
    
    if(gst_pad_pull_range(dtdriver->sinkpad, dtdriver->offset, count,
        &buffer) != GST_FLOW_OK) {
        return HX_FILESTATUS_ERROR;
    }
    
    buffer_size = GST_BUFFER_SIZE(buffer);
    memcpy(out_buffer, GST_BUFFER_DATA(buffer), buffer_size);    
    dtdriver->offset += buffer_size;

    gst_buffer_unref(buffer);
        
    return buffer_size;
}

static gint
hxoutput_file_header(const PCMFileHeader *header, gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    GstTagList *tags;
    
    tags = gst_tag_list_new();
    
    gst_tag_list_add(tags, GST_TAG_MERGE_REPLACE,
        GST_TAG_AUDIO_CODEC, dtdriver->codec, NULL);
    
    if(header->title != NULL) {
        gst_tag_list_add(tags, GST_TAG_MERGE_REPLACE,
            GST_TAG_TITLE, header->title, NULL);
    }
    
    if(header->author != NULL) {
        gst_tag_list_add(tags, GST_TAG_MERGE_REPLACE,
            GST_TAG_ARTIST, header->author, NULL);
    }
    
    if(header->copyright != NULL) {
        gst_tag_list_add(tags, GST_TAG_MERGE_REPLACE,
            GST_TAG_COPYRIGHT, header->copyright, NULL);
    }
    
    gst_element_found_tags(GST_ELEMENT(dtdriver), tags);
    
    return TRUE;
}

static void 
hxoutput_stream_header(const PCMStreamHeader *header, gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    
    dtdriver->channels = header->channels;
    dtdriver->sample_rate = header->samples_per_second;
    dtdriver->depth = header->bits_per_sample;
    dtdriver->duration = header->duration * GST_MSECOND;
    
    dtdriver->segment.duration = dtdriver->duration;
}

static gint 
hxoutput_write(const PCMDataBuffer *buffer, gpointer userdata)
{
    GstFlowReturn result = GST_FLOW_OK;
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    GstBuffer *out_buffer;
    
    guint channels = buffer->header.channels;
    guint sample_rate = buffer->header.samples_per_second;
    guint depth = buffer->header.bits_per_sample;
    guint samples = (buffer->data_size / (depth / 8)) / channels;
    guint width;
    
    switch(depth) {
        case 8:
            width = 8;
            break;
        case 12: case 16:
            width = 16;
            break;
        case 20: case 24: case 32:
            width = 32;
            break;
        default:
            g_assert_not_reached();
    }

    if(!GST_PAD_CAPS(dtdriver->srcpad)) {
        GstCaps *caps;

        GST_DEBUG("Negotiating %d Hz @ %d channels",
            sample_rate, channels);

        caps = gst_caps_new_simple("audio/x-raw-int",
            "endianness", G_TYPE_INT, G_BYTE_ORDER,
            "signed", G_TYPE_BOOLEAN, TRUE,
            "width", G_TYPE_INT, width,
            "depth", G_TYPE_INT, depth,
            "rate", G_TYPE_INT, sample_rate,
            "channels", G_TYPE_INT, channels, NULL);

        if(!gst_pad_set_caps(dtdriver->srcpad, caps)) {
            GST_ELEMENT_ERROR(dtdriver, CORE, NEGOTIATION, (NULL),
                ("Failed to negotiate caps %" GST_PTR_FORMAT, caps));
            dtdriver->last_flow = GST_FLOW_ERROR;
            gst_caps_unref(caps);
            return 0;
        }

        gst_caps_unref(caps);

        dtdriver->depth = depth;
        dtdriver->width = width;
        dtdriver->channels = channels;
        dtdriver->sample_rate = sample_rate;
    }

    if(dtdriver->need_newsegment) {
        gst_dtdriver_send_newsegment(dtdriver, FALSE);
        dtdriver->need_newsegment = FALSE;
    }

    result = gst_pad_alloc_buffer_and_set_caps(dtdriver->srcpad,
        dtdriver->segment.last_stop, buffer->data_size,
        GST_PAD_CAPS(dtdriver->srcpad), &out_buffer);

    if(result != GST_FLOW_OK) {
        GST_DEBUG("gst_pad_alloc_buffer() returned %s", gst_flow_get_name(result));
        dtdriver->segment.last_stop += samples;
        return FALSE;
    }

    GST_BUFFER_TIMESTAMP(out_buffer) =
        gst_util_uint64_scale_int(dtdriver->segment.last_stop, GST_SECOND, sample_rate);
    GST_BUFFER_DURATION(out_buffer) =
        gst_util_uint64_scale_int(samples, GST_SECOND, sample_rate);

    memcpy(GST_BUFFER_DATA(out_buffer), buffer->data_buffer, GST_BUFFER_SIZE(out_buffer));
    result = gst_pad_push(dtdriver->srcpad, out_buffer);
    
    if(result != GST_FLOW_OK) {
        GST_DEBUG("gst_pad_push() returned %s", gst_flow_get_name(result));
    }

    dtdriver->segment.last_stop += samples;
    return TRUE;
}

static void
hxoutput_eos(gpointer userdata)
{
    GstDTDriver *dtdriver = GST_DTDRIVER(userdata);
    gst_pad_push_event(dtdriver->srcpad, gst_event_new_eos());
    gst_pad_pause_task(dtdriver->sinkpad);
}

/* Plugin Entry Point */

static gboolean
plugin_init(GstPlugin *plugin)
{
    return gst_element_register(plugin, DTDRIVER_DECODER_NAME, 
        GST_RANK_PRIMARY, GST_TYPE_DTDRIVER);
}

GST_PLUGIN_DEFINE (
    GST_VERSION_MAJOR,
    GST_VERSION_MINOR,
    DTDRIVER_DECODER_NAME,
    "Helix DTDriver Decoder",
    plugin_init,
    VERSION,
    "LGPL",
    "Banshee",
    "http://banshee-project.org/"
)
