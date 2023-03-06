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

	private static bool _inited;

	public static void Init()
	{
		if (_inited)
		{
			var count = _patchList.Count;
			foreach (var type in typeof(QSBPatch).GetDerivedTypes())
			{
				if (!_patchList.Any(x => x.GetType() == type))
				{
					_patchList.Add((QSBPatch)Activator.CreateInstance(type));
				}
			}

			DebugLog.DebugWrite($"Registered {_patchList.Count - count} addon patches.", MessageType.Success);
			return;
		}

		foreach (var type in typeof(QSBPatch).GetDerivedTypes())
		{
			_patchList.Add((QSBPatch)Activator.CreateInstance(type));
		}

		foreach (QSBPatchTypes type in Enum.GetValues(typeof(QSBPatchTypes)))
		{
			TypeToInstance.Add(type, new Harmony(type.ToString()));
		}

		_inited = true;
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
		DebugLog.DebugWrite($"Patch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
		foreach (var patch in _patchList.Where(x => x.Type == type && x.PatchVendor.HasFlag(QSBCore.GameVendor)))
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