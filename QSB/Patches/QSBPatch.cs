using Harmony;
using OWML.Common;
using OWML.Utils;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QSB.Patches
{
	public abstract class QSBPatch
	{
		public abstract QSBPatchTypes Type { get; }
		public abstract void DoPatches();

		public void DoUnpatches()
		{
			var instance = QSBCore.Helper.HarmonyHelper.GetValue<HarmonyInstance>("_harmony");
			foreach (var item in _patchedMethods)
			{
				DebugLog.DebugWrite($"[Unpatch] {item.DeclaringType}.{item.Name}", MessageType.Info);
				instance.Unpatch(item, HarmonyPatchType.All);
			}
			_patchedMethods.Clear();
		}

		private List<MethodInfo> _patchedMethods = new List<MethodInfo>();

		public void Empty(string patchName)
		{
			DebugLog.DebugWrite($"[Empty] {patchName}", MessageType.Info);
			var method = GetMethodInfo(patchName);
			QSBCore.Helper.HarmonyHelper.EmptyMethod(method);
		}

		public void Prefix(string patchName) 
			=> DoPrefixPostfix(true, patchName);

		public void Postfix(string patchName) 
			=> DoPrefixPostfix(false, patchName);

		private void DoPrefixPostfix(bool isPrefix, string patchName)
		{
			var method = GetMethodInfo(patchName);

			if (method != null)
			{
				if (isPrefix)
				{
					QSBCore.Helper.HarmonyHelper.AddPrefix(method, GetType(), patchName);
				}
				else
				{
					QSBCore.Helper.HarmonyHelper.AddPostfix(method, GetType(), patchName);
				}
				_patchedMethods.Add(method);
			}

			DebugLog.DebugWrite($"{(isPrefix ? "[Prefix]" : "[Postfix]")} {patchName}", method == null ? MessageType.Error : MessageType.Success);
		}

		private MethodInfo GetMethodInfo(string patchName)
		{
			var splitName = patchName.Split('_');
			var typeName = splitName[0];
			var methodName = splitName[1];

			var type = GetFirstTypeByName(typeName);
			if (type == null)
			{
				DebugLog.DebugWrite($"Error - Couldn't find type for patch name {patchName}!", MessageType.Error);
				return null;
			}

			var method = type.GetAnyMethod(methodName);
			if (method == null)
			{
				DebugLog.DebugWrite($"Error - Couldn't find method for patch name {patchName}!", MessageType.Error);
				return null;
			}

			return method;
		}

		private Type GetFirstTypeByName(string typeName)
		{
			var a = typeof(OWRigidbody).Assembly;
			var assemblyTypes = a.GetTypes();
			for (int j = 0; j < assemblyTypes.Length; j++)
			{
				if (assemblyTypes[j].Name == typeName)
				{
					return assemblyTypes[j];
				}
			}
			return null;
		}
	}
}