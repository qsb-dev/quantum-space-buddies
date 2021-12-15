using UnityEngine;

namespace QSB.Audio
{
	internal class PlayerAudioManager
	{
		public static QSBPlayerAudioController InitRemote(Transform playerBody)
		{
			var REMOTE_Audio_Player = new GameObject("REMOTE_Audio_Player");
			REMOTE_Audio_Player.transform.parent = playerBody;
			REMOTE_Audio_Player.transform.localPosition = Vector3.zero;
			REMOTE_Audio_Player.transform.localScale = Vector3.one;

			return REMOTE_Audio_Player.AddComponent<QSBPlayerAudioController>();
		}
	}
}
