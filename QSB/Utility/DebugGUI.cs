using QSB.ClientServerStateSync;
using QSB.OrbSync;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.ShipSync;
using QSB.ShipSync.TransformSync;
using QSB.ShipSync.WorldObjects;
using QSB.TimeSync;
using QSB.WorldSync;
using System.Linq;
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
		private const int MaxLabelSize = 15;
		private const float MaxLabelDistance = 150;

		private readonly GUIStyle guiGUIStyle = new();
		private static readonly GUIStyle labelGUIStyle = new();

		private void Awake()
		{
			enabled = QSBCore.DebugSettings.DrawGui;

			guiGUIStyle.fontSize = 9;
		}

		private void WriteLine(int columnID, string text)
		{
			var currentOffset = 0f;
			var x = 0f;
			switch (columnID)
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

			GUI.Label(new Rect(x, currentOffset, 0, 0), text, guiGUIStyle);
		}

		private void WriteLine(int columnID, string text, Color color)
		{
			guiGUIStyle.normal.textColor = color;
			WriteLine(columnID, text);
			guiGUIStyle.normal.textColor = Color.white;
		}

		public void OnGUI()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			guiGUIStyle.normal.textColor = Color.white;
			GUI.contentColor = Color.white;

			column1Offset = 10f;
			column2Offset = 10f;
			column3Offset = 10f;
			column4Offset = 10f;

			#region Column1 - Server data

			WriteLine(1, $"FPS : {Mathf.Round(1f / Time.smoothDeltaTime)}");
			WriteLine(1, $"HasWokenUp : {QSBWorldSync.AllObjectsReady}");
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
				WriteLine(1, $"TimeLoop Initialized : {TimeLoop._initialized}");
				if (TimeLoop._initialized)
				{
					WriteLine(1, $"TimeLoop IsTimeFlowing : {TimeLoop.IsTimeFlowing()}");
					WriteLine(1, $"TimeLoop IsTimeLoopEnabled : {TimeLoop.IsTimeLoopEnabled()}");
				}
			}

			#endregion

			#region Column2 - Player data

			WriteLine(2, $"OrbList count : {OrbManager.Orbs.Count}");
			WriteLine(2, $"Player data :");
			foreach (var player in QSBPlayerManager.PlayerList)
			{
				WriteLine(2, $"{player.PlayerId}.{player.Name}");
				WriteLine(2, $"State : {player.State}");
				WriteLine(2, $"Eye State : {player.EyeState}");
				WriteLine(2, $"Dead : {player.IsDead}");
				WriteLine(2, $"Visible : {player.Visible}");
				WriteLine(2, $"Ready : {player.IsReady}");
				WriteLine(2, $"Suited Up : {player.SuitedUp}");

				if (player.IsReady && QSBWorldSync.AllObjectsReady)
				{
					var networkTransform = player.TransformSync;
					var referenceSector = networkTransform.ReferenceSector;
					var referenceTransform = networkTransform.ReferenceTransform;

					WriteLine(2, $" - Ref. Sector : {(referenceSector == null ? "NULL" : referenceSector.Name)}", referenceSector == null ? Color.red : Color.white);
					WriteLine(2, $" - Ref. Transform : {(referenceTransform == null ? "NULL" : referenceTransform.name)}", referenceTransform == null ? Color.red : Color.white);
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
					var currentOwner = instance.netIdentity.connectionToClient;
					if (currentOwner == null)
					{
						WriteLine(3, $"Current Owner : NULL");
					}
					else
					{
						WriteLine(3, $"Current Owner : {currentOwner.GetPlayerId()}");
					}
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
				WriteLine(4, $"- {player.PlayerId}.{player.Name}");
				var allQuantumObjects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
				var ownedQuantumObjects = allQuantumObjects.Where(x => x.ControllingPlayer == player.PlayerId);

				foreach (var quantumObject in ownedQuantumObjects)
				{
					WriteLine(4, $"{quantumObject.Name} ({quantumObject.ObjectId})");
				}
			}

			WriteLine(4, $"");
			WriteLine(4, $"Enabled QuantumObjects :");
			foreach (var qo in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>())
			{
				if (qo.ControllingPlayer != 0)
				{
					continue;
				}

				if (qo.IsEnabled)
				{
					WriteLine(4, $"{qo.Name} ({qo.ObjectId})");
				}
			}

			#endregion

			DrawWorldObjectLabels();
		}

		public void OnRenderObject() => DrawWorldObjectLines();

		private static void DrawWorldObjectLabels()
		{
			if (!QSBCore.DebugSettings.ShowDebugLabels)
			{
				return;
			}

			foreach (var obj in QSBWorldSync.GetWorldObjects())
			{
				if (obj.ReturnObject() == null)
				{
					return;
				}

				if (obj.ShouldDisplayDebug())
				{
					DrawLabel(obj.ReturnObject().transform, obj.ReturnLabel());
				}
			}
		}

		private static void DrawWorldObjectLines()
		{
			if (!QSBCore.DebugSettings.DrawLines)
			{
				return;
			}

			foreach (var obj in QSBWorldSync.GetWorldObjects())
			{
				if (obj.ReturnObject() == null)
				{
					return;
				}

				if (obj.ShouldDisplayDebug())
				{
					obj.DisplayLines();
				}
			}
		}

		public static void DrawLabel(Transform obj, string label)
		{
			var camera = Locator.GetPlayerCamera();

			if (camera == null)
			{
				return;
			}

			if (obj == null)
			{
				return;
			}

			labelGUIStyle.normal.textColor = Color.white;
			GUI.contentColor = Color.white;

			var difference = obj.transform.position - camera.transform.position;

			if (Vector3.Dot(difference.normalized, camera.transform.forward) < 0)
			{
				return;
			}

			var cheapDistance = difference.sqrMagnitude;

			if (cheapDistance > MaxLabelDistance * MaxLabelDistance)
			{
				return;
			}

			var screenPosition = camera.WorldToScreenPoint(obj.position);
			var distance = screenPosition.z;

			if (distance <= 0.05f)
			{
				return;
			}

			if (distance > MaxLabelDistance)
			{
				return;
			}

			if (screenPosition.x < 0 || screenPosition.x > Screen.width)
			{
				return;
			}

			if (screenPosition.y < 0 || screenPosition.y > Screen.height)
			{
				return;
			}

			var mappedFontSize = (int)distance.Map(0, MaxLabelDistance, MaxLabelSize, 0, true);

			if (mappedFontSize <= 0)
			{
				return;
			}

			if (mappedFontSize > MaxLabelSize)
			{
				return;
			}

			labelGUIStyle.fontSize = mappedFontSize;

			// WorldToScreenPoint's (0,0) is at screen bottom left, GUI's (0,0) is at screen top left. grrrr
			screenPosition.y = Screen.height - screenPosition.y;
			GUI.Label(new Rect(screenPosition, Vector2.zero), label, labelGUIStyle);
		}
	}
}
