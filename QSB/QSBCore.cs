using HarmonyLib;
using Mirror;
using OWML.Common;
using OWML.ModHelper;
using QSB.Localization;
using QSB.Menus;
using QSB.Messaging;
using QSB.Patches;
using QSB.QuantumSync;
using QSB.SaveSync;
using QSB.ServerSettings;
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
	Copyright (C) 2020 - 2023
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

	This work is unofficial Fan Content created under permission from the Mobius Digital Fan Content Policy.
	It includes materials which are the property of Mobius Digital and it is neither approved nor endorsed by Mobius Digital.
*/

namespace QSB;

public class QSBCore : ModBehaviour
{
	public static IModHelper Helper { get; private set; }
	public static string DefaultServerIP;
	public static AssetBundle NetworkAssetBundle { get; private set; }
	public static AssetBundle ConversationAssetBundle { get; private set; }
	public static AssetBundle DebugAssetBundle { get; private set; }
	public static AssetBundle HUDAssetBundle { get; private set; }
	public static bool IsHost => NetworkServer.active;
	public static bool IsInMultiplayer;
	public static string QSBVersion => Helper.Manifest.Version;
	public static string GameVersion =>
		// ignore the last patch numbers like the title screen does
		Application.version.Split('.').Take(3).Join(delimiter: ".");
	public static bool DLCInstalled => EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
	public static bool UseKcpTransport { get; private set; }
	public static bool IncompatibleModsAllowed { get; private set; }
	public static bool ShowPlayerNames { get; private set; }
	public static bool ShipDamage { get; private set; }
	public static bool ShowExtraHUDElements { get; private set; }
	public static bool TextChatInput { get; private set; }
	public static GameVendor GameVendor { get; private set; } = GameVendor.None;
	public static bool IsStandalone => GameVendor is GameVendor.Epic or GameVendor.Steam;
	public static IProfileManager ProfileManager => IsStandalone
		? QSBStandaloneProfileManager.SharedInstance
		: QSBMSStoreProfileManager.SharedInstance;
	public static IMenuAPI MenuApi { get; private set; }
	public static DebugSettings DebugSettings { get; private set; } = new();

	public const string NEW_HORIZONS = "xen.NewHorizons";
	public const string NEW_HORIZONS_COMPAT = "xen.NHQSBCompat";

	public static readonly string[] IncompatibleMods =
	{
		// incompatible mods
		"Raicuparta.NomaiVR",
		"xen.NewHorizons",
		"Vesper.AutoResume",
		"Vesper.OuterWildsMMO",
		"_nebula.StopTime",
		"PacificEngine.OW_Randomizer",
	};

	private static void DetermineGameVendor()
	{
		var gameAssemblyTypes = typeof(AstroObject).Assembly.GetTypes();
		var isEpic = gameAssemblyTypes.Any(x => x.Name == "EpicEntitlementRetriever");
		var isSteam = gameAssemblyTypes.Any(x => x.Name == "SteamEntitlementRetriever");
		var isUWP = gameAssemblyTypes.Any(x => x.Name == "MSStoreEntitlementRetriever");

		if (isEpic && !isSteam && !isUWP)
		{
			GameVendor = GameVendor.Epic;
		}
		else if (!isEpic && isSteam && !isUWP)
		{
			GameVendor = GameVendor.Steam;
		}
		else if (!isEpic && !isSteam && isUWP)
		{
			GameVendor = GameVendor.Gamepass;
		}
		else
		{
			// ???
			DebugLog.ToConsole($"FATAL - Could not determine game vendor.", MessageType.Fatal);
		}

		DebugLog.DebugWrite($"Determined game vendor as {GameVendor}", MessageType.Info);
	}

	public void Awake()
	{
		EpicRerouter.ModSide.Interop.Go();

		// no, we cant localize this - languages are loaded after the splash screen
		UIHelper.ReplaceUI(UITextType.PleaseUseController,
			"<color=orange>Quantum Space Buddies</color> is best experienced with friends...");

		DetermineGameVendor();

		QSBPatchManager.Init();
		QSBPatchManager.DoPatchType(QSBPatchTypes.OnModStart);
	}

