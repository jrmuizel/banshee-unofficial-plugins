/*  bbinsource.cpp
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

#include "bbinsource.h"

ByteBufferInputSource::ByteBufferInputSource(IUnknown *ctx, BBInputCallbacks *cb, void *user) :
    ref_count(0),
    callbacks(cb),
    userdata(user),
    common_class_factory(NULL),
    context(ctx)
{
}

STDMETHODIMP_(void) ByteBufferInputSource::Bind(const char* filename)
{
}

STDMETHODIMP ByteBufferInputSource::Create(UINT16 mode)
{
    return HXR_FAIL;
}

STDMETHODIMP ByteBufferInputSource::Open(UINT16 mode)
{
    if(callbacks->Open == NULL) {
        return HXR_FAIL;
    }

    return callbacks->Open(userdata) ? HXR_OK : HXR_FAIL;
}

STDMETHODIMP ByteBufferInputSource::Close()
{
    if(callbacks->Close == NULL) {
        return HXR_FAIL;
    }
    
    return callbacks->Close(userdata) ? HXR_OK : HXR_FAIL;
}

STDMETHODIMP_(BOOL) ByteBufferInputSource::Name(REF(IHXBuffer*) filename)
{
    filename = NULL;
    return FALSE;
}

STDMETHODIMP_(BOOL) ByteBufferInputSource::IsOpen()
{
    return callbacks->IsOpen == NULL ? FALSE : callbacks->IsOpen(userdata);
}

STDMETHODIMP ByteBufferInputSource::Seek(ULONG32 offset, UINT16 whence)
{
    if(callbacks->Seek == NULL) {
        return HXR_FAIL;
    }
    
    return callbacks->Seek(offset, whence, userdata) ? HXR_OK : HXR_FAIL;
}

STDMETHODIMP_(ULONG32) ByteBufferInputSource::Tell()
{
    if(callbacks->Tell == NULL) {
        return 0;
    }
    
    return callbacks->Tell(userdata);
}

STDMETHODIMP_(ULONG32) ByteBufferInputSource::Read(REF(IHXBuffer*) out_buffer, ULONG32 count)
{
    HX_RESULT result;
    IHXBuffer *buffer;
    
    if(callbacks->Read == NULL) {
        return HX_FILESTATUS_ERROR;
    }
    
    if(!IsOpen()) {
        return HX_FILESTATUS_ERROR;
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
        return HX_FILESTATUS_ERROR;
    }
    
    UCHAR *raw_buffer = (UCHAR *)malloc(count);
    long bytes_read = callbacks->Read(raw_buffer, count, userdata);
    
    if(bytes_read <= 0) {
        free(raw_buffer);
        return bytes_read;
    }
    
    buffer->SetSize(bytes_read);
    buffer->Set(raw_buffer, bytes_read);

    out_buffer = buffer;

    free(raw_buffer);
    
    return bytes_read;
}

STDMETHODIMP_(ULONG32) ByteBufferInputSource::Write(REF(IHXBuffer*) pBuf)
{
    return 0;
}

STDMETHODIMP ByteBufferInputSource::Flush()
{
    return HXR_OK;
}

STDMETHODIMP ByteBufferInputSource::Stat(struct stat *buffer)
{
    if(callbacks->Stat == NULL) {
        return HXR_FAIL;
    }
    
    return callbacks->Stat(buffer, userdata) ? HXR_OK : HXR_FAIL;
}

STDMETHODIMP ByteBufferInputSource::Delete()
{
    return HXR_OK;
}

STDMETHODIMP_(INT16) ByteBufferInputSource::GetFd()
{
    return 0;
}

STDMETHODIMP ByteBufferInputSource::GetLastError()
{
    return HXR_OK;
}

STDMETHODIMP_(void) ByteBufferInputSource::GetLastError(REF(IHXBuffer*) error)
{
    error = NULL;
}

STDMETHODIMP ByteBufferInputSource::QueryInterface(REFIID riid, void** ppvObj)
{
    if(IsEqualIID(riid, IID_IUnknown)) {
        AddRef();
        *ppvObj = (IUnknown*)this;
        return HXR_OK;
    } else if(IsEqualIID(riid, IID_IHXDataFile)) {
        AddRef();
        *ppvObj = (IHXDataFile*)this;
        return HXR_OK;
    }

    *ppvObj = NULL;
    return HXR_NOINTERFACE;
}

STDMETHODIMP_(UINT32) ByteBufferInputSource::AddRef() 
{
    return ref_count++;
}

STDMETHODIMP_(UINT32) ByteBufferInputSource::Release()
{
    if(ref_count > 0) {
        return ref_count;
    }

    delete this;
    return 0;
}
