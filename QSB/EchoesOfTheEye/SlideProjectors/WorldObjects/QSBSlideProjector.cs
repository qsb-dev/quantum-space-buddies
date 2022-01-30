using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.SlideProjectors.WorldObjects
{
	public class QSBSlideProjector : WorldObject<SlideProjector>
	{
		public override async UniTask Init(CancellationToken ct)
		{
			DebugLog.DebugWrite($"Init {this}");
		}

		public uint ControllingPlayer;

		public void OnChangeAuthority(uint newOwner)
		{
			DebugLog.DebugWrite($"{this} change ControllingPlayer to {newOwner}");
		}

		public override void SendInitialState(uint to)
		{
			// todo SendInitialState
		}
	}
}
