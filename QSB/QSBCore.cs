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
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using QSB.API;
using QSB.BodyCustomization;
using QSB.Player.Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

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
	public static string SkinVariation { get; private set; } = "Default";
	public static string JetpackVariation { get; private set; } = "Orange";
	public static GameVendor GameVendor { get; private set; } = GameVendor.None;
	public static bool IsStandalone => GameVendor is GameVendor.Epic or GameVendor.Steam;
	public static IProfileManager ProfileManager => IsStandalone
		? QSBStandaloneProfileManager.SharedInstance
		: QSBMSStoreProfileManager.SharedInstance;
	public static IMenuAPI MenuApi { get; private set; }
	public static DebugSettings DebugSettings { get; private set; } = new();

	private static string randomSkinType;
	private static string randomJetpackType;


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

	public static event Action OnSkinsBundleLoaded;

	public override object GetApi() => new QSBAPI();

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

		DebugLog.ToConsole($"Determined game vendor as {GameVendor}", MessageType.Info);
	}

	private bool _steamworksInitialized;

	public void Awake()
	{
		// no, we cant localize this - languages are loaded after the splash screen
		UIHelper.ReplaceUI(UITextType.PleaseUseController,
			"<color=orange>Quantum Space Buddies</color> is best experienced with friends...");

		DetermineGameVendor();

		QSBPatchManager.Init();
		QSBPatchManager.DoPatchType(QSBPatchTypes.OnModStart);

		if (GameVendor != GameVendor.Steam)
		{
			DebugLog.DebugWrite($"Not steam, initializing Steamworks...");

			if (!Packsize.Test())
			{
				DebugLog.ToConsole("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", MessageType.Error);
			}

			if (!DllCheck.Test())
			{
				DebugLog.ToConsole("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", MessageType.Error);
			}

			// from facepunch.steamworks SteamClient.cs
			// Normally, Steam sets these env vars when launching the game through the Steam library.
			// These would also be set when running the .exe directly, thanks to Steam's "DRM" in the exe.
			// We're setting these manually to 480 - an AppID that every Steam account owns by default.
			// This tells Steam and Steamworks that the user is playing a game they own.
			// This lets anyone use Steamworks, even if they don't own Outer Wilds.
			// We also don't have to worry about Steam achievements or DLC in this case.
			Environment.SetEnvironmentVariable("SteamAppId", "480");
			Environment.SetEnvironmentVariable("SteamGameId", "480");

			if (!SteamAPI.Init())
			{
				DebugLog.ToConsole($"FATAL - SteamAPI.Init() failed. Do you have Steam open, and are you logged in?", MessageType.Fatal);
				return;
			}

			_steamworksInitialized = true;
		}
		else
		{
			SteamRerouter.ModSide.Interop.Init();

			DebugLog.DebugWrite($"Is steam - overriding AppID");
			OverrideAppId();
		}
	}

	public void OverrideAppId()
	{
		// Normally, Steam sets env vars when launching the game through the Steam library.
		// These would also be set when running the .exe directly, thanks to Steam's "DRM" in the exe.
		// However, for Steam players to be able to join non-Steam players, everyone has to be using Steamworks with the same AppID.
		// At this point, OW has already initialized Steamworks.
		// Since we handle achievements and DLC ownership in the rerouter, we need to re-initialize Steamworks with the new AppID.

		// (Also, Mobius forgor to change some default Steamworks code, so sometimes these env vars aren't set at all.
		// In this instance the overlay and achievements also don't work, but we can't fix that here.)

		// reset steamworks instance
		SteamManager.s_EverInitialized = false;
		var instance = SteamManager.s_instance;
		instance.m_bInitialized = false;
		SteamManager.s_instance = null;

		// Releases pointers and frees memory used by Steam to manage the current game.
		// Does not unhook the overlay, so we dont have to worry about that :peepoHappy:
		SteamAPI.Shutdown();

		// Set the env vars to an AppID that everyone owns by default.
		// from facepunch.steamworks SteamClient.cs
		Environment.SetEnvironmentVariable("SteamAppId", "480");
		Environment.SetEnvironmentVariable("SteamGameId", "480");

		// Re-initialize Steamworks.
		instance.InitializeOnAwake();

		// TODO also reregister hook and gamepad thing or else i think that wont work
	}

	public void OnDestroy()
	{
		if (_steamworksInitialized)
		{
			SteamAPI.Shutdown();
		}
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

		LoadBundleAsync("qsb_network_big");
		LoadBundleAsync("qsb_skins", request => BodyCustomizer.Instance.OnBundleLoaded(request.assetBundle));

		NetworkAssetBundle = LoadBundle("qsb_network");
		ConversationAssetBundle = LoadBundle("qsb_conversation");
		HUDAssetBundle = LoadBundle("qsb_hud");

		if (NetworkAssetBundle == null || ConversationAssetBundle == null || HUDAssetBundle == null)
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

		if (DebugSettings.RandomizeSkins)
		{
			var skinSetting = (JObject)ModHelper.Config.Settings["skinType"];
			var skinOptions = skinSetting["options"].ToObject<string[]>();
			randomSkinType = skinOptions[UnityEngine.Random.Range(0, skinOptions.Length - 1)];

			var jetpackSetting = (JObject)ModHelper.Config.Settings["jetpackType"];
			var jetpackOptions = jetpackSetting["options"].ToObject<string[]>();
			randomJetpackType = jetpackOptions[UnityEngine.Random.Range(0, jetpackOptions.Length - 1)];

			Configure(ModHelper.Config);
		}
	}

	private AssetBundle LoadBundle(string bundleName)
	{
		var timer = new Stopwatch();
		timer.Start();
		var ret = Helper.Assets.LoadBundle(Path.Combine("AssetBundles", bundleName));
		timer.Stop();
		DebugLog.ToConsole($"Bundle {bundleName} loaded in {timer.ElapsedMilliseconds} ms!", MessageType.Success);
		return ret;
	}

	private void LoadBundleAsync(string bundleName, Action<AssetBundleCreateRequest> runOnLoaded = null)
	{
		DebugLog.DebugWrite($"Loading {bundleName}...", MessageType.Info);
		var timer = new Stopwatch();
		timer.Start();
		var path = Path.Combine(ModHelper.Manifest.ModFolderPath, "AssetBundles", bundleName);
		var request = AssetBundle.LoadFromFileAsync(path);
		request.completed += _ =>
		{
			timer.Stop();
			DebugLog.ToConsole($"Bundle {bundleName} loaded in {timer.ElapsedMilliseconds} ms!", MessageType.Success);
			runOnLoaded?.Invoke(request);
		};
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
	public static readonly List<string> CosmeticAddons = new();

	private void RegisterAddons()
	{
		var addons = GetDependants();
		foreach (var addon in addons)
		{
			DebugLog.DebugWrite($"Registering addon {addon.ModHelper.Manifest.UniqueName}");
			Addons.Add(addon.ModHelper.Manifest.UniqueName, addon);
		}
	}

	/// <summary>
	/// Registers an addon that shouldn't be considered for hash checks when joining.
	/// This addon MUST NOT create any WorldObjects or NetworkBehaviours.
	/// </summary>
	/// <param name="addon">The behaviour of the addon.</param>
	public static void RegisterNotRequiredForAllPlayers(IModBehaviour addon)
	{
		var uniqueName = addon.ModHelper.Manifest.UniqueName;
		var addonAssembly = addon.GetType().Assembly;

		foreach (var type in addonAssembly.GetTypes())
		{
			if (typeof(WorldObjectManager).IsAssignableFrom(type) ||
				typeof(IWorldObject).IsAssignableFrom(type) ||
				typeof(NetworkBehaviour).IsAssignableFrom(type))
			{
				DebugLog.ToConsole($"Addon \"{uniqueName}\" cannot be cosmetic, as it creates networking objects.", MessageType.Error);
				return;
			}
		}

		DebugLog.DebugWrite($"Registering {uniqueName} as a cosmetic addon.");
		CosmeticAddons.Add(uniqueName);
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

		if (DebugSettings.RandomizeSkins)
		{
			SkinVariation = randomSkinType;
			JetpackVariation = randomJetpackType;
		}
		else
		{
			SkinVariation = config.GetSettingsValue<string>("skinType");
			JetpackVariation = config.GetSettingsValue<string>("jetpackType");
		}

		if (IsHost)
		{
			ServerSettingsManager.ServerShowPlayerNames = ShowPlayerNames;
			new ServerSettingsMessage().Send();
		}

		if (IsInMultiplayer)
		{
			new PlayerInformationMessage().Send();
		}
	}

	private void Update()
	{
		if (Keyboard.current[Key.Q].isPressed && Keyboard.current[Key.NumpadEnter].wasPressedThisFrame)
		{
			DebugSettings.DebugMode = !DebugSettings.DebugMode;

			GetComponent<DebugActions>().enabled = DebugSettings.DebugMode;
			GetComponent<DebugGUI>().enabled = DebugSettings.DebugMode;
			DebugCameraSettings.UpdateFromDebugSetting();

			DebugLog.ToConsole($"DEBUG MODE = {DebugSettings.DebugMode}");
		}

		if (_steamworksInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	private void CheckCompatibilityMods()
	{
		var mainMod = "";
		var compatMod = "";
		var missingCompat = false;

		/*if (Helper.Interaction.ModExists(NEW_HORIZONS) && !Helper.Interaction.ModExists(NEW_HORIZONS_COMPAT))
		{
			mainMod = NEW_HORIZONS;
			compatMod = NEW_HORIZONS_COMPAT;
			missingCompat = true;
		}*/

		if (missingCompat)
		{
			DebugLog.ToConsole($"FATAL - You have mod \"{mainMod}\" installed, which is not compatible with QSB without the compatibility mod \"{compatMod}\". " +
				$"Either disable the mod, or install/enable the compatibility mod.", MessageType.Fatal);
		}
	}
}
