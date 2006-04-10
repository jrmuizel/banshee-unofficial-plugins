using System;
using System.Threading;

using Banshee.Base;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Alarm
{
    public class AlarmThread
    {
    	private bool isInAlarmMinute = false; // what a dirty hack :(
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
        				isInAlarmMinute = true;
        			}else{
        				isInAlarmMinute = false;
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
        	if (PlayerEngineCore.CurrentState == PlayerEngineState.Playing || isInAlarmMinute)
        	{
        		return;
        	}
        	
        	LogCore.Instance.PushDebug("Start playing ", "");
        	
        	if (this.plugin.FadeDuration > 0)
        	{
        		PlayerEngineCore.Volume = plugin.FadeStartVolume;
        		PlayerEngineCore.Play();
        		
        		int ticks = plugin.FadeEndVolume - plugin.FadeStartVolume;
        		float sleep = ((float) plugin.FadeDuration / (float) ticks) * 1000;
        		for(PlayerEngineCore.Volume = plugin.FadeStartVolume;
        				PlayerEngineCore.Volume <= plugin.FadeEndVolume;
        				PlayerEngineCore.Volume++)
        		{
        			Thread.Sleep((int) sleep);
        		}
        	}else{
        		// No fade
        		PlayerEngineCore.Play();
        	}
        }
    }
}
