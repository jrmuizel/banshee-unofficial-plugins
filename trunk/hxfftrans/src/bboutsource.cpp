/*  bboutsource.cpp
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

#include <stdlib.h>

#include "bboutsource.h"

ByteBufferOutputSource::ByteBufferOutputSource(IUnknown *ctx, BBOutputCallbacks *cb, void *user) : 
    ref_count(0),
    userdata(user),
    context(ctx),
    common_class_factory(NULL),
    callbacks(cb)
{
    memset(&stream_header, 0, sizeof(PCMStreamHeader));
}

STDMETHODIMP ByteBufferOutputSource::OnFileHeader(HX_RESULT status, IHXValues* values)
{
    PCMFileHeader *header;
    IHXBuffer *buffer;
    HX_RESULT result;
    
    if(callbacks->FileHeader == NULL) {
        return HXR_FAIL;
    }
    
    if(common_class_factory == NULL) {
        result = context->QueryInterface(IID_IHXCommonClassFactory,
            (void **)&common_class_factory);
        if(result != HXR_OK) {
            return HX_FILESTATUS_ERROR;
        }
    }
    
    result = common_class_factory->CreateInstance(CLSID_IHXBuffer, (void **)&buffer);
    if(result != HXR_OK) {
        return result;
    }
    
    header = (PCMFileHeader *)malloc(sizeof(PCMFileHeader));
    if(header == NULL) {
        return HXR_OUTOFMEMORY;
    }
    
    memset(header, 0, sizeof(PCMFileHeader));
    
    if(SUCCEEDED(values->GetPropertyBuffer("Title", buffer))) {
        header->title = strndup((char *)buffer->GetBuffer(), buffer->GetSize());
        HX_RELEASE(buffer);
    }
    
    if(SUCCEEDED(values->GetPropertyBuffer("Author", buffer))) {
        header->author = strndup((char *)buffer->GetBuffer(), buffer->GetSize());
        HX_RELEASE(buffer);
    }
    
    if(SUCCEEDED(values->GetPropertyBuffer("Copyright", buffer))) {
        header->copyright = strndup((char *)buffer->GetBuffer(), buffer->GetSize());
        HX_RELEASE(buffer);
    }
    
    if(SUCCEEDED(values->GetPropertyBuffer("Abstract", buffer))) {
        header->abstract = strndup((char *)buffer->GetBuffer(), buffer->GetSize());
        HX_RELEASE(buffer);
    }
    
    result = callbacks->FileHeader((const PCMFileHeader *)header, userdata) ? HXR_OK : HXR_FAIL;
    
    if(header->title != NULL) {
        free(header->title);
    }
    
    if(header->author != NULL) {
        free(header->author);
    }
    
    if(header->copyright != NULL) {
        free(header->copyright);
    }
    
    if(header->abstract != NULL) {
        free(header->abstract);
    }
    
    free(header);
    
    return result;
}

STDMETHODIMP ByteBufferOutputSource::OnStreamHeader(HX_RESULT status, IHXValues* values)
{
    values->GetPropertyULONG32("Channels", stream_header.channels);
    values->GetPropertyULONG32("SamplesPerSecond", stream_header.samples_per_second);
    values->GetPropertyULONG32("AvgBitRate", stream_header.average_bit_rate);
    values->GetPropertyULONG32("Duration", stream_header.duration);
    
    stream_header.bits_per_sample = 16; // FIXME: how to query for this?
    
    if(callbacks->StreamHeader != NULL) {
        callbacks->StreamHeader((const PCMStreamHeader *)&stream_header, userdata);
    }
    
    return HXR_OK;
}

STDMETHODIMP ByteBufferOutputSource::OnStreamDone(HX_RESULT status, UINT16 unStreamNumber)
{
    if(callbacks->StreamDone != NULL) {
        callbacks->StreamDone(userdata);
    }
    
    return HXR_OK;
}

STDMETHODIMP ByteBufferOutputSource::OnPacket(HX_RESULT status, IHXPacket *packet)
{
    if(callbacks->Write == NULL) {
        return HXR_FAIL;
    }

    if(packet->GetStreamNumber() == 0) {
        IHXBuffer *hxbuffer = packet->GetBuffer();
        PCMDataBuffer *buffer = (PCMDataBuffer *)malloc(sizeof(PCMDataBuffer));
    
        memcpy(&buffer->header, &stream_header, sizeof(PCMStreamHeader));
        buffer->data_size = hxbuffer->GetSize();
        buffer->data_buffer = hxbuffer->GetBuffer();
        
        int result = callbacks->Write(buffer, userdata);
        
        free(buffer);
        HX_RELEASE(hxbuffer);
        
        return result ? HXR_OK : HXR_FAIL;
    }
    
    return HXR_FAIL;
}

STDMETHODIMP ByteBufferOutputSource::OnTermination(HX_RESULT status)
{
    return HXR_OK;
}

STDMETHODIMP ByteBufferOutputSource::QueryInterface(REFIID riid, void** ppvObj)
{
   if(IsEqualIID(riid, IID_IUnknown)) {
       AddRef();
       *ppvObj = (IUnknown*)this;
       return HXR_OK;
   } else if(IsEqualIID(riid, IID_IHXSourceInput)) {
       AddRef();
       *ppvObj = (IHXSourceInput*)this;
       return HXR_OK;
   }

   *ppvObj = NULL;
   return HXR_NOINTERFACE;
}

STDMETHODIMP_(UINT32) ByteBufferOutputSource::AddRef() 
{
   return ref_count++;
}

STDMETHODIMP_(UINT32) ByteBufferOutputSource::Release()
{
   if(ref_count > 0) {
      return ref_count;
   }

   delete this;
   return 0;
}
