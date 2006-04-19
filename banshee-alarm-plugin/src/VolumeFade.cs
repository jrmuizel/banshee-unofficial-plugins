using System;
using System.Threading;

using Banshee.Base;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Alarm
{
	public class VolumeFade
	{
		float sleep;
		ushort endVolume;
		int increment;
		ushort curVolume;

		public VolumeFade(ushort start, ushort end, ushort duration)
		{
			sleep = ((float) duration / (float) Math.Abs(end - start)) * 1000;
			increment = (ushort) (start < end ? 1 : -1);
			endVolume = end;
			curVolume = start;
			GLib.Timeout.Add((uint) sleep, VolumeFadeTick);
		}
		
		private bool VolumeFadeTick(){
			if(PlayerEngineCore.Volume == endVolume){
				LogCore.Instance.PushDebug("Volume Fade: Done.","");
				return false;
			}
			LogCore.Instance.PushDebug("Volume Fade: Fading a notch...",
					String.Format("Vol={0}, End={1}, inc={2}",
						PlayerEngineCore.Volume,
						endVolume,
						increment
					));
			
			if(increment == 1)
				curVolume++;
			else
				curVolume--;
			
			PlayerEngineCore.Volume = curVolume;
			return true;
		}
		
	}
}
