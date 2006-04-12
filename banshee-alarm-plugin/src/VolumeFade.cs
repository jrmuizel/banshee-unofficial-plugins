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
		ushort increment;
		
		public VolumeFade(ushort start, ushort end, ushort duration)
		{
			sleep = ((float) duration / (float) Math.Abs(end - start)) * 1000;
			increment = (ushort) (start < end ? 1 : -1);
			endVolume = end;
			GLib.Timeout.Add((uint) sleep, VolumeFadeTick);
		}
		
		private bool VolumeFadeTick(){
			if(PlayerEngineCore.Volume == endVolume){
				LogCore.Instance.PushDebug("Sleep Timer: Done fading.  Should pause soon.", "");
				return false;
			}
			LogCore.Instance.PushDebug("Sleep Timer: Fading down a notch.", "");
			PlayerEngineCore.Volume += increment;
			return true;
		}
		
	}
}
