using HarmonyLib;
using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Patches;

public static class QSBPatchManager
{
	public static event Action<QSBPatchTypes> OnPatchType;
	public static event Action<QSBPatchTypes> OnUnpatchType;

	private static readonly List<QSBPatch> _patchList = new();
	private static readonly List<QSBPatchTypes> _patchedTypes = new();

	public static Dictionary<QSBPatchTypes, Harmony> TypeToInstance = new();

	public static void Init()
	{
		foreach (var type in typeof(QSBPatch).GetDerivedTypes())
		{
			_patchList.Add((QSBPatch)Activator.CreateInstance(type));
		}

		TypeToInstance = new Dictionary<QSBPatchTypes, Harmony>
		{
			{
				QSBPatchTypes.OnClientConnect, new Harmony("QSB.Client")
			},
			{
				QSBPatchTypes.OnServerClientConnect, new Harmony("QSB.Server")
			},
			{
				QSBPatchTypes.OnNonServerClientConnect, new Harmony("QSB.NonServer")
			},
			{
				QSBPatchTypes.RespawnTime, new Harmony("QSB.Death")
			}
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
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error while patching {patch.GetType().Name} :\r\n{ex}", MessageType.Error);
			}
		}

		_patchedTypes.Add(type);
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