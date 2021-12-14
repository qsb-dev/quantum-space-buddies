using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Audio
{
	internal class PlayerAudioManager
	{
		public static QSBPlayerAudioController InitRemote(Transform playerBody)
		{
			DebugLog.DebugWrite($"InitRemote {playerBody.name}");
			var REMOTE_Audio_Player = new GameObject("REMOTE_Audio_Player");
			REMOTE_Audio_Player.transform.parent = playerBody;
			REMOTE_Audio_Player.transform.localPosition = Vector3.zero;
			REMOTE_Audio_Player.transform.localScale = Vector3.one;

			return REMOTE_Audio_Player.AddComponent<QSBPlayerAudioController>();
		}
	}
}
