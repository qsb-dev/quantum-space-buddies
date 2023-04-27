using Mirror;
using QSB.ClientServerStateSync;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.HUD;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.ShipSync;
using QSB.ShipSync.TransformSync;
using QSB.ShipSync.WorldObjects;
using QSB.TimeSync;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Utility;

internal class DebugGUI : MonoBehaviour, IAddComponentOnStart
{
	private const float _debugLineSpacing = 8f;
	private const float FixedWidth = 200f;
	private const float Column1 = 20f;
	private static float column1Offset = 10f;
	private const float Column2 = Column1 + FixedWidth;
	private static float column2Offset = 10f;
	private const float Column3 = Column2 + FixedWidth;
	private static float column3Offset = 10f;
	private const float Column4 = Column3 + FixedWidth;
	private static float column4Offset = 10f;
	private const int MaxLabelSize = 15;
	private const float MaxLabelDistance = 150;

	private static readonly GUIStyle guiGUIStyle = new();
	private static readonly GUIStyle labelGUIStyle = new();

	private void Awake()
	{
		enabled = QSBCore.DebugSettings.DebugMode;

		guiGUIStyle.fontSize = 9;
	}

	private static void WriteLine(int columnID, string text)
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

	private static void WriteLine(int columnID, string text, Color color)
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

