using OWML.Common;
using QuantumUNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QSB.Utility
{
	public static class Extensions
	{
		#region UNITY

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

			var playerController = connection.PlayerControllers.FirstOrDefault();
			if (playerController == null)
			{
				DebugLog.ToConsole($"Error - Player Controller of {connection.address} is null.", MessageType.Error);
				return uint.MaxValue;
			}

			return playerController.UnetView.NetId.Value;
		}

		public static void SpawnWithServerAuthority(this GameObject go)
		{
			if (!QSBCore.IsHost)
			{
				DebugLog.ToConsole($"Error - Tried to spawn {go.name} using SpawnWithServerAuthority when not the host!", MessageType.Error);
				return;
			}

			QNetworkServer.SpawnWithClientAuthority(go, QNetworkServer.localConnection);
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
					DebugLog.ToConsole($"Error invoking delegate! Message : {ex.InnerException!.Message} Stack Trace : {ex.InnerException.StackTrace}", MessageType.Error);
				}
			}
		}

		public static float Map(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo, bool clamp)
		{
			var mappedValue = ((value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom)) + outputFrom;

			return clamp
				? Mathf.Clamp(mappedValue, outputTo, outputFrom)
				: mappedValue;
		}

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
			const BindingFlags flags = BindingFlags.Instance
				| BindingFlags.Static
				| BindingFlags.Public
				| BindingFlags.NonPublic
				| BindingFlags.DeclaredOnly;
			if (typeof(T)
				.GetField(eventName, flags)?
				.GetValue(instance) is not MulticastDelegate multiDelegate)
			{
				return;
			}

			multiDelegate.GetInvocationList().ToList().ForEach(dl => dl.DynamicInvoke(args));
		}

		public static IEnumerable<Type> GetDerivedTypes(this Type type) => type.Assembly.GetTypes()
			.Where(x => !x.IsInterface && !x.IsAbstract && type.IsAssignableFrom(x));

		#endregion
	}
}
