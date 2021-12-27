using HarmonyLib;
using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QuantumUNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QSB.Utility
{
	public static class Extensions
	{
		#region UNITY

		public static void Show(this GameObject gameObject) => SetVisibility(gameObject, true);

		public static void Hide(this GameObject gameObject) => SetVisibility(gameObject, false);

		private static void SetVisibility(GameObject gameObject, bool isVisible)
		{
			var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var renderer in renderers)
			{
				renderer.enabled = isVisible;
			}
		}

		public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
			=> transform.rotation * localRotation;

		public static GameObject InstantiateInactive(this GameObject original)
		{
			original.SetActive(false);
			var copy = Object.Instantiate(original);
			original.SetActive(true);
			return copy;
		}

		public static Transform InstantiateInactive(this Transform original) =>
			original.gameObject.InstantiateInactive().transform;

		#endregion

		#region QNET

		public static uint GetPlayerId(this QNetworkConnection connection)
		{
			if (connection == null)
			{
				DebugLog.ToConsole($"Error - Trying to get player id of null QNetworkConnection.\r\n{Environment.StackTrace}", MessageType.Error);
				return uint.MaxValue;
			}

			var playerController = connection.PlayerControllers[0];
			if (playerController == null)
			{
				DebugLog.ToConsole($"Error - Player Controller of {connection.address} is null.", MessageType.Error);
				return uint.MaxValue;
			}

			var go = playerController.Gameobject;
			if (go == null)
			{
				DebugLog.ToConsole($"Error - GameObject of {playerController.UnetView.NetId.Value} is null.", MessageType.Error);
				return uint.MaxValue;
			}

			var controller = go.GetComponent<PlayerTransformSync>();
			if (controller == null)
			{
				DebugLog.ToConsole($"Error - No PlayerTransformSync found on {go.name}", MessageType.Error);
				return uint.MaxValue;
			}

			return controller.NetId.Value;
		}

		public static void SpawnWithServerAuthority(this GameObject go)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole($"Error - Tried to spawn {go.name} using SpawnWithServerAuthority when not the host!", MessageType.Error);
				return;
			}

			QNetworkServer.SpawnWithClientAuthority(go, QSBPlayerManager.LocalPlayer.TransformSync.gameObject);
		}

		#endregion

		#region C#

		public static void SafeInvoke(this MulticastDelegate multicast, params object[] args)
		{
			foreach (var del in multicast.GetInvocationList())
			{
				try
				{
					del.DynamicInvoke(args);
				}
				catch (TargetInvocationException ex)
				{
					DebugLog.ToConsole($"Error invoking delegate! Message : {ex.InnerException.Message} Stack Trace : {ex.InnerException.StackTrace}", MessageType.Error);
				}
			}
		}

		public static float Map(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo)
			=> ((value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom)) + outputFrom;

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var item in enumerable)
			{
				action(item);
			}
		}

		public static int IndexOf<T>(this T[] array, T value) => Array.IndexOf(array, value);

		public static bool IsInRange<T>(this IList<T> list, int index) => index >= 0 && index < list.Count;

		public static bool TryGet<T>(this IList<T> list, int index, out T element)
		{
			if (!list.IsInRange(index))
			{
				element = default;
				return false;
			}

			element = list[index];
			return true;
		}

		public static void RaiseEvent<T>(this T instance, string eventName, params object[] args)
		{
			if (typeof(T)
				.GetField(eventName, BindingFlags.Instance
					| BindingFlags.Static
					| BindingFlags.Public
					| BindingFlags.NonPublic
					| BindingFlags.DeclaredOnly)?
				.GetValue(instance) is not MulticastDelegate multiDelegate)
			{
				return;
			}

			multiDelegate.GetInvocationList().ToList().ForEach(dl => dl.DynamicInvoke(args));
		}

		/// <summary>
		/// get base.name in obj and invoke it with args
		/// </summary>
		public static void InvokeBase(this object obj, string name, params object[] args)
			=> InvokeBase<object>(obj, name, args);

		/// <summary>
		/// get base.name in obj and invoke it with args
		/// </summary>
		public static TResult InvokeBase<TResult>(this object obj, string name, params object[] args)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
			var argTypes = args.Select(x => x?.GetType() ?? typeof(object)).ToArray();

			var baseType = obj.GetType();
			var baseMethod = default(MethodInfo);
			while (baseMethod == null)
			{
				baseType = baseType.BaseType;
				if (baseType == null)
				{
					throw new MissingMethodException("can't find method "
						+ $"{obj.GetType()}.base.{name}"
						+ $"({argTypes.Join()})");
				}
				baseMethod = baseType.GetMethod(name, flags, null, argTypes, null);
			}

			Array.Resize(ref argTypes, argTypes.Length + 1);
			argTypes[argTypes.Length - 1] = baseMethod.ReturnType;
			var dlType = Expression.GetDelegateType(argTypes);

			var ptr = baseMethod.MethodHandle.GetFunctionPointer();
			var dl = (Delegate)Activator.CreateInstance(dlType, obj, ptr);
			return (TResult)dl.DynamicInvoke(args);
		}

		public static IEnumerable<Type> GetDerivedTypes(this Type type) => type.Assembly.GetTypes()
			.Where(x => !x.IsInterface && !x.IsAbstract && type.IsAssignableFrom(x));

		#endregion

		#region OW

		public static Vector3 GetRelativeAngularVelocity(this OWRigidbody baseBody, OWRigidbody relativeBody)
			=> baseBody.GetAngularVelocity() - relativeBody.GetAngularVelocity();

		#endregion
	}
}
