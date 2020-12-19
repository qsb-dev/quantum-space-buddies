using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using QSB.ConversationSync;
using QSB.ElevatorSync;
using QSB.GeyserSync;
using QSB.OrbSync;
using QSB.Patches;
using QSB.SectorSync;
using QSB.TimeSync;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;
using UnityEngine;

/*
	Copyright (C) 2020 Henry Pointer (_nebula / misternebula), Aleksander Waage (AmazingAlek), Ricardo Lopes (Raicuparta)
	
	Consult LICENSE file for full license.
*/

namespace QSB
{
	public class QSBCore : ModBehaviour
	{
		public static IModBehaviour ModBehaviour { get; private set; }
		public static IModHelper Helper { get; private set; }
		public static string DefaultServerIP { get; private set; }
		public static int Port { get; private set; }
		public static bool DebugMode { get; private set; }
		public static AssetBundle NetworkAssetBundle { get; private set; }
		public static AssetBundle InstrumentAssetBundle { get; private set; }
		public static bool HasWokenUp { get; set; }
		public static bool IsServer => QSBNetworkServer.active;

		public void Awake()
		{
			Application.runInBackground = true;

			var instance = TextTranslation.Get().GetValue<TextTranslation.TranslationTable>("m_table");
			instance.theUITable[(int)UITextType.PleaseUseController] =
				"<color=orange>Quantum Space Buddies</color> is best experienced with friends...";

			ModBehaviour = this;
		}

		public void Start()
		{
			Helper = ModHelper;
			DebugLog.ToConsole($"* Start of QSB version {Helper.Manifest.Version} - authored by {Helper.Manifest.Author}", MessageType.Info);

			NetworkAssetBundle = Helper.Assets.LoadBundle("assets/network");
			InstrumentAssetBundle = Helper.Assets.LoadBundle("assets/instruments");

			QSBPatchManager.Init();

			QSBPatchManager.DoPatchType(QSBPatchTypes.OnModStart);

			gameObject.AddComponent<QSBNetworkManager>();
			gameObject.AddComponent<QSBNetworkManagerHUD>();
			gameObject.AddComponent<DebugActions>();
			gameObject.AddComponent<ElevatorManager>();
			gameObject.AddComponent<GeyserManager>();
			gameObject.AddComponent<OrbManager>();
			gameObject.AddComponent<QSBSectorManager>();
			gameObject.AddComponent<ConversationManager>();
			gameObject.AddComponent<QSBInputManager>();
			gameObject.AddComponent<TimeSyncUI>();

			// Stop players being able to pause
			Helper.HarmonyHelper.EmptyMethod(typeof(OWTime).GetMethod("Pause"));
		}

		public void Update() =>
			QSBNetworkIdentity.UNetStaticUpdate();

		public override void Configure(IModConfig config)
		{
			DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
			Port = config.GetSettingsValue<int>("port");
			if (QSBNetworkManager.Instance != null)
			{
				QSBNetworkManager.Instance.networkPort = Port;
			}
			DebugMode = config.GetSettingsValue<bool>("debugMode");
		}
	}
}