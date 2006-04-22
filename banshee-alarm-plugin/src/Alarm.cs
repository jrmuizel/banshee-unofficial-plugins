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
        			
        			if (now.Hour == plugin.AlarmHour && now.Minute == plugin.AlarmMinute && plugin.AlarmEnabled)
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
        		LogCore.Instance.PushDebug("Alarm main loop aborted", "");
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
        		
        		new VolumeFade(plugin.FadeStartVolume, plugin.FadeEndVolume, plugin.FadeDuration);
        	}else{
        		// No fade
        		PlayerEngineCore.Play();
        	}
        }
    }
}
