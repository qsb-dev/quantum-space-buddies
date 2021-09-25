using QSB.ClientServerStateSync;
using QSB.Player;
using QSB.ProbeSync.TransformSync;
using QSB.Syncs;
using QSB.TimeSync;
using UnityEngine;

namespace QSB.Utility
{
	internal class DebugGUI : MonoBehaviour
	{
		private const float _debugLineSpacing = 8f;

		private GUIStyle guiStyle = new GUIStyle()
		{
			fontSize = 9
		};

		public void OnGUI()
		{
			if (!QSBCore.DebugMode)
			{
				return;
			}

			guiStyle.normal.textColor = Color.white;
			GUI.contentColor = Color.white;

			var offset = 10f;
			GUI.Label(new Rect(220, 10, 200f, 20f), $"FPS : {Mathf.Round(1f / Time.smoothDeltaTime)}", guiStyle);
			offset += _debugLineSpacing;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"HasWokenUp : {QSBCore.WorldObjectsReady}", guiStyle);
			offset += _debugLineSpacing;
			if (WakeUpSync.LocalInstance != null)
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Server State : {ServerStateManager.Instance.GetServerState()}", guiStyle);
				offset += _debugLineSpacing;
				var currentState = WakeUpSync.LocalInstance.CurrentState;
				GUI.Label(new Rect(220, offset, 200f, 20f), $"WakeUpSync State : {currentState}", guiStyle);
				offset += _debugLineSpacing;
				var reason = WakeUpSync.LocalInstance.CurrentReason;
				if (currentState == WakeUpSync.State.FastForwarding && reason != null)
				{
					
					GUI.Label(new Rect(220, offset, 200f, 20f), $"Reason : {(FastForwardReason)reason}", guiStyle);
					offset += _debugLineSpacing;
				}
				else if (currentState == WakeUpSync.State.Pausing && reason != null)
				{
					GUI.Label(new Rect(220, offset, 200f, 20f), $"Reason : {(PauseReason)reason}", guiStyle);
					offset += _debugLineSpacing;
				}
				else if (currentState != WakeUpSync.State.Loaded && currentState != WakeUpSync.State.NotLoaded && reason == null)
				{
					GUI.Label(new Rect(220, offset, 200f, 20f), $"Reason : NULL", guiStyle);
					offset += _debugLineSpacing;
				}
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Time Difference : {WakeUpSync.LocalInstance.GetTimeDifference()}", guiStyle);
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Timescale : {OWTime.GetTimeScale()}", guiStyle);
				offset += _debugLineSpacing;
			}

			var offset2 = 10f;
			GUI.Label(new Rect(420, offset2, 200f, 20f), $"Player data :", guiStyle);
			offset2 += _debugLineSpacing;
			foreach (var player in QSBPlayerManager.PlayerList)
			{
				GUI.Label(new Rect(420, offset2, 400f, 20f), $"{player.PlayerId}.{player.Name}", guiStyle);
				offset2 += _debugLineSpacing;
				GUI.Label(new Rect(420, offset2, 400f, 20f), $"State : {player.State}", guiStyle);
				offset2 += _debugLineSpacing;
				GUI.Label(new Rect(420, offset2, 400f, 20f), $"Dead : {player.IsDead}", guiStyle);
				offset2 += _debugLineSpacing;

				if (player.PlayerStates.IsReady && QSBCore.WorldObjectsReady)
				{
					var networkTransform = player.TransformSync;
					var sector = networkTransform.ReferenceSector;

					GUI.Label(new Rect(420, offset2, 400f, 20f), $" - L.Pos : {networkTransform.transform.localPosition}", guiStyle);
					offset2 += _debugLineSpacing;
					GUI.Label(new Rect(420, offset2, 400f, 20f), $" - Sector : {(sector == null ? "NULL" : sector.Name)}", guiStyle);
					offset2 += _debugLineSpacing;
					var probeSync = SyncBase.GetPlayers<PlayerProbeSync>(player);
					if (probeSync != default)
					{
						var probeSector = probeSync.ReferenceSector;
						GUI.Label(new Rect(420, offset2, 400f, 20f), $" - Probe Sector : {(probeSector == null ? "NULL" : probeSector.Name)}", guiStyle);
						offset2 += _debugLineSpacing;
					}
				}
			}
		}
	}
}