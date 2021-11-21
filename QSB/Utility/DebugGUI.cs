using OWML.Utils;
using QSB.ClientServerStateSync;
using QSB.OrbSync.TransformSync;
using QSB.Player;
using QSB.QuantumSync;
using QSB.ShipSync;
using QSB.ShipSync.TransformSync;
using QSB.ShipSync.WorldObjects;
using QSB.Syncs;
using QSB.TimeSync;
using QSB.WorldSync;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

namespace QSB.Utility
{
	internal class DebugGUI : MonoBehaviour
	{
		private const float _debugLineSpacing = 8f;
		private const float FixedWidth = 200f;
		private const float Column1 = 20f;
		private float column1Offset = 10f;
		private const float Column2 = Column1 + FixedWidth;
		private float column2Offset = 10f;
		private const float Column3 = Column2 + FixedWidth;
		private float column3Offset = 10f;
		private const float Column4 = Column3 + FixedWidth;
		private float column4Offset = 10f;

		private GUIStyle guiStyle = new()
		{
			fontSize = 9
		};

		private void WriteLine(int collumnID, string text)
		{
			var currentOffset = 0f;
			var x = 0f;
			switch (collumnID)
			{
				case 1:
					x = Column1;
					currentOffset = column1Offset;
					column1Offset += _debugLineSpacing;
					break;
				case 2:
					x = Column2;
					currentOffset = column2Offset;
					column2Offset += _debugLineSpacing;
					break;
				case 3:
					x = Column3;
					currentOffset = column3Offset;
					column3Offset += _debugLineSpacing;
					break;
				case 4:
					x = Column4;
					currentOffset = column4Offset;
					column4Offset += _debugLineSpacing;
					break;
			}

			GUI.Label(new Rect(x, currentOffset, FixedWidth, 20f), text, guiStyle);
		}

		private void WriteLine(int collumnID, string text, Color color)
		{
			guiStyle.normal.textColor = color;
			WriteLine(collumnID, text);
			guiStyle.normal.textColor = Color.white;
		}

