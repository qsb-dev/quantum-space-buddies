using QSB.Player;
using QSB.PlayerBodySetup.Remote;
using UnityEngine;

namespace QSB.Tools.ProbeTool
{
	internal static class ProbeCreator
	{
		public static void CreateProbe(Transform newProbe, PlayerInfo player)
		{
			var qsbProbe = newProbe.gameObject.GetComponent<QSBProbe>();
			player.Probe = qsbProbe;
			qsbProbe.SetOwner(player);

			FixMaterialsInAllChildren.ReplaceMaterials(newProbe);

			newProbe.gameObject.SetActive(true);
		}
	}
}
