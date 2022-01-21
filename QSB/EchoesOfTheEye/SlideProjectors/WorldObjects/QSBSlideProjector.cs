using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.SlideProjectors.WorldObjects
{
	public class QSBSlideProjector : WorldObject<SlideProjector>
	{
		public override void Init()
		{
			base.Init();
			DebugLog.DebugWrite($"Init {LogName}");
		}

		public uint ControllingPlayer;

		public void OnChangeAuthority(uint newOwner)
		{
			DebugLog.DebugWrite($"{LogName} change ControllingPlayer to {newOwner}");
		}
	}
}
