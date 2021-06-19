using QSB.Player.TransformSync;
using QSB.Player;
using QSB.ProbeSync.TransformSync;
using QSB.QuantumSync;
using QSB.ShipSync.TransformSync;
using QSB.ShipSync;
using QSB.TimeSync;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Utility
{
	class DebugGUI : MonoBehaviour
	{
		private const float _debugLineSpacing = 11f;

		public void OnGUI()
		{
			if (!QSBCore.DebugMode)
			{
				return;
			}

			var offset = 10f;
			GUI.Label(new Rect(220, 10, 200f, 20f), $"FPS : {Mathf.Round(1f / Time.smoothDeltaTime)}");
			offset += _debugLineSpacing;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"HasWokenUp : {QSBCore.WorldObjectsReady}");
			offset += _debugLineSpacing;
			if (WakeUpSync.LocalInstance != null)
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Time Difference : {WakeUpSync.LocalInstance.GetTimeDifference()}");
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Timescale : {OWTime.GetTimeScale()}");
				offset += _debugLineSpacing;
			}

			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var offset3 = 10f;
			var playerSector = PlayerTransformSync.LocalInstance.ReferenceSector;
			var playerText = playerSector == null ? "NULL" : playerSector.Name;
			GUI.Label(new Rect(420, offset3, 400f, 20f), $"Current sector : {playerText}");
			offset3 += _debugLineSpacing;
			var probeSector = PlayerProbeSync.LocalInstance.ReferenceSector;
			var probeText = probeSector == null ? "NULL" : probeSector.Name;

			GUI.Label(new Rect(420, offset3, 200f, 20f), $"Player sectors :");
			offset3 += _debugLineSpacing;
			foreach (var sector in PlayerTransformSync.LocalInstance.SectorSync.SectorList)
			{
				GUI.Label(new Rect(420, offset3, 400f, 20f), $"- {sector.Name} : {SectorSync.SectorSync.CalculateSectorScore(sector, Locator.GetPlayerTransform(), Locator.GetPlayerBody())}");
				offset3 += _debugLineSpacing;
			}

			GUI.Label(new Rect(420, offset3, 200f, 20f), $"Current Flyer : {ShipManager.Instance.CurrentFlyer}");
			offset3 += _debugLineSpacing;
			var ship = ShipTransformSync.LocalInstance;
			if (ship == null)
			{
				GUI.Label(new Rect(420, offset3, 200f, 20f), $"SHIP INSTANCE NULL");
				offset3 += _debugLineSpacing;
			}
			else
			{
				GUI.Label(new Rect(420, offset3, 200f, 20f), $"In control of ship? : {ship.HasAuthority}");
				offset3 += _debugLineSpacing;
				GUI.Label(new Rect(420, offset3, 400f, 20f), $"Ship sector : {(ship.ReferenceSector == null ? "NULL" : ship.ReferenceSector.Name)}");
				offset3 += _debugLineSpacing;
				if (ship.ReferenceTransform != null)
				{
					GUI.Label(new Rect(420, offset3, 400f, 20f), $"Ship relative velocity : {ship.GetRelativeVelocity()}");
					offset3 += _debugLineSpacing;
					GUI.Label(new Rect(420, offset3, 400f, 20f), $"Ship velocity : {ship.AttachedObject.GetVelocity()}");
					offset3 += _debugLineSpacing;
					GUI.Label(new Rect(420, offset3, 400f, 20f), $"Static Frame velocity : {Locator.GetCenterOfTheUniverse().GetStaticFrameWorldVelocity()}");
					offset3 += _debugLineSpacing;
					GUI.Label(new Rect(420, offset3, 400f, 20f), $"Reference point velocity : {ship.ReferenceTransform.GetAttachedOWRigidbody().GetPointVelocity(ship.AttachedObject.transform.position)}");
					offset3 += _debugLineSpacing;
					GUI.Label(new Rect(420, offset3, 400f, 20f), $"Ship velocity mag. : {ship.GetVelocityChangeMagnitude()}");
					offset3 += _debugLineSpacing;
				}

				GUI.Label(new Rect(420, offset3, 200f, 20f), $"Ship sectors :");
				offset3 += _debugLineSpacing;
				foreach (var sector in ship.SectorSync.SectorList)
				{
					GUI.Label(new Rect(420, offset3, 400f, 20f), $"- {sector.Name} : {SectorSync.SectorSync.CalculateSectorScore(sector, Locator.GetShipTransform(), Locator.GetShipBody())}");
					offset3 += _debugLineSpacing;
				}
			}

			var offset2 = 10f;
			GUI.Label(new Rect(620, offset2, 200f, 20f), $"Owned Objects :");
			offset2 += _debugLineSpacing;
			foreach (var obj in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().Where(x => x.ControllingPlayer == QSBPlayerManager.LocalPlayerId))
			{
				GUI.Label(new Rect(620, offset2, 200f, 20f), $"- {(obj as IWorldObject).Name}, {obj.ControllingPlayer}, {obj.IsEnabled}");
				offset2 += _debugLineSpacing;
			}

			GUI.Label(new Rect(220, offset, 200f, 20f), $"Player data :");
			offset += _debugLineSpacing;
			foreach (var player in QSBPlayerManager.PlayerList.Where(x => x.PlayerStates.IsReady))
			{
				var networkTransform = player.TransformSync;
				var sector = networkTransform.ReferenceSector;

				GUI.Label(new Rect(220, offset, 400f, 20f), $"{player.PlayerId} - L.Pos : {networkTransform.transform.localPosition}");
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 400f, 20f), $" - Sector : {(sector == null ? "NULL" : sector.Name)}");
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 400f, 20f), $" - L.Accel : {player.JetpackAcceleration?.LocalAcceleration}");
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 400f, 20f), $" - Thrusting : {player.JetpackAcceleration?.IsThrusting}");
				offset += _debugLineSpacing;
			}
		}
	}
}