﻿using System.Linq;
using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Input;
using QSB.ConversationSync;
using QSB.Inputs;
using QSB.Menus;
using QSB.Patches;
using QSB.Player;
using QSB.RespawnSync;
using QSB.SatelliteSync;
using QSB.StatueSync;
using QSB.TimeSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
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
		public static bool DebugMode => DebugSettings.DebugMode;
		public static bool ShowLinesInDebug => DebugMode && DebugSettings.DrawLines;
		public static bool ShowQuantumVisibilityObjects => DebugMode && DebugSettings.ShowQuantumVisibilityObjects;
		public static bool ShowQuantumDebugBoxes => DebugMode && DebugSettings.ShowQuantumDebugBoxes;
		public static bool AvoidTimeSync => DebugMode && DebugSettings.AvoidTimeSync;
		public static bool SkipTitleScreen => DebugMode && DebugSettings.SkipTitleScreen;
		public static AssetBundle NetworkAssetBundle { get; private set; }
		public static AssetBundle InstrumentAssetBundle { get; private set; }
		public static AssetBundle ConversationAssetBundle { get; private set; }
		public static AssetBundle DebugAssetBundle { get; private set; }
		public static bool IsHost => QNetworkServer.active;
		public static bool IsInMultiplayer => QNetworkManager.singleton.isNetworkActive;
		public static string QSBVersion => Helper.Manifest.Version;
		public static string GameVersion => Application.version;
		public static GamePlatform Platform => typeof(Achievements).Assembly.GetTypes().Any(x => x.Name == "EpicEntitlementRetriever")
			? GamePlatform.Epic
			: GamePlatform.Steam;
		public static IMenuAPI MenuApi { get; private set; }

		private static DebugSettings DebugSettings { get; set; } = new DebugSettings();

		public void Awake()
		{
			var instance = TextTranslation.Get().m_table;
			instance.theUITable[(int)UITextType.PleaseUseController] =
				"<color=orange>Quantum Space Buddies</color> is best experienced with friends...";
		}

		public void Start()
		{
			Helper = ModHelper;
			DebugLog.ToConsole($"* Start of QSB version {QSBVersion} - authored by {Helper.Manifest.Author}", MessageType.Info);

			MenuApi = ModHelper.Interaction.GetModApi<IMenuAPI>("_nebula.MenuFramework");

			NetworkAssetBundle = Helper.Assets.LoadBundle("AssetBundles/network");
			InstrumentAssetBundle = Helper.Assets.LoadBundle("AssetBundles/instruments");
			ConversationAssetBundle = Helper.Assets.LoadBundle("AssetBundles/conversation");
			DebugAssetBundle = Helper.Assets.LoadBundle("AssetBundles/debug");

			DebugSettings = ModHelper.Storage.Load<DebugSettings>("debugsettings.json");

			if (DebugSettings == null)
			{
				DebugSettings = new DebugSettings();
			}

			QSBPatchManager.Init();

			gameObject.AddComponent<QSBNetworkManager>();
			gameObject.AddComponent<DebugActions>();
			gameObject.AddComponent<ConversationManager>();
			gameObject.AddComponent<QSBInputManager>();
			gameObject.AddComponent<TimeSyncUI>();
			gameObject.AddComponent<PlayerEntanglementWatcher>();
			gameObject.AddComponent<DebugGUI>();
			gameObject.AddComponent<MenuManager>();
			gameObject.AddComponent<RespawnManager>();
			gameObject.AddComponent<SatelliteProjectorManager>();
			gameObject.AddComponent<StatueManager>();

			// WorldObject managers
			foreach (var type in typeof(WorldObjectManager).GetDerivedTypes())
			{
				gameObject.AddComponent(type);
			}

			DebugBoxManager.Init();

			Helper.HarmonyHelper.EmptyMethod<ModCommandListener>("Update");

			QSBPatchManager.OnPatchType += OnPatchType;
			QSBPatchManager.OnUnpatchType += OnUnpatchType;
		}

		private void OnPatchType(QSBPatchTypes type)
		{
			if (type == QSBPatchTypes.OnClientConnect)
			{
				Application.runInBackground = true;
				if (Locator.GetSceneMenuManager() != null && Locator.GetSceneMenuManager().pauseMenu.IsOpen())
				{
					Locator.GetSceneMenuManager().pauseMenu._pauseMenu.EnableMenu(false);
				}
			}
		}

		private void OnUnpatchType(QSBPatchTypes type)
		{
			if (type == QSBPatchTypes.OnClientConnect)
			{
				Application.runInBackground = false;
			}
		}

		public void Update() =>
			QNetworkIdentity.UNetStaticUpdate();

		public override void Configure(IModConfig config)
		{
			DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
			Port = config.GetSettingsValue<int>("port");
			if (QSBNetworkManager.Instance != null)
			{
				QSBNetworkManager.Instance.networkPort = Port;
			}
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
 * Daft Punk
 * Natalie Holt
 * WMD
 */
