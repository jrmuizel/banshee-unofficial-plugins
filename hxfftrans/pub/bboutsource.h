/*  bboutsource.h
 *
 *  Copyright (C) 2006 Novell, Inc.
 *  Written by Aaron Bockover <abockover@novell.com> 
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
 
#ifndef _BBOUTSOURCE_H
#define _BBOUTSOURCE_H
 
#include "hxsrcin.h"
#include "hxcomm.h"
#include "bbiosetup.h"

class ByteBufferOutputSource : public IHXSourceInput
{
private:
    
    INT32 ref_count;
    void *userdata;
    IUnknown *context;
    IHXCommonClassFactory *common_class_factory;
    BBOutputCallbacks *callbacks;
    PCMStreamHeader stream_header;
    
public:
    
    ByteBufferOutputSource(IUnknown *context, BBOutputCallbacks *cb, void *user);

    STDMETHOD(OnFileHeader) (THIS_ HX_RESULT status, IHXValues* values);
    STDMETHOD(OnStreamHeader) (THIS_ HX_RESULT status, IHXValues* values);
    STDMETHOD(OnStreamDone) (THIS_ HX_RESULT status, UINT16 unStreamNumber);
    STDMETHOD(OnPacket) (THIS_ HX_RESULT status, IHXPacket *packet);
    STDMETHOD(OnTermination) (THIS_ HX_RESULT status);
    
    STDMETHOD(QueryInterface) (THIS_ REFIID riid, void **ppvObj);
    STDMETHOD_(UINT32, AddRef) (THIS);
    STDMETHOD_(UINT32, Release) (THIS);
};

#endif /* _BBOUTSOURCE_H */
