using HarmonyLib;
using OWML.Common;
using QSB.Animation.NPC.Patches;
using QSB.Animation.Patches;
using QSB.CampfireSync.Patches;
using QSB.ConversationSync.Patches;
using QSB.DeathSync.Patches;
using QSB.EchoesOfTheEye.LightSensorSync.Patches;
using QSB.ElevatorSync.Patches;
using QSB.FrequencySync.Patches;
using QSB.GeyserSync.Patches;
using QSB.Inputs.Patches;
using QSB.ItemSync.Patches;
using QSB.LogSync.Patches;
using QSB.OrbSync.Patches;
using QSB.Player.Patches;
using QSB.PoolSync.Patches;
using QSB.QuantumSync.Patches;
using QSB.RoastingSync.Patches;
using QSB.SatelliteSync.Patches;
using QSB.ShipSync.Patches;
using QSB.StatueSync.Patches;
using QSB.TimeSync.Patches;
using QSB.Tools.ProbeLauncherTool.Patches;
using QSB.TranslationSync.Patches;
using QSB.Anglerfish.Patches;
using QSB.MeteorSync.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Patches
{
	public static class QSBPatchManager
	{
		public static event Action<QSBPatchTypes> OnPatchType;
		public static event Action<QSBPatchTypes> OnUnpatchType;

		private static List<QSBPatch> _patchList = new List<QSBPatch>();
		private static List<QSBPatchTypes> _patchedTypes = new List<QSBPatchTypes>();

		public static Dictionary<QSBPatchTypes, Harmony> TypeToInstance = new Dictionary<QSBPatchTypes, Harmony>();

		public static void Init()
		{
			_patchList = new List<QSBPatch>
			{
				new ConversationPatches(),
				new DeathPatches(),
				new ElevatorPatches(),
				new OrbPatches(),
				new LogPatches(),
				new QuantumVisibilityPatches(),
				new ServerQuantumPatches(),
				new ClientQuantumPatches(),
				new FrequencyPatches(),
				new SpiralPatches(),
				new QuantumPatches(),
				new ItemPatches(),
				new StatuePatches(),
				new GeyserPatches(),
				new PoolPatches(),
				new CampfirePatches(),
				new RoastingPatches(),
				new PlayerPatches(),
				new PlayerAnimationPatches(),
				new CharacterAnimationPatches(),
				new ShipPatches(),
				new InputPatches(),
				new TimePatches(),
				new MapPatches(),
				new LauncherPatches(),
				new SolanumPatches(),
				new SatelliteProjectorPatches(),
				new LightSensorPatches(),
				new AnglerPatches(),
				new MeteorClientPatches(),
				new MeteorServerPatches()
			};

			TypeToInstance = new Dictionary<QSBPatchTypes, Harmony>
			{
				{ QSBPatchTypes.OnClientConnect, new Harmony("QSB.Client") },
				{ QSBPatchTypes.OnServerClientConnect, new Harmony("QSB.Server") },
				{ QSBPatchTypes.OnNonServerClientConnect, new Harmony("QSB.NonServer") },
				{ QSBPatchTypes.RespawnTime, new Harmony("QSB.Death") }
			};

			DebugLog.DebugWrite("Patch Manager ready.", MessageType.Success);
		}

		public static void DoPatchType(QSBPatchTypes type)
		{
			if (_patchedTypes.Contains(type))
			{
				DebugLog.ToConsole($"Warning - Tried to patch type {type}, when it has already been patched!", MessageType.Warning);
				return;
			}

			OnPatchType?.SafeInvoke(type);
			//DebugLog.DebugWrite($"Patch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
			foreach (var patch in _patchList.Where(x => x.Type == type))
			{
				//DebugLog.DebugWrite($" - Patching in {patch.GetType().Name}", MessageType.Info);
				try
				{
					patch.DoPatches(TypeToInstance[type]);
					_patchedTypes.Add(type);
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Error while patching {patch.GetType().Name} :\r\n{ex}", MessageType.Error);
				}
			}
		}

		public static void DoUnpatchType(QSBPatchTypes type)
		{
			if (!_patchedTypes.Contains(type))
			{
				DebugLog.ToConsole($"Warning - Tried to unpatch type {type}, when it is either unpatched or was never patched.", MessageType.Warning);
				return;
			}

			OnUnpatchType?.SafeInvoke(type);
			//DebugLog.DebugWrite($"Unpatch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
			TypeToInstance[type].UnpatchSelf();
			_patchedTypes.Remove(type);
		}
	}
}
