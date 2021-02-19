using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using QSB.ConversationSync;
using QSB.ElevatorSync;
using QSB.GeyserSync;
using QSB.OrbSync;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync;
using QSB.SectorSync;
using QSB.TimeSync;
using QSB.TranslationSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using System.Linq;
using System.Reflection;
using UnityEngine;

/*
	Copyright (C) 2020 - 2021
			Henry Pointer (_nebula / misternebula), 
			Aleksander Waage (AmazingAlek), 
			Ricardo Lopes (Raicuparta)
	
	This program is free software: you can redistribute it and/or
	modify it under the terms of the GNU Affero General Public License
	as published by the Free Software Foundation, either version 3 of
	the License, or (at your option) any later version.

	This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
	without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
	See the GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License along with this program. If not, see https://www.gnu.org/licenses/.
*/

namespace QSB
{
	public class QSBCore : ModBehaviour
	{
		public static IModHelper Helper { get; private set; }
		public static string DefaultServerIP { get; private set; }
		public static int Port { get; private set; }
		public static bool DebugMode { get; private set; }
		public static bool ShowLinesInDebug { get; private set; }
		public static AssetBundle NetworkAssetBundle { get; private set; }
		public static AssetBundle InstrumentAssetBundle { get; private set; }
		public static AssetBundle ConversationAssetBundle { get; private set; }
		public static bool HasWokenUp { get; set; }
		public static bool IsServer => QNetworkServer.active;
		public static bool IsInMultiplayer => QNetworkManager.singleton.isNetworkActive;
		public static GameObject GameObjectInstance => _thisInstance.gameObject;

		private static QSBCore _thisInstance;

		public void Awake()
		{
			Application.runInBackground = true;

			_thisInstance = this;

			var instance = TextTranslation.Get().GetValue<TextTranslation.TranslationTable>("m_table");
			instance.theUITable[(int)UITextType.PleaseUseController] =
				"<color=orange>Quantum Space Buddies</color> is best experienced with friends...";
		}

		public void Start()
		{
			Helper = ModHelper;
			DebugLog.ToConsole($"* Start of QSB version {Helper.Manifest.Version} - authored by {Helper.Manifest.Author}", MessageType.Info);

			NetworkAssetBundle = Helper.Assets.LoadBundle("assets/network");
			InstrumentAssetBundle = Helper.Assets.LoadBundle("assets/instruments");
			ConversationAssetBundle = Helper.Assets.LoadBundle("assets/conversation");

			QSBPatchManager.Init();
			QSBPatchManager.DoPatchType(QSBPatchTypes.OnModStart);

			gameObject.AddComponent<QSBNetworkManager>();
			gameObject.AddComponent<QNetworkManagerHUD>();
			gameObject.AddComponent<DebugActions>();
			gameObject.AddComponent<ElevatorManager>();
			gameObject.AddComponent<GeyserManager>();
			gameObject.AddComponent<OrbManager>();
			gameObject.AddComponent<QSBSectorManager>();
			gameObject.AddComponent<ConversationManager>();
			gameObject.AddComponent<QSBInputManager>();
			gameObject.AddComponent<TimeSyncUI>();
			gameObject.AddComponent<QuantumManager>();
			gameObject.AddComponent<SpiralManager>();
			gameObject.AddComponent<RepeatingManager>();

			DebugBoxManager.Init();

			// Stop players being able to pause
			Helper.HarmonyHelper.EmptyMethod(typeof(OWTime).GetMethod("Pause"));
		}

		public void Update() =>
			QNetworkIdentity.UNetStaticUpdate();

		public void OnGUI()
		{
			GUI.Label(new Rect(220, 10, 200f, 20f), $"Rough FPS : {1f / Time.smoothDeltaTime}");
			GUI.Label(new Rect(220, 40, 200f, 20f), $"HasWokenUp : {QSBCore.HasWokenUp}");

			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode)
			{
				return;
			}

			if (QSBSceneManager.CurrentScene != OWScene.SolarSystem)
			{
				return;
			}

			var offset = 70f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"QM Visible : {Locator.GetQuantumMoon().IsVisible()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"QM Locked : {Locator.GetQuantumMoon().IsLocked()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"QM Illuminated : {Locator.GetQuantumMoon().IsIlluminated()}");
			offset += 30f;
			//GUI.Label(new Rect(220, offset, 200f, 20f), $"Shrine player in dark? : {QuantumManager.Instance.Shrine.IsPlayerInDarkness()}");
			//offset += 30f;
			var tracker = Locator.GetQuantumMoon().GetValue<ShapeVisibilityTracker>("_visibilityTracker");
			foreach (var camera in QSBPlayerManager.GetPlayerCameras())
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- {camera.name} : {tracker.GetType().GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tracker, new object[] { camera.GetFrustumPlanes() })}");
				offset += 30f;
			}

			// Used for diagnosing specific socketed objects. Just set <index> to be the correct index.
			/*
			var index = 110;
			var socketedObject = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(index);
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Controller : {socketedObject.ControllingPlayer}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Visible : {socketedObject.AttachedObject.IsVisible()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Locked : {socketedObject.AttachedObject.IsLocked()}");
			offset += 30f;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"{index} Illuminated : {socketedObject.AttachedObject.IsIlluminated()}");
			offset += 30f;
			var socketedTrackers = socketedObject.AttachedObject.GetComponentsInChildren<ShapeVisibilityTracker>();
			if (socketedTrackers == null || socketedTrackers.Length == 0)
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- List is null or empty.");
				return;
			}
			if (socketedTrackers.Any(x => x is null))
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- Uses a null.");
				return;
			}
			foreach (var camera in QSBPlayerManager.GetPlayerCameras())
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"- {camera.name} : {socketedTrackers.Any(x => (bool)x.GetType().GetMethod("IsInFrustum", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(x, new object[] { camera.GetFrustumPlanes() }))}");
				offset += 30f;
			}
			*/

			offset = 10f;
			GUI.Label(new Rect(440, offset, 200f, 20f), $"Owned Objects :");
			offset += 30f;
			foreach (var obj in QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().Where(x => x.ControllingPlayer == QSBPlayerManager.LocalPlayerId))
			{
				GUI.Label(new Rect(440, offset, 200f, 20f), $"- {(obj as IWorldObject).Name}");
				offset += 30f;
			}
		}

		public override void Configure(IModConfig config)
		{
			DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
			Port = config.GetSettingsValue<int>("port");
			if (QSBNetworkManager.Instance != null)
			{
				QSBNetworkManager.Instance.networkPort = Port;
			}
			DebugMode = config.GetSettingsValue<bool>("debugMode");
			if (!DebugMode)
			{
				FindObjectsOfType<DebugZOverride>().ToList().ForEach(x => Destroy(x.gameObject));
			}
			ShowLinesInDebug = config.GetSettingsValue<bool>("showLinesInDebug");
		}
	}
}