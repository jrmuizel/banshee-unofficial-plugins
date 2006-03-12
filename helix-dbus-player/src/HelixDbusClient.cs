/***************************************************************************
 *  HelixDbusClient.cs
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

using System;
using System.Collections;
using DBus;
using Gtk;

namespace Helix
{
    [Interface(RemotePlayer.Interface)]
    public abstract class RemotePlayer : IDisposable
    {
        public const string Interface = "org.gnome.HelixDbusPlayer";
        public const string ServiceName = "org.gnome.HelixDbusPlayer";
        public const string ObjectPath = "/org/gnome/HelixDbusPlayer/Player";
        
        public event MessageHandler Message;
        
        private static RemotePlayer instance;

        public static RemotePlayer Connect()
        {
            if(instance == null) {
                Service service = Service.Get(Bus.GetSessionBus(), ServiceName);     
                service.SignalCalled += OnSignalCalled;   
                instance = (RemotePlayer)service.GetObject(typeof(RemotePlayer), ObjectPath);
            }
            
            return instance;
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        
        public void Dispose(bool disposing)
        {
            if(disposing) {
                GC.SuppressFinalize(this);
            }
        }
        
        private static void OnSignalCalled(Signal signal)
        {
            if(signal.PathName != ObjectPath || signal.InterfaceName != Interface 
                || signal.Name != "Message" || instance == null) {
                return;
            }
            
            instance.Signal(signal);
        }
        
        private void Signal(Signal signal)
        {
            Message message = null;
            string current_key = null;
        
            foreach(DBus.DBusType.IDBusType argument in signal.Arguments) {
                if(message == null) {
                    message = new Message((MessageType)argument.Get());
                    continue;
                }
            
                object current = argument.Get();
            
                if(current_key == null) {
                    current_key = (string)current;
                    continue;
                }
                
                message.AppendArgument(current_key, current);
                current_key = null;
            }
            
            if(message != null) {
                OnMessage(message);
            }
        }
        
        private void OnMessage(Message message)
        {
            MessageHandler handler = Message;
            if(handler != null) {
                handler(this, new MessageArgs(message));
            }
        }

        [Method] public abstract bool OpenUri(string uri);
        [Method] public abstract void Play();
        [Method] public abstract void Pause();
    }
    
    public enum MessageType {
        None = 0,
        VisualState,
        IdealSize,
        Length,
        Title,
        Groups,
        GroupStarted,
        Contacting,
        Buffering,
        ContentConcluded,
        ContentState,
        Status,
        Volume,
        Mute,
        ClipBandwidth,
        Error,
        GotoUrl,
        RequestAuthentication,
        RequestUpgrade,
        HasComponent
    }
    
    public delegate void MessageHandler(object o, MessageArgs args);
    
    public class MessageArgs
    {
        public Message message;
        
        public MessageArgs(Message message)
        {
            this.message = message;
        }

        public Message Message {
            get { return message; }
        }
        
        public MessageType Type {
            get { return message.Type; }
        }
    }
    
    public class Message : IEnumerable
    {
        private MessageType type;
        private Hashtable arguments = new Hashtable();
        
        internal Message(MessageType type)
        {
            this.type = type;
        }
        
        internal void AppendArgument(string key, object value)
        {
            arguments[key] = value;
        }
        
        public IEnumerator GetEnumerator()
        {
            return arguments.Keys.GetEnumerator();
        }
        
        public override string ToString()
        {
            string str = String.Format("{0}\n", type.ToString());
            foreach(string key in this) {
                object value = this[key];
                str += String.Format("  {0} = {1} ({2})\n", key, value, value.GetType());
            }
            
            return str;
        }
        
        public MessageType Type {
            get { return type; }
        }
        
        public object this [string key] {
            get { return arguments[key]; }
        }
    }
}

public static class HelixDbusClient
{
    public static void Main()
    {
        Application.Init();
        Helix.RemotePlayer player = Helix.RemotePlayer.Connect();
        player.Message += OnPlayerMessage;
        Console.WriteLine(player.OpenUri("file:///home/aaron/test.mp3"));
        player.Play();
        System.Threading.Thread.Sleep(1000);
        player.Pause();
        player.Dispose();
        Application.Run();
    }
    
    public static void OnPlayerMessage(object o, Helix.MessageArgs args)
    {
        Console.WriteLine(args.Message);
    }
}
