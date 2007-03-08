#include <unistd.h>
#include <glib/gstdio.h>
#include <gst/gst.h>

static GMainLoop *loop;

static gboolean
bus_callback(GstBus *bus, GstMessage *message, gpointer data)
{
	switch(GST_MESSAGE_TYPE(message)) {
		case GST_MESSAGE_ERROR: {
			GError *error;
			gchar *debug;

			gst_message_parse_error(message, &error, &debug);
			g_printf("Error %s, %s\n", error->message, debug);
			g_error_free(error);
			g_free(debug);

			g_main_loop_quit(loop);
			break;
		} case GST_MESSAGE_EOS:
			g_main_loop_quit(loop);
			break;
		default:
			break;
	}

	return TRUE;
}

int main(int argc, char **argv)
{
	GstElement *pipeline;
	GstElement *src;
	GstElement *decoder;
	GstElement *sink;
	int fi = 0;

	if(argc < 2) {
		g_printf("Usage: %s <file>\n", argv[0]);
		exit(1);
	}

	gst_init(NULL, NULL);

	pipeline = gst_pipeline_new("pipeline");
	src = gst_element_factory_make("filesrc", "src");
	decoder = gst_element_factory_make("dtdrdec", "decoder");
	sink = gst_element_factory_make("alsasink", "sink");

	gst_bin_add_many(GST_BIN(pipeline), src, decoder, sink, NULL);
	gst_element_link_many(src, decoder, sink, NULL);

	g_object_set(src, "location", argv[1], NULL);
	gst_bus_add_watch(gst_pipeline_get_bus(GST_PIPELINE(pipeline)), 
		bus_callback, NULL);

	gst_element_set_state(pipeline, GST_STATE_PLAYING);

	loop = g_main_loop_new(NULL, FALSE);
	g_main_loop_run(loop);

	gst_element_set_state(pipeline, GST_STATE_NULL);
	gst_object_unref(pipeline);

	exit(0);
}

