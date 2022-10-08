using Cysharp.Threading.Tasks;
using Mirror;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QSB.Utility;

public static class Extensions
{
	#region UNITY

	public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
		=> transform.rotation * localRotation;

	public static GameObject InstantiateInactive(this GameObject original)
	{
		if (!original.activeSelf)
		{
			return Object.Instantiate(original);
		}

		original.SetActive(false);
		var copy = Object.Instantiate(original);
		original.SetActive(true);
		return copy;
	}

	#endregion

	#region MIRROR

	public static uint GetPlayerId(this NetworkConnectionToClient conn)
	{
		if (!conn.identity)
		{
			// wtf
			DebugLog.ToConsole($"Error - GetPlayerId on {conn} has no identity\n{Environment.StackTrace}", MessageType.Error);
			return uint.MaxValue;
		}

		return conn.identity.netId;
	}

	public static NetworkConnectionToClient GetNetworkConnection(this uint playerId)
	{
		var conn = NetworkServer.connections.Values.FirstOrDefault(x => playerId == x.GetPlayerId());
		if (conn == default)
		{
			DebugLog.ToConsole($"Error - GetNetworkConnection on {playerId} found no connection\n{Environment.StackTrace}", MessageType.Error);
		}

		return conn;
	}

	public static void SpawnWithServerAuthority(this GameObject go) =>
		NetworkServer.Spawn(go, NetworkServer.localConnection);

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
				DebugLog.ToConsole($"Error invoking delegate! {ex.InnerException}", MessageType.Error);
			}
		}
	}

	public static float Map(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo, bool clamp)
	{
		var mappedValue = (value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom) + outputFrom;

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

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		var comparer = Comparer<TKey>.Default;
		var yk = default(TKey);
		var y = default(TSource);
		var hasValue = false;
		foreach (var x in source)
		{
			var xk = keySelector(x);
			if (!hasValue)
			{
				hasValue = true;
				yk = xk;
				y = x;
			}
			else if (comparer.Compare(xk, yk) < 0)
			{
				yk = xk;
				y = x;
			}
		}

		if (!hasValue)
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}

		return y;
	}

	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		var comparer = Comparer<TKey>.Default;
		var yk = default(TKey);
		var y = default(TSource);
		var hasValue = false;
		foreach (var x in source)
		{
			var xk = keySelector(x);
			if (!hasValue)
			{
				hasValue = true;
				yk = xk;
				y = x;
			}
			else if (comparer.Compare(xk, yk) > 0)
			{
				yk = xk;
				y = x;
			}
		}

		if (!hasValue)
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}

		return y;
	}

	public static bool IsInRange<T>(this IList<T> list, int index) => index >= 0 && index < list.Count;

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

		multiDelegate.SafeInvoke(args);
	}

	public static IEnumerable<Type> GetDerivedTypes(this Type type) =>
		QSBCore.Addons.Values
			.Select(x => x.GetType().Assembly)
			.Append(type.Assembly)
			.SelectMany(x => x.GetTypes())
			.Where(x => !x.IsInterface && !x.IsAbstract && type.IsAssignableFrom(x))
			.OrderBy(x => x.FullName);

	public static Guid ToGuid(this int value)
	{
		var bytes = new byte[16];
		BitConverter.GetBytes(value).CopyTo(bytes, 0);
		return new Guid(bytes);
	}

	public static void Try(this object self, string doingWhat, Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			DebugLog.ToConsole($"{self} - error {doingWhat} : {e}", MessageType.Error);
		}
	}

	public static async UniTask Try(this object self, string doingWhat, Func<UniTask> func)
	{
		try
		{
			await func().SuppressCancellationThrow();
		}
		catch (Exception e)
		{
			DebugLog.ToConsole($"{self} - error {doingWhat} : {e}", MessageType.Error);
		}
	}

	// Adapted from https://stackoverflow.com/a/30758270
	public static int GetSequenceHash(this IEnumerable<string> list)
	{
		const int seed = 487;
		const int modifier = 31;

		unchecked
		{
			return list.Aggregate(seed, (current, item) =>
				(current * modifier) + item.GetStableHashCode());
		}
	}

	#endregion
}