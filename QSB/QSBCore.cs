using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Input;
using OWML.Utils;
using QSB.Animation.NPC;
using QSB.CampfireSync;
using QSB.ConversationSync;
using QSB.ElevatorSync;
using QSB.GeyserSync;
using QSB.Inputs;
using QSB.ItemSync;
using QSB.OrbSync;
using QSB.Patches;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.PoolSync;
using QSB.ProbeSync.TransformSync;
using QSB.QuantumSync;
using QSB.QuantumSync.WorldObjects;
using QSB.SectorSync;
using QSB.ShipSync;
using QSB.ShipSync.TransformSync;
using QSB.StatueSync;
using QSB.TimeSync;
using QSB.TranslationSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using System.Collections.Generic;
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
		public static IModUnityEvents UnityEvents => Helper.Events.Unity;
		public static string DefaultServerIP { get; private set; }
		public static int Port { get; private set; }
		public static bool DebugMode { get; private set; }
		public static bool ShowLinesInDebug { get; private set; }
		public static AssetBundle NetworkAssetBundle { get; private set; }
		public static AssetBundle InstrumentAssetBundle { get; private set; }
		public static AssetBundle ConversationAssetBundle { get; private set; }
		public static bool WorldObjectsReady => WorldObjectManager.AllReady && IsInMultiplayer && PlayerTransformSync.LocalInstance != null;
		public static bool IsServer => QNetworkServer.active;
		public static bool IsInMultiplayer => QNetworkManager.singleton.isNetworkActive;
		public static string QSBVersion => Helper.Manifest.Version;

		private const float _debugLineSpacing = 11f;

		public void Awake()
		{
			Application.runInBackground = true;

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

			gameObject.AddComponent<QSBNetworkManager>();
			gameObject.AddComponent<QNetworkManagerHUD>();
			gameObject.AddComponent<DebugActions>();
			gameObject.AddComponent<ConversationManager>();
			gameObject.AddComponent<QSBInputManager>();
			gameObject.AddComponent<TimeSyncUI>();
			gameObject.AddComponent<RepeatingManager>();
			gameObject.AddComponent<PlayerEntanglementWatcher>();
			gameObject.AddComponent<ShipManager>();

			// WorldObject managers
			gameObject.AddComponent<QuantumManager>();
			gameObject.AddComponent<SpiralManager>();
			gameObject.AddComponent<ElevatorManager>();
			gameObject.AddComponent<GeyserManager>();
			gameObject.AddComponent<OrbManager>();
			gameObject.AddComponent<QSBSectorManager>();
			gameObject.AddComponent<ItemManager>();
			gameObject.AddComponent<StatueManager>();
			gameObject.AddComponent<PoolManager>();
			gameObject.AddComponent<CampfireManager>();
			gameObject.AddComponent<CharacterAnimManager>();

			DebugBoxManager.Init();

			Helper.HarmonyHelper.EmptyMethod<ModCommandListener>("Update");

			// Stop players being able to pause
			Helper.HarmonyHelper.EmptyMethod(typeof(OWTime).GetMethod("Pause"));
		}

		public void Update() =>
			QNetworkIdentity.UNetStaticUpdate();

		public void OnGUI()
		{
			if (!DebugMode)
			{
				return;
			}

			var offset = 10f;
			GUI.Label(new Rect(220, 10, 200f, 20f), $"FPS : {Mathf.Round(1f / Time.smoothDeltaTime)}");
			offset += _debugLineSpacing;
			GUI.Label(new Rect(220, offset, 200f, 20f), $"HasWokenUp : {WorldObjectsReady}");
			offset += _debugLineSpacing;
			if (WakeUpSync.LocalInstance != null)
			{
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Time Difference : {WakeUpSync.LocalInstance.GetTimeDifference()}");
				offset += _debugLineSpacing;
				GUI.Label(new Rect(220, offset, 200f, 20f), $"Timescale : {OWTime.GetTimeScale()}");
				offset += _debugLineSpacing;
			}

			if (!WorldObjectsReady)
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

		public override void Configure(IModConfig config)
		{
			DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
			Port = config.GetSettingsValue<int>("port");
			if (QSBNetworkManager.Instance != null)
			{
				QSBNetworkManager.Instance.networkPort = Port;
			}

			DebugMode = config.GetSettingsValue<bool>("debugMode");
			ShowLinesInDebug = config.GetSettingsValue<bool>("showLinesInDebug");
		}
	}
}

/*
 * _nebula's music thanks
 * I listen to music constantly while programming/working - here's my thanks to them for keeping me entertained :P
 * 
 * Wintergatan
 * HOME
 * C418
 * Lupus Nocte
 * Max Cooper
 * Darren Korb
 * Harry Callaghan
 * Toby Fox
 * Andrew Prahlow
 * Valve (Mike Morasky, Kelly Bailey)
 * Joel Nielsen
 * Vulfpeck
 * Detektivbyrån
 * Ben Prunty
 * ConcernedApe
 * Jake Chudnow
 * Murray Gold
 * Teleskärm
 */