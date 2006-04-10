using System;
using System.Threading;

using Banshee.Base;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Alarm
{
    public class AlarmThread
    {
    	private AlarmPlugin plugin;
        
        public AlarmThread(AlarmPlugin plugin)
        {
        	this.plugin = plugin;
        }
        
        public void MainLoop()
        {
        	try
        	{
        		while(true)
        		{
        			Thread.Sleep(1000);
        			
        			DateTime now = DateTime.Now;
        			
        			if (now.Hour == this.plugin.AlarmHour && now.Minute == this.plugin.AlarmMinute)
        			{
        				this.StartPlaying();
        			}
        			
        		}
        	}
        	catch (ThreadAbortException tex)
        	{
        		LogCore.Instance.PushInformation("Alarm main loop aborted", "");
        	}
        }
        
        private void StartPlaying()
        {
        	if (PlayerEngineCore.CurrentState == PlayerEngineState.Playing)
        	{
        		return;
        	}
        	
        	LogCore.Instance.PushDebug("Start playing ", "");
        	
        	if (this.plugin.FadeDuration > 0)
        	{
        		ushort increment = (ushort)Math.Ceiling( (this.plugin.FadeEndVolume - this.plugin.FadeStartVolume) / this.plugin.FadeDuration);
        		
        		LogCore.Instance.PushDebug("Start fade-in", String.Format("Start fade-in from {0} to {1} in {2}s, increment is {3}", 
        			this.plugin.FadeStartVolume, this.plugin.FadeEndVolume, this.plugin.FadeDuration, increment));
        			
        		PlayerEngineCore.Volume = this.plugin.FadeStartVolume;
        		PlayerEngineCore.Play();
        		
        		for (int i = 0; i < this.plugin.FadeDuration; i++)
        		{
        			Thread.Sleep(1000);
        			if (PlayerEngineCore.Volume < this.plugin.FadeEndVolume)
        			{
        				LogCore.Instance.PushDebug("Alarm fade-in", String.Format("volume is {0} before increment", PlayerEngineCore.Volume));
        				PlayerEngineCore.Volume += increment;
        				LogCore.Instance.PushDebug("Alarm fade-in", String.Format("volume is {0} after increment", PlayerEngineCore.Volume));
        			}
        		}
        	}
        	else
        	{
        		// No fade
        		PlayerEngineCore.Play();
        	}
        }
    }
}
