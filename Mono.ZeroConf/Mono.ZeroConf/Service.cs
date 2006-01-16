//
// Service.cs
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
using System.Runtime.InteropServices;

namespace Mono.Zeroconf
{
    public class Service
    {
        public const int MaxServiceName = 64;
        public const int MaxDomainName = 1005;
        public const int InterfaceIndexAny = 0;
        public const int InterfaceIndexLocalOnly = -1;
        
        /* DNSServiceBrowse */
     
        public delegate void BrowseReplyCallback(ServiceRef sdRef, uint interfaceIndex, ServiceError errorCode, 
            string serviceName, string regtype, string replyDomain);
     
        private delegate void DNSServiceBrowseReply(ServiceRef sdRef, ServiceFlags flags, uint interfaceIndex,
            ServiceError errorCode, string serviceName, string regtype, string replyDomain, 
            BrowseReplyCallback context);
            
        [DllImport("mdns")]
        private static extern ServiceError DNSServiceBrowse(out ServiceRef sdRef, ServiceFlags flags,
            uint interfaceIndex, string regtype, string domain, DNSServiceBrowseReply callBack, 
            BrowseReplyCallback context);
  
        private static void OnDNSServiceBrowseReply(ServiceRef sdRef, ServiceFlags flags, uint interfaceIndex,
            ServiceError errorCode, string serviceName, string regtype, string replyDomain, 
            BrowseReplyCallback callback)
        {
            if(callback != null) {
                callback(sdRef, interfaceIndex, errorCode, serviceName, regtype, replyDomain);
            }
        }
  
        public static ServiceRef Browse(uint interfaceIndex, string regtype, string domain,
            BrowseReplyCallback callback)
        {
            ServiceRef sdRef = ServiceRef.Zero;
            ServiceError result = DNSServiceBrowse(out sdRef, ServiceFlags.Default, interfaceIndex, 
                regtype, domain, OnDNSServiceBrowseReply, callback);
            ServiceErrorException.ThrowIfNecessary(result, sdRef);
            return sdRef;
        }
        /* DNSServiceResolve */
        
        public delegate void ResolveReplyCallback(ServiceRef sdRef, uint interfaceIndex, ServiceError errorCode,
            string fullname, string hosttarget, ushort port, ushort txtLen, string txtRecord);
        
        private delegate void DNSServiceResolveReply(ServiceRef sdRef, ServiceFlags flags, uint interfaceIndex,
            ServiceError errorCode, string fullname, string hosttarget, ushort port, ushort txtLen, 
            string txtRecord, ResolveReplyCallback context);
            
        [DllImport("mdns")]
        private static extern ServiceError DNSServiceResolve(out ServiceRef sdRef, ServiceFlags flags,
            uint interfaceIndex, string name, string regtype, string domain, DNSServiceResolveReply callBack,
            ResolveReplyCallback context);
            
        private static void OnDNSServiceResolveReply(ServiceRef sdRef, ServiceFlags flags, uint interfaceIndex,
            ServiceError errorCode, string fullname, string hosttarget, ushort port, ushort txtLen, 
            string txtRecord, ResolveReplyCallback callback)
        {
            if(callback != null) {
                callback(sdRef, interfaceIndex, errorCode, fullname, hosttarget, port, txtLen, txtRecord);
            }
        }
        
        public static ServiceRef Resolve(uint interfaceIndex, string name, string regtype, string domain,
            ResolveReplyCallback callback)
        {
            ServiceRef sdRef = ServiceRef.Zero;
            ServiceError result = DNSServiceResolve(out sdRef, ServiceFlags.Default, interfaceIndex, 
                name, regtype, domain, OnDNSServiceResolveReply, callback);
            ServiceErrorException.ThrowIfNecessary(result, sdRef);
            return sdRef;
        }
       
        /* DNSServiceEnumerateDomains */
        
        /*public delegate void DomainEnumReplyCallback(ServiceRef sdRef, ServiceFlags flags, uint interfaceIndex,
            ServiceError errorCode, string replyDomain, IntPtr context);
     
        [DllImport("mdns")]
        private static extern ServiceError DNSServiceEnumerateDomains(out ServiceRef sdRef, ServiceFlags flags,
            uint interfaceIndex, DomainEnumReplyCallback callBack, IntPtr context);
            
        public static ServiceError EnumerateDomains(out ServiceRef sdRef, ServiceFlags flags,
            uint interfaceIndex, DomainEnumReplyCallback callBack)
        {
            return DNSServiceEnumerateDomains(out sdRef, flags, interfaceIndex, callBack, IntPtr.Zero);
        }*/
    }
}
