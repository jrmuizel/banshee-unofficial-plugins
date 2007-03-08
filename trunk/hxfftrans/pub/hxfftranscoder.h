/*  hxfftranscoder.h
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

#ifndef _HXFFTRANSCODER_H
#define _HXFFTRANSCODER_H
 
#include "hxengin.h"
#include "hxdataf.h"
#include "pckunpck.h"
#include "ffdriver.h"

#include "bbinsource.h"
#include "bboutsource.h"

class HXFFTranscoder : public IUnknown
{
private:

    FFDriver *ff_driver;
    IUnknown *driver_context;
    IHXValues* driver_commands;
    void *userdata;
    IHXDataFile *input_node;
    IHXSourceInput *output_node;
    INT32 ref_count;
    
    STDMETHOD_(void, Dispose) (THIS);
   
public:
    
    HXFFTranscoder(void *user);
    
    STDMETHOD(Initialize) (THIS_ const char *company_name, const char *product_name, 
        BBInputCallbacks *input_callbacks, BBOutputCallbacks *output_callbacks);
    STDMETHOD(Initialize) (THIS_ const char *company_name, const char *product_name,
        BBInputCallbacks *input_callbacks, BBOutputCallbacks *output_callbacks,
        ULONG32 samples_per_second, ULONG32 channels, ULONG32 bits_per_sample);
        
    STDMETHOD(Drive) (THIS_ BB_INPUT_TYPE input_type);
    STDMETHOD(Drive) (THIS_ BB_INPUT_TYPE input_type, const char *output_uri);
    STDMETHOD(Drive) (THIS_ const char *input_uri, const char *output_uri);
    STDMETHOD_(void, Stop) (THIS);

    STDMETHOD(QueryInterface) (THIS_ REFIID riid, void** ppvObj);
    STDMETHOD_(ULONG32, AddRef) (THIS);
    STDMETHOD_(ULONG32, Release) (THIS);
};

#endif /* _HXFFTRANSCODER_H */