		DrawGui();
		DrawWorldObjectLabels();
	}

	private static void DrawGui()
	{
		guiGUIStyle.normal.textColor = Color.white;
		GUI.contentColor = Color.white;

		column1Offset = 10f;
		column2Offset = 10f;
		column3Offset = 10f;
		column4Offset = 10f;

		#region Column1 - Server data

		WriteLine(1, $"FPS : {Mathf.Round(1f / Time.smoothDeltaTime)}");
		WriteLine(1, $"Ping : {Math.Round(NetworkTime.rtt * 1000.0)} ms");
		if (!QSBCore.DebugSettings.DrawGui)
		{
			return;
		}

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
				WriteLine(1, "Reason : NULL", Color.red);
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

			WriteLine(1, $"Selected WorldObject : {(DebugActions.WorldObjectSelection == null ? "All" : DebugActions.WorldObjectSelection.Name)}");

			if (Locator.GetDeathManager() != null)
			{
				WriteLine(1, $"Invincible : {Locator.GetDeathManager()._invincible}");
			}

			if (Locator.GetToolModeSwapper() != null)
			{
				WriteLine(1, $"Tool Mode : {Locator.GetToolModeSwapper().GetToolMode()}");
			}
			
			WriteLine(1, $"Input Mode Stack :");
			foreach (var item in OWInput.GetInputModeStack())
			{
				WriteLine(1, $" - {item}");
			}

			WriteLine(1, $"HUD Icon Stack :");
			foreach (var item in MultiplayerHUDManager.HUDIconStack)
			{
				WriteLine(1, $" - {item}");
			}
	
			WriteLine(1, $"Sectors :");
			foreach (var sector in PlayerTransformSync.LocalInstance.SectorDetector.SectorList)
			{
				WriteLine(1, $"- {sector.Name}");
			}
		}

		#endregion

		#region Column2 - Player data

		WriteLine(2, "Player data :");
		foreach (var player in QSBPlayerManager.PlayerList)
		{
			WriteLine(2, player.ToString(), Color.cyan);
			WriteLine(2, $"State : {player.State}");
			WriteLine(2, $"Eye State : {player.EyeState}");
			WriteLine(2, $"Dead : {player.IsDead}");
			WriteLine(2, $"Ready : {player.IsReady}");
			WriteLine(2, $"Suited Up : {player.SuitedUp}");
			WriteLine(2, $"In Suited Up State : {player.AnimationSync?.InSuitedUpState}");
			WriteLine(2, $"InDreamWorld : {player.InDreamWorld}");

			if (player.IsReady && QSBWorldSync.AllObjectsReady)
			{
				WriteLine(2, $"Illuminated : {player.LightSensor?.IsIlluminated()}");
				var singleLightSensor = (SingleLightSensor)player.LightSensor;
				// will be null for remote player light sensors
				if (singleLightSensor?._lightSources != null)
				{
					foreach (var item in singleLightSensor._lightSources)
					{
						WriteLine(2, $"- {item.GetLightSourceType()}");
					}
				}

				var networkTransform = player.TransformSync;
				var referenceSector = networkTransform.ReferenceSector;
				var referenceTransform = networkTransform.ReferenceTransform;

				WriteLine(2, $" - Ref. Sector : {(referenceSector == null ? "NULL" : referenceSector.Name)}", referenceSector == null ? Color.red : Color.white);
				WriteLine(2, $" - Ref. Transform : {(referenceTransform == null ? "NULL" : referenceTransform.name)}", referenceTransform == null ? Color.red : Color.white);
			}
		}

		#endregion

		if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
		{
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
						WriteLine(3, "Current Owner : NULL");
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
				WriteLine(3, "ShipTransformSync.LocalInstance is null.", Color.red);
			}

			WriteLine(3, "QSBShipComponent");
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

			WriteLine(3, "QSBShipHull");
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

			if (QSBWorldSync.AllObjectsReady && QSBCore.DLCInstalled)
			{
				var ghost = QSBWorldSync.GetWorldObjects<QSBGhostBrain>().First(x => x.AttachedObject._name == "Kamaji");
				WriteLine(4, ghost.AttachedObject._name);
				WriteLine(4, $"Action:{ghost.GetCurrentActionName()}");
				WriteLine(4, $"Threat Awareness:{ghost.GetThreatAwareness()}");
				var interestedPlayer = ghost._data.interestedPlayer;
				WriteLine(4, $"InterestedPlayer:{(interestedPlayer == null ? "NULL" : interestedPlayer.player.PlayerId)}");

				foreach (var player in ghost._data.players.Values)
				{
					WriteLine(4, $"{player.player.PlayerId}");
					WriteLine(4, $"- isPlayerVisible:{player.sensor.isPlayerVisible}");
					WriteLine(4, $"- isPlayerHeldLanternVisible:{player.sensor.isPlayerHeldLanternVisible}");
					WriteLine(4, $"- isIlluminatedByPlayer:{player.sensor.isIlluminatedByPlayer}");
					WriteLine(4, $"- isPlayerLocationKnown:{player.isPlayerLocationKnown}");
					WriteLine(4, $"- timeSincePlayerLocationKnown:{player.timeSincePlayerLocationKnown}");
					var lantern = player.player.AssignedSimulationLantern;
					if (lantern != null)
					{
						WriteLine(4, $"- IsHeldByPlayer:{lantern.AttachedObject.GetLanternController().IsHeldByPlayer()}");
						WriteLine(4, $"- Concealed:{lantern.AttachedObject.GetLanternController().IsConcealed()}");
					}
					else
					{
						WriteLine(4, "- LANTERN NULL", Color.red);
					}

					var playerCamera = player.player.Camera;

					if (playerCamera != null)
					{
						var position = playerCamera.transform.position;
						WriteLine(4, $"- Camera in vision cone:{ghost.AttachedObject._sensors.CheckPointInVisionCone(position)}");
						WriteLine(4, $"- CheckLineOccluded:{ghost.AttachedObject._sensors.CheckLineOccluded(ghost.AttachedObject._sensors._sightOrigin.position, position)}");
					}
					else
					{
						WriteLine(4, "- CAMERA NULL", Color.red);
					}
				}
			}
		}

		/*
		#region Column4 - Quantum Object Possesion

		foreach (var player in QSBPlayerManager.PlayerList)
		{
			WriteLine(4, $"- {player}");
			var allQuantumObjects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
			var ownedQuantumObjects = allQuantumObjects.Where(x => x.ControllingPlayer == player.PlayerId);

			foreach (var quantumObject in ownedQuantumObjects)
			{
				WriteLine(4, $"{quantumObject.Name} ({quantumObject.ObjectId})");
				WriteLine(4, $" - IsIlluminated:{quantumObject.GetVisibilityObject().IsIlluminated()}");
				WriteLine(4, $" - IsVisible:{quantumObject.GetVisibilityObject().IsVisible()}");
				foreach (var tracker in quantumObject.GetVisibilityObject()._visibilityTrackers)
				{
					WriteLine(4, $" - {tracker.name}");
					WriteLine(4, $"    - IsVisible:{tracker.IsVisible()}");
					WriteLine(4, $"    - IsVisibleUsingCameraFrustum:{tracker.IsVisibleUsingCameraFrustum()}");
				}
			}
		}

		WriteLine(4, "");
		WriteLine(4, "Enabled QuantumObjects :");
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
		*/
	}

	private static void DrawWorldObjectLabels()
	{
		if (QSBCore.DebugSettings.DrawLabels)
		{
			var list = DebugActions.WorldObjectSelection == null
				? QSBWorldSync.GetWorldObjects()
				: QSBWorldSync.GetWorldObjects(DebugActions.WorldObjectSelection);

			foreach (var obj in list)
			{
				if (obj.ShouldDisplayDebug())
				{
					DrawLabel(obj.AttachedObject.transform, obj.ReturnLabel());
				}
			}
		}
		else if (QSBCore.DebugSettings.DrawGhostAI)
		{
			foreach (var obj in QSBWorldSync.GetWorldObjects<IGhostObject>())
			{
				if (obj.ShouldDisplayDebug())
				{
					DrawLabel(obj.AttachedObject.transform, obj.ReturnLabel());
				}
			}
		}
	}

	public void OnRenderObject() => DrawWorldObjectLines();

	private static void DrawWorldObjectLines()
	{
		if (QSBCore.DebugSettings.DrawLines)
		{
			var list = DebugActions.WorldObjectSelection == null
				? QSBWorldSync.GetWorldObjects()
				: QSBWorldSync.GetWorldObjects(DebugActions.WorldObjectSelection);

			foreach (var obj in list)
			{
				if (obj.ShouldDisplayDebug())
				{
					obj.DisplayLines();
				}
			}
		}
		else if (QSBCore.DebugSettings.DrawGhostAI)
		{
			foreach (var obj in QSBWorldSync.GetWorldObjects<IGhostObject>())
			{
				if (obj.ShouldDisplayDebug())
				{
					obj.DisplayLines();
				}
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
