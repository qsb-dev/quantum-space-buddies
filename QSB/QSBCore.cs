﻿using HarmonyLib;
using Mirror;
using OWML.Common;
using OWML.ModHelper;
using QSB.Menus;
using QSB.Patches;
using QSB.QuantumSync;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

/*
	Copyright (C) 2020 - 2022
			Henry Pointer (_nebula / misternebula),
			Will Corby (JohnCorby),
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
		public static string DefaultServerIP;
		public static AssetBundle NetworkAssetBundle { get; private set; }
		public static AssetBundle InstrumentAssetBundle { get; private set; }
		public static AssetBundle ConversationAssetBundle { get; private set; }
		public static AssetBundle DebugAssetBundle { get; private set; }
		public static AssetBundle TextAssetsBundle { get; private set; }
		public static bool IsHost => NetworkServer.active;
		public static bool IsInMultiplayer => QSBNetworkManager.singleton.isNetworkActive;
		public static string QSBVersion => Helper.Manifest.Version;
		public static string GameVersion =>
			// ignore the last patch numbers like the title screen does
			Application.version.Split('.').Take(3).Join(delimiter: ".");
		public static bool DLCInstalled => EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
		public static IMenuAPI MenuApi { get; private set; }
		public static DebugSettings DebugSettings { get; private set; } = new();

		public void Awake()
		{
			EpicRerouter.ModSide.Interop.Go();

			UIHelper.ReplaceUI(UITextType.PleaseUseController,
				"<color=orange>Quantum Space Buddies</color> is best experienced with friends...");
		}

		public void Start()
		{
			Helper = ModHelper;
			DebugLog.ToConsole($"* Start of QSB version {QSBVersion} - authored by {Helper.Manifest.Author}", MessageType.Info);

			DebugSettings = Helper.Storage.Load<DebugSettings>("debugsettings.json") ?? new DebugSettings();

			if (DebugSettings.HookDebugLogs)
			{
				Application.logMessageReceived += (condition, stackTrace, logType) =>
					DebugLog.DebugWrite($"[Debug] {condition}\nStacktrace: {stackTrace}", logType switch
					{
						LogType.Error => MessageType.Error,
						LogType.Assert => MessageType.Error,
						LogType.Warning => MessageType.Warning,
						LogType.Log => MessageType.Message,
						LogType.Exception => MessageType.Error,
						_ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
					});
			}

			RegisterAddons();

			InitAssemblies();

			MenuApi = ModHelper.Interaction.GetModApi<IMenuAPI>(ModHelper.Manifest.Dependencies[0]);

			NetworkAssetBundle = Helper.Assets.LoadBundle("AssetBundles/network");
			InstrumentAssetBundle = Helper.Assets.LoadBundle("AssetBundles/instruments");
			ConversationAssetBundle = Helper.Assets.LoadBundle("AssetBundles/conversation");
			DebugAssetBundle = Helper.Assets.LoadBundle("AssetBundles/debug");
			TextAssetsBundle = Helper.Assets.LoadBundle("AssetBundles/textassets");

			QSBPatchManager.Init();
			DeterministicManager.Init();

			var components = typeof(IAddComponentOnStart).GetDerivedTypes()
				.Select(x => gameObject.AddComponent(x))
				.ToArray();

			QSBWorldSync.Managers = components.OfType<WorldObjectManager>().ToArray();
			QSBPatchManager.OnPatchType += OnPatchType;
			QSBPatchManager.OnUnpatchType += OnUnpatchType;
		}

		private static void OnPatchType(QSBPatchTypes type)
		{
			if (type == QSBPatchTypes.OnClientConnect)
			{
				Application.runInBackground = true;
			}
		}

		private static void OnUnpatchType(QSBPatchTypes type)
		{
			if (type == QSBPatchTypes.OnClientConnect)
			{
				Application.runInBackground = false;
			}
		}

		public static readonly SortedDictionary<string, IModBehaviour> Addons = new();

		private static void RegisterAddons()
		{
			var addons = Helper.Interaction.GetDependants(Helper.Manifest.UniqueName);
			foreach (var addon in addons)
			{
				Addons.Add(addon.ModHelper.Manifest.UniqueName, addon);
			}
		}

		private static void InitAssemblies()
		{
			static void Init(Assembly assembly)
			{
				DebugLog.DebugWrite(assembly.ToString());
				assembly
					.GetTypes()
					.SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
					.Where(x => x.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute)))
					.ForEach(x => x.Invoke(null, null));
			}

			DebugLog.DebugWrite("Running RuntimeInitializeOnLoad methods for our assemblies", MessageType.Info);
			foreach (var path in Directory.EnumerateFiles(Helper.Manifest.ModFolderPath, "*.dll"))
			{
				if (Path.GetFileNameWithoutExtension(path) == "ProxyScripts")
				{
					continue;
				}
				var assembly = Assembly.LoadFile(path);
				Init(assembly);
			}

			foreach (var addon in Addons.Values)
			{
				var assembly = addon.GetType().Assembly;
				Init(assembly);
			}

			DebugLog.DebugWrite("Assemblies initialized", MessageType.Success);
		}

		public override void Configure(IModConfig config) => DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");

		private void Update()
		{
			if (Keyboard.current[Key.Q].isPressed && Keyboard.current[Key.D].wasPressedThisFrame)
			{
				DebugSettings.DebugMode = !DebugSettings.DebugMode;

				GetComponent<DebugActions>().enabled = DebugSettings.DebugMode;
				GetComponent<DebugGUI>().enabled = DebugSettings.DrawGui;
				QuantumManager.UpdateFromDebugSetting();
				DebugCameraSettings.UpdateFromDebugSetting();

				DebugLog.ToConsole($"DEBUG MODE = {DebugSettings.DebugMode}");
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