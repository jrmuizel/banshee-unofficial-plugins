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

#ifndef _BBINSOURCE_H
#define _BBINSOURCE_H

#include "hxsrcin.h"
#include "hxdataf.h"
#include "hxcomm.h"

#include "bbiosetup.h"

class ByteBufferInputSource : public IHXDataFile
{
private:

    INT32 ref_count;
    BBInputCallbacks *callbacks;
    void *userdata;
    IHXCommonClassFactory *common_class_factory;
    IUnknown *context;
    
public:

    ByteBufferInputSource(IUnknown *context, BBInputCallbacks *cb, void *user);
    
    STDMETHOD_(void, Bind) (THIS_ const char *FileName) ;
    STDMETHOD(Create) (THIS_ UINT16 uOpenMode) ;
    STDMETHOD(Open) (THIS_ UINT16 uOpenMode) ;
    STDMETHOD(Close)	 (THIS) ;
    STDMETHOD_(BOOL, Name) (THIS_ REF(IHXBuffer*) pFileName) ;
    STDMETHOD_(BOOL, IsOpen)	 (THIS) ;
    STDMETHOD(Seek) (THIS_ ULONG32 offset, UINT16 fromWhere) ;
    STDMETHOD_(ULONG32, Tell) (THIS) ;
    STDMETHOD_(ULONG32, Read) (THIS_ REF(IHXBuffer*) pBuf, ULONG32 count) ;
    STDMETHOD_(ULONG32, Write) (THIS_ REF(IHXBuffer*) pBuf) ;
    STDMETHOD(Flush)	 (THIS) ;
    STDMETHOD(Stat) (THIS_ struct stat* buffer) ;
    STDMETHOD(Delete) (THIS) ;
    STDMETHOD_(INT16, GetFd) (THIS) ;
    STDMETHOD(GetLastError) (THIS) ;
    STDMETHOD_(void, GetLastError) (THIS_ REF(IHXBuffer*) err) ;
					
    STDMETHOD(QueryInterface) (THIS_ REFIID riid, void **ppvObj);
    STDMETHOD_(UINT32, AddRef) (THIS);
    STDMETHOD_(UINT32, Release) (THIS);    
};

#endif /* _BBINSOURCE_H */
