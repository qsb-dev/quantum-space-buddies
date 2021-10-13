using OWML.Common;
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
			_patchedMethods.Add(method);
		}

		public void Prefix(string patchName, params Type[] args)
			=> DoPrefixPostfix(true, patchName, args);

		public void Postfix(string patchName, params Type[] args)
			=> DoPrefixPostfix(false, patchName, args);

		private void DoPrefixPostfix(bool isPrefix, string patchName, params Type[] args)
		{
			var method = GetMethodInfo(patchName, args);

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

		private MethodInfo GetMethodInfo(string patchName, params Type[] args)
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

			var allMethodsOfName = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(x => x.Name == methodName);

			if (allMethodsOfName.Count() == 0)
			{
				DebugLog.ToConsole($"Error - Could not find method {methodName} in type {typeName}.", MessageType.Error);
				return null;
			}

			if (allMethodsOfName.Count() == 1)
			{
				return allMethodsOfName.First();
			}

			DebugLog.DebugWrite($"More than one method found with name {methodName} in type {typeName}");

			foreach (var method in allMethodsOfName)
			{
				DebugLog.DebugWrite($"checking {method.Name}");
				var paramList = method.GetParameters().Select(x => x.ParameterType);
				if (Enumerable.SequenceEqual(args, paramList))
				{
					DebugLog.DebugWrite($"match!");
					return method;
				}
			}

			DebugLog.DebugWrite($"nothing found");

			DebugLog.ToConsole($"Error - Could not find method {methodName} in type {typeName} with parameter list of {string.Join(", ", args.Select(x => x.FullName).ToArray())}", MessageType.Error);
			foreach (var method in allMethodsOfName)
			{
				var paramList = method.GetParameters().Select(x => x.ParameterType);
				DebugLog.ToConsole($"- Found {method.Name}, but with params {string.Join(", ", paramList.Select(x => x.FullName).ToArray())}", MessageType.Error);
			}

			return null;
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
			/*
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
			*/
		}
	}
}