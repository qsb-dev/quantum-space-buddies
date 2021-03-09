using OWML.Common;
using QSB.Player;
using QSB.TransformSync;
using QuantumUNET;
using System;
using System.Reflection;
using UnityEngine;

namespace QSB.Utility
{
	public static class Extensions
	{
		// GAMEOBJECT
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

		public static GameObject InstantiateInactive(this GameObject original)
		{
			original.SetActive(false);
			var copy = UnityEngine.Object.Instantiate(original);
			original.SetActive(true);
			return copy;
		}

		public static Transform InstantiateInactive(this Transform original) =>
			original.gameObject.InstantiateInactive().transform;

		// QNET
		public static uint GetPlayerId(this QNetworkConnection connection)
		{
			var go = connection.PlayerControllers[0].Gameobject;
			var controller = go.GetComponent<PlayerTransformSync>();
			return controller.NetId.Value;
		}

		// C#
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
	}
}
