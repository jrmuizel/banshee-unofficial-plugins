#include <stdlib.h>
#include <stdio.h>
#include <glib.h>
#include <glib/gstdio.h>

#include <gst/gst.h>
#include <gst/base/gstadapter.h>

#define PARSER_SAMPLES 512
#define SCOPE_SIZE 1000000

typedef struct ScopeParser ScopeParser;

struct ScopeParser {
    GstElement *fakesink;
    GstPad *tee_pad;
    GstAdapter *adapter;
    gint16 scope[PARSER_SAMPLES];
    gint channels;
    guint buffer_probe_id;
    guint event_probe_id;
};

static gboolean
scope_parser_buffer_probe(GstPad *pad, GstBuffer *buffer, gpointer user_data)
{
    ScopeParser *parser = (ScopeParser *)user_data;
    guint available = gst_adapter_available(parser->adapter);
    
    // this sucks
    if(available > SCOPE_SIZE) {
        gst_adapter_flush(parser->adapter, available - 30000);
    }
    
    if(buffer != NULL) {
        gst_buffer_ref(buffer);
        gst_adapter_push(parser->adapter, buffer);
    }
    
    return TRUE;
}

static gboolean
scope_parser_event_probe(GstPad *pad, GstEvent *event, gpointer user_data)
{
    ScopeParser *parser = (ScopeParser *)user_data;
    if(parser == NULL) {
        return TRUE;
    }
    
    switch(GST_EVENT_TYPE(event)) {
        case GST_EVENT_NEWSEGMENT: {
            gst_adapter_clear(parser->adapter);
            parser->channels = 2;
        }
        
        default:
            break;
    }
    
    return TRUE;
}

gint16 *
scope_parser_poll_scope(ScopeParser *parser)
{
    GstBuffer *first_buffer, *last_buffer;
    GstFormat format = GST_FORMAT_TIME;
    guint64 first_stamp, last_stamp;
    gint64 sink_stamp = 0;
    gint offset, bytes_per_read, i, c;
    guint available;
    gint16 *data;
    gdouble factor;
    
    if(parser->channels == 0) {
        return NULL;
    }
    
    bytes_per_read = PARSER_SAMPLES * parser->channels * sizeof(gint16);

    if(gst_adapter_available(parser->adapter) < bytes_per_read) {
        return parser->scope;
    }
    
    first_buffer = (GstBuffer *)g_slist_nth_data(parser->adapter->buflist, 0);
    last_buffer = (GstBuffer *)(g_slist_last(parser->adapter->buflist)->data);

    first_stamp = GST_BUFFER_TIMESTAMP(first_buffer);
    last_stamp = GST_BUFFER_TIMESTAMP(last_buffer);
    
    gst_element_query_position(parser->fakesink, &format, &sink_stamp);

    available = gst_adapter_available(parser->adapter);
    data = (gint16*)gst_adapter_peek(parser->adapter, available);
    
    factor = (gdouble)(last_stamp - sink_stamp) / (last_stamp - first_stamp);
  
    offset = available - (int)(factor * (double)available);
    if(offset < 0) {
        offset *= -1;
    }
    offset = MIN(offset, available - bytes_per_read);

    for(i = 0; i < PARSER_SAMPLES; i++, data += parser->channels) {
        parser->scope[i] = 0;
        for(c = 0; c < parser->channels; c++) {
            parser->scope[i] += data[offset / sizeof(gint16) + c];
        }
        parser->scope[i] /= parser->channels;
    }
    
    return parser->scope;
}

ScopeParser *
scope_parser_new()
{
    ScopeParser *parser = g_new0(ScopeParser, 1);
    if(parser == NULL) {
        return NULL;
    }
    
    parser->adapter = gst_adapter_new();
    parser->fakesink = NULL;
    parser->channels = 0;
    parser->buffer_probe_id = 0;
    parser->event_probe_id = 0;
    
    return parser;
}

void
scope_parser_free(ScopeParser *parser)
{
    if(parser != NULL) {
        gst_pad_remove_buffer_probe(parser->tee_pad, parser->buffer_probe_id);
        gst_pad_remove_event_probe(parser->tee_pad, parser->event_probe_id);
    
        g_object_unref(parser->adapter);
        parser->adapter = NULL;
        
        g_free(parser);
        parser = NULL;
    }
}

gboolean
scope_parser_attach_to_tee(ScopeParser *parser, GstElement *tee)
{
    GstPad *sink_pad;
    
    if(parser == NULL) {
        return FALSE;
    }
    
    parser->tee_pad = gst_element_get_request_pad(tee, "src1");
    if(parser->tee_pad == NULL) {
        g_warning("Could not get a source pad from the tee");
        return FALSE;
    }
    
    parser->fakesink = gst_element_factory_make("fakesink", "fakesink");
    
    if(parser->fakesink == NULL) {
        g_warning("Could not create fakesink element");
        return FALSE;
    }
    
    sink_pad = gst_element_get_pad(parser->fakesink, "sink");
    gst_pad_link(parser->tee_pad, sink_pad);
    
    parser->buffer_probe_id = gst_pad_add_buffer_probe(parser->tee_pad, 
        G_CALLBACK(scope_parser_buffer_probe), parser);
    parser->event_probe_id = gst_pad_add_event_probe(parser->tee_pad, 
        G_CALLBACK(scope_parser_event_probe), parser);
        
    gst_object_unref(parser->tee_pad);
    gst_object_unref(sink_pad);
    
    return TRUE;
}
