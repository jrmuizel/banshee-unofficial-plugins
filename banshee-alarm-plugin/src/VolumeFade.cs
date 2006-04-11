using System;
using System.Threading;

using Banshee.Base;
using Banshee.MediaEngine;

namespace Banshee.Plugins.Alarm
{
	public class VolumeFade
	{
		public VolumeFade(ushort start, ushort end, ushort duration)
		{
			int ticks = Math.Abs(end - start);
			float sleep = ((float) duration / (float) ticks) * 1000;

			for(PlayerEngineCore.Volume = start;
				PlayerEngineCore.Volume != end;
				PlayerEngineCore.Volume += (ushort) (start < end ? 1 : -1))
			{
				Thread.Sleep((int) sleep);
			}
		}
	}
}