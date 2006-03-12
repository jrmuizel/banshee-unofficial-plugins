/***************************************************************************
 *  hxplayer.h
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
 
#ifndef _HXPLAYER_H
#define _HXPLAYER_H

#include "hxmessage.h"

typedef struct HxPlayer HxPlayer;

typedef void (* HxPlayerMessageCallback) (HxPlayer *player, HxMessage *message);

HxPlayer *hxplayer_new();
void hxplayer_free(HxPlayer *hxplayer);

void hxplayer_set_user_info(HxPlayer *hxplayer, gpointer user_info);
gpointer hxplayer_get_user_info(HxPlayer *hxplayer);
void hxplayer_set_message_callback(HxPlayer *hxplayer, HxPlayerMessageCallback callback);

int hxplayer_get_state(HxPlayer *hxplayer);

int hxplayer_open(HxPlayer *hxplayer, const char *uri);

void hxplayer_play(HxPlayer *hxplayer);
void hxplayer_pause(HxPlayer *hxplayer);

void hxplayer_pump(HxPlayer *hxplayer);
void hxplayer_pump_begin(HxPlayer *hxplayer);
void hxplayer_pump_end(HxPlayer *hxplayer);

#endif /* _HXPLAYER_H */
