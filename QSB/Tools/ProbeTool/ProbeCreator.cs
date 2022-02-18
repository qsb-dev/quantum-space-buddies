using QSB.Player;
using QSB.PlayerBodySetup.Remote;
using UnityEngine;

namespace QSB.Tools.ProbeTool
{
	internal static class ProbeCreator
	{
		public static Transform CreateProbe(PlayerInfo player)
		{
			var REMOTE_Probe_Body = Object.Instantiate(QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/REMOTE_Probe_Body.prefab")).transform;

			var qsbProbe = REMOTE_Probe_Body.gameObject.GetComponent<QSBProbe>();
			player.Probe = qsbProbe;
			qsbProbe.SetOwner(player);

			FixMaterialsInAllChildren.ReplaceMaterials(REMOTE_Probe_Body);

			player.ProbeBody = REMOTE_Probe_Body.gameObject;

			return REMOTE_Probe_Body;
		}
	}
}
