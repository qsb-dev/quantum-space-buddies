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
			foreach (var item in _patchedMethods)
			{
				//DebugLog.DebugWrite($"[Unpatch] {item.DeclaringType}.{item.Name}", MessageType.Info);
				Unpatch(item);
			}

			_patchedMethods.Clear();
		}

		private List<MethodInfo> _patchedMethods = new List<MethodInfo>();

		public void Empty(string patchName)
		{
			//DebugLog.DebugWrite($"[Empty] {patchName}", MessageType.Success);
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

			//DebugLog.DebugWrite($"{(isPrefix ? "[Prefix]" : "[Postfix]")} {patchName}", method == null ? MessageType.Error : MessageType.Success);
		}

		private MethodInfo GetMethodInfo(string patchName)
		{
			var splitName = patchName.Split('_');
			var typeName = splitName[0];
			var methodName = splitName[1];

			var type = GetFirstTypeByName(typeName);
			if (type == null)
			{
				DebugLog.ToConsole($"Error - Couldn't find type for patch name {patchName}!", MessageType.Error);
				return null;
			}

			var method = type.GetAnyMethod(methodName);
			if (method == null)
			{
				DebugLog.ToConsole($"Error - Couldn't find method for patch name {patchName}!", MessageType.Error);
				return null;
			}

			return method;
		}

		private Type GetFirstTypeByName(string typeName)
		{
			var a = typeof(OWRigidbody).Assembly;
			var assemblyTypes = a.GetTypes();
			for (var j = 0; j < assemblyTypes.Length; j++)
			{
				if (assemblyTypes[j].Name == typeName)
				{
					return assemblyTypes[j];
				}
			}

			return null;
		}

		private void Unpatch(MethodInfo method)
		{
			var dictionary = typeof(HarmonySharedState).Invoke<Dictionary<MethodBase, byte[]>>("GetState", new object[0]);
			var methodBase = dictionary.Keys.First(m =>
				m.DeclaringType == method.DeclaringType
				&& m.Name == method.Name);

			var patchInfo = PatchInfoSerialization.Deserialize(dictionary.GetValueSafe(methodBase));
			patchInfo.RemovePostfix(QSBCore.Helper.Manifest.UniqueName);
			patchInfo.RemovePrefix(QSBCore.Helper.Manifest.UniqueName);
			patchInfo.RemoveTranspiler(QSBCore.Helper.Manifest.UniqueName);

			PatchFunctions.UpdateWrapper(methodBase, patchInfo, QSBCore.Helper.Manifest.UniqueName);
			dictionary[methodBase] = patchInfo.Serialize();
		}
	}
}