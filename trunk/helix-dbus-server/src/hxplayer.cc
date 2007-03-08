/***************************************************************************
 *  hxplayer.cc
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
#include <unistd.h>
#include <pthread.h>

#include "HXClientCFuncs.h"
#include "HXClientCallbacks.h"
#include "HXClientConstants.h"

#include "hxplayer.h"
#include "hxmessage.h"
#include "hxmessagesegment.h"

struct HxPlayer {
    HXClientPlayerToken token;
    HxPlayerMessageCallback message_cb;
    int state;
    pthread_t tid;
    bool join_request;
    gpointer user_info;
};

static void hxplayer_raise_message(HxPlayer *player, HxMessage *message);
#define __RAISE__(player, message) { hxplayer_raise_message((HxPlayer *)player, message); }

// private Helix/hxclientkit callbacks

static void
OnVisualStateChanged(void * user_data, bool hasVisualContent)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_VISUAL_STATE,
        hxmessage_segment_new_int32("HasVisualContent", hasVisualContent),
        NULL
    ));
}

static void
OnIdealSizeChanged(void * user_data, SInt32 idealWidth, SInt32 idealHeight)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_IDEAL_SIZE,
        hxmessage_segment_new_int32("IdealWidth", idealWidth),
        hxmessage_segment_new_int32("IdealHeight", idealHeight),
        NULL
    ));
}

static void
OnLengthChanged(void * user_data, UInt32 length)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_LENGTH,
        hxmessage_segment_new_uint32("Length", length),
        NULL
    ));
}

static void
OnTitleChanged(void * user_data, const char* pTitle)
{
    const char *p = pTitle;
    
    while(true) {
        if(*p == '\0') {
            break;
        } else if(*(p++) < 0) {
            return;
        }
    }
    
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_TITLE,
        hxmessage_segment_new_string("Title", pTitle),
        NULL
    ));
}

static void
OnGroupsChanged(void * user_data)
{
    __RAISE__ (user_data, hxmessage_new(HX_MESSAGE_GROUPS));
}

static void
OnGroupStarted(void * user_data, UInt16 groupIndex)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_GROUP_STARTED,
        hxmessage_segment_new_uint32("GroupIndex", (UInt32)groupIndex),
        NULL
    ));
}

static void
OnContacting(void * user_data, const char* pHostName)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_CONTACTING,
        hxmessage_segment_new_string("HostName", pHostName),
        NULL
    ));
}

static void
OnBuffering(void * user_data, UInt32 bufferingReason, UInt16 bufferPercent)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_BUFFERING,
        hxmessage_segment_new_uint32("Reason", (UInt32)bufferingReason),
        hxmessage_segment_new_uint32("Percent", (UInt32)bufferPercent),
        NULL
    ));
}

static void
OnContentConcluded(void * user_data)
{
    __RAISE__ (user_data, hxmessage_new(HX_MESSAGE_CONTENT_CONCLUDED));
}

static void
OnContentStateChanged(void * user_data, int oldContentState, int newContentState)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_CONTENT_STATE,
        hxmessage_segment_new_int32("OldState", oldContentState),
        hxmessage_segment_new_int32("NewState", newContentState),
        NULL
    ));
}

static void
OnStatusChanged(void * user_data, const char* pStatus)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_STATUS,
        hxmessage_segment_new_string("Status", pStatus),
        NULL
    ));
}

static void
OnVolumeChanged(void * user_data, UInt16 volume)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_VOLUME,
        hxmessage_segment_new_uint32("Volume", (UInt32)volume),
        NULL
    ));
}

static void
OnMuteChanged(void * user_data, bool hasMuted)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_MUTE,
        hxmessage_segment_new_int32("Volume", (SInt32)hasMuted),
        NULL
    ));
}

static void
OnClipBandwidthChanged(void * user_data, SInt32 clipBandwidth)
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_CLIP_BANDWIDTH,
        hxmessage_segment_new_int32("ClipBandwidth", clipBandwidth),
        NULL
    ));
}

static void
OnErrorOccurred(void * user_data, UInt32 hxCode, UInt32 userCode,
        const char* pErrorString, const char* pUserString,
        const char* pMoreInfoURL)
        
{
    __RAISE__ (user_data, hxmessage_new_args(HX_MESSAGE_ERROR,
        hxmessage_segment_new_uint32("HxCode", hxCode),
        hxmessage_segment_new_uint32("UserCode", userCode),
        hxmessage_segment_new_string("ErrorString", pErrorString),
        hxmessage_segment_new_string("UserString", pUserString),
        hxmessage_segment_new_string("MoreInfoURL", pMoreInfoURL),
        NULL
    ));
}

static const HXClientCallbacks HXCLIENT_CALLBACKS = {
    OnVisualStateChanged,
    OnIdealSizeChanged,
    OnLengthChanged,
    OnTitleChanged,
    OnGroupsChanged,
    OnGroupStarted,
    OnContacting,
    OnBuffering,
    OnContentStateChanged,
    OnContentConcluded,
    OnStatusChanged,
    OnVolumeChanged,
    OnMuteChanged,
    OnClipBandwidthChanged,
    OnErrorOccurred,
    NULL,
    NULL,
    NULL,
    NULL
};

// Fake Preferences Methods; Required to work around a bug that
// causes segfaults in httpfsys

static bool
HasFeature(const char *feature_name)
{
	return FALSE;
}

bool 
ReadPreference(const char *key, unsigned char *value_buffer, 
    UInt32 buffer_length, UInt32 *buffer_used_length)
{
	return FALSE;
}

bool
WritePreference(const char *key, const unsigned char *value_buffer, 
    UInt32 buffer_length)
{
	return FALSE;
}

bool
DeletePreference(const char *key)
{
	return FALSE;
}

static const HXClientEngineCallbacks CLIENT_ENGINE_CALLBACKS = {
    ReadPreference,
    WritePreference,
    DeletePreference,
    HasFeature
};

// private hxplayer methods

static void *
hxplayer_pump_thread(void *userdata)
{
    HxPlayer *hxplayer = (HxPlayer *)userdata;
    
    while(hxplayer_get_state(hxplayer) && !hxplayer->join_request) {
        hxplayer_pump(hxplayer);
        usleep(100);
    }
    
    return NULL;
}

static void hxplayer_raise_message(HxPlayer *player, HxMessage *message)
{
    if(player == NULL || message == NULL) {
        return;    
    }
    
    if(player->message_cb != NULL) {
        player->message_cb(player, message);
    }
}

// public hxplayer methods

HxPlayer *
hxplayer_new()
{
    HxPlayer *hxplayer = g_new0(HxPlayer, 1);
    
    if(hxplayer == NULL) {
        return NULL;
    }
    
    hxplayer->token = NULL;
    hxplayer->state = 1;
    hxplayer->tid = 0;
    hxplayer->join_request = FALSE;
    hxplayer->message_cb = NULL;
    hxplayer->user_info = NULL;
    
    ClientEngineSetCallbacks(&CLIENT_ENGINE_CALLBACKS);
    
    if(!ClientPlayerCreate(&hxplayer->token, NULL, hxplayer, &HXCLIENT_CALLBACKS)) {
        free(hxplayer);
        hxplayer = NULL;
        return NULL;
    }
    
    return hxplayer;
}

void
hxplayer_free(HxPlayer *hxplayer)
{
    if(hxplayer == NULL) {
        return;
    }
    
    hxplayer_pump_end(hxplayer);
    
    ClientPlayerClose(hxplayer->token);
    
    g_free(hxplayer);
    hxplayer = NULL;
}

void
hxplayer_set_message_callback(HxPlayer *hxplayer, HxPlayerMessageCallback callback)
{
    if(hxplayer == NULL) {
        return;
    }
    
    hxplayer->message_cb = callback;
}

void 
hxplayer_set_user_info(HxPlayer *hxplayer, gpointer user_info)
{
    if(hxplayer == NULL) {
        return;
    }
    
    hxplayer->user_info = user_info;
}

gpointer
hxplayer_get_user_info(HxPlayer *hxplayer)
{
    if(hxplayer == NULL) {
        return NULL;
    }
    
    return hxplayer->user_info;
}

int
hxplayer_get_state(HxPlayer *hxplayer)
{
    if(hxplayer == NULL) {
        return 0;
    }
    
    return hxplayer->state;
}

void
hxplayer_pump(HxPlayer *hxplayer)
{
    if(hxplayer == NULL) {
        return;
    }
    
    ClientEngineProcessXEvent(NULL);
}

void
hxplayer_pump_begin(HxPlayer *hxplayer)
{
    if(hxplayer == NULL || hxplayer->tid != 0) {
        return;
    }
    
    hxplayer->join_request = FALSE;
    pthread_create(&hxplayer->tid, NULL, hxplayer_pump_thread, hxplayer);
}

void
hxplayer_pump_end(HxPlayer *hxplayer)
{
    if(hxplayer == NULL || hxplayer->tid == 0 || hxplayer->join_request == TRUE) {
        return;
    }
    
    hxplayer->join_request = TRUE;
    pthread_join(hxplayer->tid, NULL);
    hxplayer->join_request = FALSE;
}

HXClientPlayerToken 
hxplayer_get_player_token(HxPlayer *hxplayer)
{
    if(hxplayer == NULL) {
        return NULL;
    }
    
    return hxplayer->token;
}