		public void OnGUI()
		{
			if (!QSBCore.DebugMode)
			{
				return;
			}

			guiStyle.normal.textColor = Color.white;
			GUI.contentColor = Color.white;

			column1Offset = 10f;
			column2Offset = 10f;
			column3Offset = 10f;
			column4Offset = 10f;

			#region Column1 - Server data
			WriteLine(1, $"FPS : {Mathf.Round(1f / Time.smoothDeltaTime)}");
			WriteLine(1, $"HasWokenUp : {QSBCore.WorldObjectsReady}");
			if (WakeUpSync.LocalInstance != null)
			{
				WriteLine(1, $"Server State : {ServerStateManager.Instance.GetServerState()}");
				var currentState = WakeUpSync.LocalInstance.CurrentState;
				WriteLine(1, $"WakeUpSync State : {currentState}");
				var reason = WakeUpSync.LocalInstance.CurrentReason;
				if (currentState == WakeUpSync.State.FastForwarding && reason != null)
				{
					WriteLine(1, $"Reason : {(FastForwardReason)reason}");
				}
				else if (currentState == WakeUpSync.State.Pausing && reason != null)
				{
					WriteLine(1, $"Reason : {(PauseReason)reason}");
				}
				else if (currentState != WakeUpSync.State.Loaded && currentState != WakeUpSync.State.NotLoaded && reason == null)
				{
					WriteLine(1, $"Reason : NULL", Color.red);
				}

				WriteLine(1, $"Time Difference : {WakeUpSync.LocalInstance.GetTimeDifference()}");
				WriteLine(1, $"Timescale : {OWTime.GetTimeScale()}");
				WriteLine(1, $"Time Remaining : {Mathf.Floor(TimeLoop.GetSecondsRemaining() / 60f)}:{Mathf.Round(TimeLoop.GetSecondsRemaining() % 60f * 100f / 100f)}");
				WriteLine(1, $"Loop Count : {TimeLoop.GetLoopCount()}");
			}
			#endregion

			#region Column2 - Player data
			WriteLine(2, $"OrbList count : {NomaiOrbTransformSync.OrbTransformSyncs.Count}");
			WriteLine(2, $"Player data :");
			foreach (var player in QSBPlayerManager.PlayerList)
			{
				if (player == null)
				{
					WriteLine(2, $"NULL PLAYER", Color.red);
					continue;
				}

				WriteLine(2, $"{player.PlayerId}.{player.Name}");
				WriteLine(2, $"State : {player.State}");
				WriteLine(2, $"Dead : {player.IsDead}");
				WriteLine(2, $"Visible : {player.Visible}");
				WriteLine(2, $"Ready : {player.IsReady}");
				WriteLine(2, $"Suited Up : {player.SuitedUp}");

				if (player.IsReady && QSBCore.WorldObjectsReady)
				{
					var networkTransform = player.TransformSync;
					var referenceSector = networkTransform.ReferenceSector;
					var referenceTransform = networkTransform.ReferenceTransform;
					var parent = networkTransform.AttachedObject?.transform.parent;

					WriteLine(2, $" - Ref. Sector : {(referenceSector == null ? "NULL" : referenceSector.Name)}", referenceSector == null ? Color.red : Color.white);
					WriteLine(2, $" - Ref. Transform : {(referenceTransform == null ? "NULL" : referenceTransform.name)}", referenceTransform == null ? Color.red : Color.white);
					WriteLine(2, $" - Parent : {(parent == null ? "NULL" : parent.name)}", parent == null ? Color.red : Color.white);
				}
			}
			#endregion

			#region Column3 - Ship data
			
			WriteLine(3, $"Current Flyer : {ShipManager.Instance.CurrentFlyer}");
			if (ShipTransformSync.LocalInstance != null)
			{
				var instance = ShipTransformSync.LocalInstance;
				if (QSBCore.IsHost)
				{
					WriteLine(3, $"Current Owner : {instance.NetIdentity.ClientAuthorityOwner.GetPlayerId()}");
				}
				var sector = instance.ReferenceSector;
				WriteLine(3, $"Ref. Sector : {(sector != null ? sector.Name : "NULL")}", sector == null ? Color.red : Color.white);
				var transform = instance.ReferenceTransform;
				WriteLine(3, $"Ref. Transform : {(transform != null ? transform.name : "NULL")}", transform == null ? Color.red : Color.white);
			}
			else
			{
				WriteLine(3, $"ShipTransformSync.LocalInstance is null.", Color.red);
			}

			WriteLine(3, $"QSBShipComponent");
			foreach (var component in QSBWorldSync.GetWorldObjects<QSBShipComponent>())
			{
				var attachedObject = component.AttachedObject;
				if (attachedObject == null)
				{
					WriteLine(3, $"- {component.ObjectId} NULL ATTACHEDOBJECT", Color.red);
				}
				else
				{
					WriteLine(3, $"- {component.AttachedObject.name} RepairFraction:{component.AttachedObject._repairFraction}");
				}
			}

			WriteLine(3, $"QSBShipHull");
			foreach (var hull in QSBWorldSync.GetWorldObjects<QSBShipHull>())
			{
				var attachedObject = hull.AttachedObject;
				if (attachedObject == null)
{
					WriteLine(3, $"- {hull.ObjectId} NULL ATTACHEDOBJECT", Color.red);
				}
				else
				{
					WriteLine(3, $"- {hull.AttachedObject.name}, Integrity:{hull.AttachedObject.integrity}");
				}
			}
			#endregion

			#region Column4 - Quantum Object Possesion
			foreach (var player in QSBPlayerManager.PlayerList)
			{
				if (player == null)
				{
					WriteLine(4, $"- NULL PLAYER", Color.red);
					continue;
				}

				WriteLine(4, $"- {player.PlayerId}.{player.Name}");
				var allQuantumObjects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
				var ownedQuantumObjects = allQuantumObjects.Where(x => x.ControllingPlayer == player.PlayerId);

				foreach (var quantumObject in ownedQuantumObjects)
				{
					var qsbObj = quantumObject as IWorldObject;
					if (qsbObj == null)
					{
						WriteLine(4, $"NULL QSBOBJ", Color.red);
					}
					else
					{
						WriteLine(4, $"{qsbObj.Name} ({qsbObj.ObjectId})");
					}
				}
			}
			#endregion
		}
	}
}