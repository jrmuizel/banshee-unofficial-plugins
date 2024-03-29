//
// BrowseService.cs
//
// Authors:
//	Aaron Bockover  <abockover@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Collections;

namespace Mono.Zeroconf
{
    public sealed class BrowseService : Service
    {
        private bool is_resolved = false;
        private bool resolve_pending = false;
        
        public event EventHandler Resolved;

        public BrowseService()
        {
        }
        
        public BrowseService(string name, string replyDomain, string regtype) : base(name, replyDomain, regtype)
        {
        }

        public void Resolve()
        {
            Resolve(false);
        }
        
        public void Resolve(bool requery)
        {
            if(resolve_pending) {
                return;
            }
        
            is_resolved = false;
            resolve_pending = true;
            
            if(requery) {
                InterfaceIndex = 0;
            }
        
            ServiceRef sd_ref;
            ServiceError error = Native.DNSServiceResolve(out sd_ref, ServiceFlags.None, 
                InterfaceIndex, Name, RegType, ReplyDomain, OnResolveReply, IntPtr.Zero);
                
            if(error != ServiceError.NoError) {
                throw new ServiceErrorException(error);
            }
            
            sd_ref.Process();
        }
        
        private void OnResolveReply(ServiceRef sdRef, ServiceFlags flags, uint interfaceIndex,
            ServiceError errorCode, string fullname, string hosttarget, ushort port, ushort txtLen, 
            IntPtr txtRecord, IntPtr contex)
        {
            is_resolved = true;
            resolve_pending = false;
            
            InterfaceIndex = interfaceIndex;
            FullName = fullname;
            HostTarget = hosttarget;
            this.port = (short)port;
            TxtRecord = new TxtRecord(txtLen, txtRecord);
            
            EventHandler handler = Resolved;
            if(handler != null) {
                handler(this, new EventArgs());
            }
            
            sdRef.Deallocate();
        }
        
        public bool IsResolved {
            get { return is_resolved; }
        }
    }
}
