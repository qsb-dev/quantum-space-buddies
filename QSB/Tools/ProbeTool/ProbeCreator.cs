using QSB.Player;
using QSB.PlayerBodySetup.Remote;
using UnityEngine;

namespace QSB.Tools.ProbeTool;

internal static class ProbeCreator
{
	private static GameObject _prefab;

	private static GameObject GetPrefab()
	{
		if (_prefab != null)
		{
			return _prefab;
		}

		_prefab = QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/REMOTE_Probe_Body.prefab");
		ShaderReplacer.ReplaceShaders(_prefab);
		FontReplacer.ReplaceFonts(_prefab);
		QSBDopplerFixer.AddDopplerFixers(_prefab);
		return _prefab;
	}

	public static Transform CreateProbe(PlayerInfo player)
	{
		var REMOTE_Probe_Body = Object.Instantiate(GetPrefab());

		var qsbProbe = REMOTE_Probe_Body.GetComponent<QSBProbe>();
		player.Probe = qsbProbe;
		qsbProbe.SetOwner(player);

		player.ProbeBody = REMOTE_Probe_Body;

		return REMOTE_Probe_Body.transform;
	}
}