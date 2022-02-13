using QSB.Player;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace QSB.Tools.ProbeTool
{
	internal static class ProbeCreator
	{
		public static void CreateProbe(Transform newProbe, PlayerInfo player)
		{
			var qsbProbe = newProbe.gameObject.GetComponent<QSBProbe>();
			player.Probe = qsbProbe;
			qsbProbe.SetOwner(player);

			newProbe.gameObject.SetActive(true);
		}
	}
}
