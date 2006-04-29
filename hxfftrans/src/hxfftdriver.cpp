/*  hxfftdriver.cpp
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

#include "dllpath.h"
#include "hxengin.h"

#include "hxfftdriver.h"
#include "hxfftranscoder.h"

ENABLE_DLLACCESS_PATHS(g_DTDriveAccessPath);

void
hx_set_dll_access_paths(const char *plugin_path, const char *codec_path)
{
    GetDLLAccessPath()->SetPath(DLLTYPE_PLUGIN, plugin_path);
    GetDLLAccessPath()->SetPath(DLLTYPE_CODEC, codec_path);
}

struct HXFFTDriver {
    HXFFTranscoder *driver;
};

#define DRIVER_ASSERT(driver) { if(driver == NULL || driver->driver == NULL) { return 0; } }

HXFFTDriver *
hxfftdriver_new(void *userdata)
{
    HXFFTDriver *driver = (HXFFTDriver *)malloc(sizeof(HXFFTDriver));
    driver->driver = new HXFFTranscoder(userdata);
    return driver;
}

void
hxfftdriver_free(HXFFTDriver *driver)
{
    if(driver == NULL) {
        return;
    }
    
    if(driver->driver != NULL) {
        HX_RELEASE(driver->driver);
        driver->driver = NULL;
    }
    
    free(driver);
    driver = NULL;
}

int
hxfftdriver_initialize(HXFFTDriver *driver, const char *company_name, const char *product_name)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Initialize(company_name, product_name, 
        NULL, NULL) == HXR_OK ? 1 : 0;
}

int
hxfftdriver_initialize_full(HXFFTDriver *driver, const char *company_name, const char *product_name,
    unsigned long samples_per_second, unsigned long channels, unsigned long bits_per_sample)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Initialize(company_name, product_name, 
        NULL, NULL, samples_per_second, channels, bits_per_sample) == HXR_OK ? 1 : 0;
}

int
hxfftdriver_initialize_external_io(HXFFTDriver *driver, const char *company_name, const char *product_name,
    BBInputCallbacks *input_callbacks, BBOutputCallbacks *output_callbacks)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Initialize(company_name, product_name, 
        input_callbacks, output_callbacks) == HXR_OK ? 1 : 0;
}

int
hxfftdriver_initialize_external_io_full(HXFFTDriver *driver, const char *company_name, const char *product_name,
    BBInputCallbacks *input_callbacks, BBOutputCallbacks *output_callbacks,
    unsigned long samples_per_second, unsigned long channels, unsigned long bits_per_sample)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Initialize(company_name, product_name, 
        input_callbacks, output_callbacks, samples_per_second, 
        channels, bits_per_sample) == HXR_OK ? 1 : 0;
}

int
hxfftdriver_drive(HXFFTDriver *driver, const char *input_uri, const char *output_uri)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Drive(input_uri, output_uri) == HXR_OK ? 1 : 0;
}

int
hxfftdriver_drive_external_io(HXFFTDriver *driver, BB_INPUT_TYPE input_type)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Drive(input_type) == HXR_OK ? 1 : 0;
}

int
hxfftdriver_drive_external_input(HXFFTDriver *driver, 
    BB_INPUT_TYPE input_type, const char *output_uri)
{
    DRIVER_ASSERT(driver);
    return driver->driver->Drive(input_type, output_uri) == HXR_OK ? 1 : 0;
}

void
hxfftdriver_stop(HXFFTDriver *driver)
{
    if(driver == NULL || driver->driver == NULL) {
        return;
    }
    
    driver->driver->Stop();
}
