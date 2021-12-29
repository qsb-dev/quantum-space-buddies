using QSB.EyeOfTheUniverse.VesselSync.WorldObjects;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.EyeOfTheUniverse.VesselSync
{
	internal class VesselManager : WorldObjectManager
	{
		public static VesselManager Instance { get; private set; }

		private List<PlayerInfo> _playersInCage = new();
		private QSBVesselWarpController _warpController;

		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void Awake()
		{
			base.Awake();
			Instance = this;
		}

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBVesselWarpController, VesselWarpController>();
			_warpController = QSBWorldSync.GetWorldObjects<QSBVesselWarpController>().First();
		}

		public void Enter(PlayerInfo player)
		{
			DebugLog.DebugWrite($"{player.PlayerId} enter");
			_playersInCage.Add(player);
		}

		public void Exit(PlayerInfo player)
		{
			DebugLog.DebugWrite($"{player.PlayerId} exit");
			_playersInCage.Remove(player);

			if (_playersInCage.Count == 0 && _warpController.AttachedObject._hasPower)
			{
				DebugLog.DebugWrite($"NO PLAYERS LEFT");
			}
		}
	}
}
