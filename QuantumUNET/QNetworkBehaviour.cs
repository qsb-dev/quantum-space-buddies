using QuantumUNET.Components;
using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QNetworkBehaviour : MonoBehaviour
	{
		public bool LocalPlayerAuthority => MyView.LocalPlayerAuthority;
		public bool IsServer => MyView.IsServer;
		public bool IsClient => MyView.IsClient;
		public bool IsLocalPlayer => MyView.IsLocalPlayer;
		public bool HasAuthority => MyView.HasAuthority;
		public NetworkInstanceId NetId => MyView.NetId;
		public QNetworkConnection ConnectionToServer => MyView.ConnectionToServer;
		public QNetworkConnection ConnectionToClient => MyView.ConnectionToClient;
		public short PlayerControllerId => MyView.PlayerControllerId;

		protected uint SyncVarDirtyBits { get; private set; }
		protected bool SyncVarHookGuard { get; set; }

		public QNetworkIdentity NetIdentity => MyView;

		private QNetworkIdentity MyView
		{
			get
			{
				QNetworkIdentity myView;
				if (gameObject == null)
				{
					QLog.FatalError($"Trying to get QNetworkIdentity of a null gameobject?");
					return null;
				}

				if (m_MyView == null)
				{
					m_MyView = GetComponent<QNetworkIdentity>();
					if (m_MyView == null)
					{
						QLog.FatalError($"There is no QNetworkIdentity on this object (name={name}). Please add one.");
					}

					myView = m_MyView;
				}
				else
				{
					myView = m_MyView;
				}

				return myView;
			}
		}

		protected void ClientSendUpdateVars()
		{
			var writer = new QNetworkWriter();
			writer.StartMessage(QMsgType.ClientUpdateVars);
			writer.Write(NetId);
			writer.Write(GetType().Name);
			if (OnSerialize(writer, false))
			{
				ClearAllDirtyBits();
				writer.FinishMessage();
				QClientScene.readyConnection.SendWriter(writer);
			}
		}

		protected void SendEventInternal(QNetworkWriter writer, string eventName)
		{
			if (!QNetworkServer.active)
			{
				QLog.Error($"Tried to send event {eventName} but QSBNetworkServer isn't active.");
				return;
			}

			writer.FinishMessage();
			QNetworkServer.SendWriterToReady(gameObject, writer);
		}

		public void SetDirtyBit(uint dirtyBit) => SyncVarDirtyBits |= dirtyBit;

		public void ClearAllDirtyBits()
		{
			m_LastSendTime = Time.time;
			SyncVarDirtyBits = 0U;
		}

		internal int GetDirtyChannel()
		{
			if (Time.time - m_LastSendTime > GetNetworkSendInterval())
			{
				if (SyncVarDirtyBits != 0U)
				{
					return 0;
				}
			}

			return -1;
		}

		public virtual bool OnSerialize(QNetworkWriter writer, bool initialState)
		{
			if (!initialState)
			{
				writer.WritePackedUInt32(0U);
			}

			return false;
		}

		public virtual void OnDeserialize(QNetworkReader reader, bool initialState)
		{
			if (!initialState)
			{
				reader.ReadPackedUInt32();
			}
		}

		public virtual void PreStartClient()
		{
		}

		public virtual void OnNetworkDestroy()
		{
		}

		public virtual void OnStartServer()
		{
		}

		public virtual void OnStartClient()
		{
		}

		public virtual void OnStartLocalPlayer()
		{
		}

		public virtual void OnStartAuthority()
		{
		}

		public virtual void OnStopAuthority()
		{
		}

		public virtual bool OnRebuildObservers(HashSet<QNetworkConnection> observers, bool initialize) => false;

		public virtual void OnSetLocalVisibility(bool vis)
		{
		}

		public virtual bool OnCheckObserver(QNetworkConnection conn) => true;

		public virtual float GetNetworkSendInterval() => 0.1f;

		private float m_LastSendTime;
		private QNetworkIdentity m_MyView;

		public delegate void CmdDelegate(QNetworkBehaviour obj, QNetworkReader reader);

		protected delegate void EventDelegate(List<Delegate> targets, QNetworkReader reader);
	}

	internal static class DotNetCompatibility
	{
		internal static string GetMethodName(this Delegate func) => func.Method.Name;

		internal static Type GetBaseType(this Type type) => type.BaseType;

		internal static string GetErrorCode(this SocketException e) => e.ErrorCode.ToString();
	}
}