/*  hxfftranscoder.cpp
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
 
#include "hxfftranscoder.h"
#include "bboutsource.h"
#include "bbinsource.h"

#define HXFFT_BAIL(result) { \
    if(!SUCCEEDED(result)) { \
        Dispose(); \
        return result; \
    } \
}

#define HXFFT_CREATE_OR_BAIL(assign, object) { \
    assign = object; \
    if(object == NULL) { \
        HXFFT_BAIL(HXR_OUTOFMEMORY); \
    } \
}

HXFFTranscoder::HXFFTranscoder(void *user) :
    ff_driver(NULL),
    driver_context(NULL),
    driver_commands(NULL),
    userdata(user),
    input_node(NULL),
    output_node(NULL),
    ref_count(0)
{
}

STDMETHODIMP HXFFTranscoder::Initialize(const char *company_name, const char *product_name,
    BBInputCallbacks *input_callbacks, BBOutputCallbacks *output_callbacks)
{
    return Initialize(company_name, product_name, 
        input_callbacks, output_callbacks, 44100, 2, 16);
}

STDMETHODIMP HXFFTranscoder::Initialize(const char *company_name, const char *product_name,
    BBInputCallbacks *input_callbacks, BBOutputCallbacks *output_callbacks,
    ULONG32 samples_per_second, ULONG32 channels, ULONG32 bits_per_sample)
{    
    HXFFT_CREATE_OR_BAIL(ff_driver, new FFDriver());
    ff_driver->AddRef();
   
    HXFFT_BAIL(ff_driver->InitContext(TRUE, (char *)company_name, (char *)product_name, 1, 0));  
    HXFFT_BAIL(ff_driver->GetContext(&driver_context));
    
    if(input_callbacks != NULL) {
        HXFFT_CREATE_OR_BAIL(input_node, new ByteBufferInputSource(driver_context, 
            input_callbacks, userdata));
        HXFFT_BAIL(ff_driver->SetClientContext((IUnknown *)(IHXDataFile *)input_node));
    }
    
    if(output_callbacks != NULL) {
        HXFFT_CREATE_OR_BAIL(output_node, new ByteBufferOutputSource(driver_context, 
            output_callbacks, userdata));
    }
    
    HXFFT_BAIL(CreateValuesCCF(driver_commands, driver_context));
   
    HX_RESULT result = HXR_OK;    
    result &= driver_commands->SetPropertyULONG32("DecodeSource", 1);
    result &= driver_commands->SetPropertyULONG32("DecodeAudio", 1);
    result &= driver_commands->SetPropertyULONG32("MaxSpeed", 1);
    result &= driver_commands->SetPropertyULONG32("Header", 0);
    result &= driver_commands->SetPropertyULONG32("OutputSamplesPerSecond", samples_per_second);
    result &= driver_commands->SetPropertyULONG32("OutputChannels", channels);
    result &= driver_commands->SetPropertyULONG32("OutputBitsPerSample", bits_per_sample);
    HXFFT_BAIL(result);

    HXFFT_BAIL(ff_driver->Init(NULL, NULL, NULL, driver_commands, output_node, NULL, NULL));
   
    return HXR_OK;
}

STDMETHODIMP HXFFTranscoder::Drive(BB_INPUT_TYPE input_type)
{
    return Drive(input_type, NULL);
}

STDMETHODIMP HXFFTranscoder::Drive(BB_INPUT_TYPE input_type, const char *output_uri)
{
    switch(input_type) {
        case BB_INPUT_TYPE_MP3:
            return Drive("fileproxy://.mp3", output_uri);
        case BB_INPUT_TYPE_AAC:
            return Drive("fileproxy://.mp4", output_uri);
    }
    
    return HXR_FAIL;
}

STDMETHODIMP HXFFTranscoder::Drive(const char *input_uri, const char *output_uri)
{
    HX_RESULT result = output_uri == NULL ? 
        ff_driver->Drive((char *)input_uri) : 
        ff_driver->Drive((char *)input_uri, (char *)output_uri);
    
    if(ff_driver != NULL) {
        ff_driver->Close();
    }
    
    return result;
}

STDMETHODIMP_(void) HXFFTranscoder::Stop()
{
    ff_driver->Stop();
    if(ff_driver != NULL) {
        ff_driver->Close();
    }
}

STDMETHODIMP_(void) HXFFTranscoder::Dispose()
{
    HX_RELEASE(ff_driver);
    HX_RELEASE(driver_context);
    HX_RELEASE(driver_commands);
    HX_RELEASE(input_node);
    HX_RELEASE(output_node);
}

// IUnknown implementations

STDMETHODIMP HXFFTranscoder::QueryInterface(REFIID riid, void **ppvObj)
{
    if(IsEqualIID(riid, IID_IUnknown)) {
        AddRef();
        *ppvObj = (IUnknown *)this;
        return HXR_OK;
    }

    *ppvObj = NULL;
    return HXR_NOINTERFACE;
}

STDMETHODIMP_(ULONG32) HXFFTranscoder::AddRef()
{
    return ref_count++;
}

STDMETHODIMP_(ULONG32) HXFFTranscoder::Release()
{
    if(ref_count > 0) {
        return ref_count;
    }
    
    Dispose();
    
    delete this;
    return 0;
}