	public void Start()
	{
		Helper = ModHelper;
		DebugLog.ToConsole($"* Start of QSB version {QSBVersion} - authored by {Helper.Manifest.Author}", MessageType.Info);

		CheckCompatibilityMods();

		DebugSettings = Helper.Storage.Load<DebugSettings>("debugsettings.json") ?? new DebugSettings();

		if (DebugSettings.HookDebugLogs)
		{
			Application.logMessageReceived += (condition, stackTrace, logType) =>
				DebugLog.ToConsole(
					$"[Debug] {condition}" +
					(stackTrace != string.Empty ? $"\nStacktrace: {stackTrace}" : string.Empty),
					logType switch
					{
						LogType.Error => MessageType.Error,
						LogType.Assert => MessageType.Error,
						LogType.Warning => MessageType.Warning,
						LogType.Log => MessageType.Message,
						LogType.Exception => MessageType.Error,
						_ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
					}
				);
		}

		if (DebugSettings.AutoStart)
		{
			UseKcpTransport = true;
			DebugSettings.DebugMode = true;
		}

		RegisterAddons();

		InitAssemblies();

		// init again to get addon patches
		QSBPatchManager.Init();

		MenuApi = ModHelper.Interaction.TryGetModApi<IMenuAPI>(ModHelper.Manifest.Dependencies[0]);

		DebugLog.DebugWrite("loading qsb_network_big bundle", MessageType.Info);
		var path = Path.Combine(ModHelper.Manifest.ModFolderPath, "AssetBundles/qsb_network_big");
		var request = AssetBundle.LoadFromFileAsync(path);
		request.completed += _ => DebugLog.DebugWrite("qsb_network_big bundle loaded", MessageType.Success);

		NetworkAssetBundle = Helper.Assets.LoadBundle("AssetBundles/qsb_network");
		ConversationAssetBundle = Helper.Assets.LoadBundle("AssetBundles/qsb_conversation");
		DebugAssetBundle = Helper.Assets.LoadBundle("AssetBundles/qsb_debug");
		HUDAssetBundle = Helper.Assets.LoadBundle("AssetBundles/qsb_hud");

		if (NetworkAssetBundle == null || ConversationAssetBundle == null || DebugAssetBundle == null)
		{
			DebugLog.ToConsole($"FATAL - An assetbundle is missing! Re-install mod or contact devs.", MessageType.Fatal);
			return;
		}

		DeterministicManager.Init();
		QSBLocalization.Init();

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

	private void RegisterAddons()
	{
		var addons = GetDependants();
		foreach (var addon in addons)
		{
			DebugLog.DebugWrite($"Registering addon {addon.ModHelper.Manifest.UniqueName}");
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

	public override void Configure(IModConfig config)
	{
		UseKcpTransport = config.GetSettingsValue<bool>("useKcpTransport") || DebugSettings.AutoStart;
		QSBNetworkManager.UpdateTransport();

		DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
		IncompatibleModsAllowed = config.GetSettingsValue<bool>("incompatibleModsAllowed");
		ShowPlayerNames = config.GetSettingsValue<bool>("showPlayerNames");
		ShipDamage = config.GetSettingsValue<bool>("shipDamage");
		ShowExtraHUDElements = config.GetSettingsValue<bool>("showExtraHud");
		TextChatInput = config.GetSettingsValue<bool>("textChatInput");

		if (IsHost)
		{
			ServerSettingsManager.ServerShowPlayerNames = ShowPlayerNames;
			new ServerSettingsMessage().Send();
		}
	}

	private void Update()
	{
		if (Keyboard.current[Key.Q].isPressed && Keyboard.current[Key.NumpadEnter].wasPressedThisFrame)
		{
			DebugSettings.DebugMode = !DebugSettings.DebugMode;

			GetComponent<DebugActions>().enabled = DebugSettings.DebugMode;
			GetComponent<DebugGUI>().enabled = DebugSettings.DebugMode;
			QuantumManager.UpdateFromDebugSetting();
			DebugCameraSettings.UpdateFromDebugSetting();

			DebugLog.ToConsole($"DEBUG MODE = {DebugSettings.DebugMode}");
		}
	}

	private void CheckCompatibilityMods()
	{
		var mainMod = "";
		var compatMod = "";
		var missingCompat = false;

		if (Helper.Interaction.ModExists(NEW_HORIZONS) && !Helper.Interaction.ModExists(NEW_HORIZONS_COMPAT))
		{
			mainMod = NEW_HORIZONS;
			compatMod = NEW_HORIZONS_COMPAT;
			missingCompat = true;
		}

		if (missingCompat)
		{
			DebugLog.ToConsole($"FATAL - You have mod \"{mainMod}\" installed, which is not compatible with QSB without the compatibility mod \"{compatMod}\". " +
				$"Either disable the mod, or install/enable the compatibility mod.", MessageType.Fatal);
		}
	}
}
