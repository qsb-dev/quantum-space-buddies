using Mirror;
using OWML.Common;
using QSB.Audio.Messages;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.GeyserSync.Messages;
using QSB.MeteorSync.Messages;
using QSB.OwnershipSync;
using QSB.Patches;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.QuantumSync.Messages;
using QSB.SaveSync.Messages;
using QSB.TimeSync.Messages;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace QSB.Messaging;

public static class QSBMessageManager
{
	#region inner workings

	internal static readonly Dictionary<int, Type> _types = new();

	private static string _rxPath;
	private static string _txPath;

	static QSBMessageManager()
	{
		foreach (var type in typeof(QSBMessage).GetDerivedTypes())
		{
			var hash = type.FullName.GetStableHashCode();
			_types.Add(hash, type);
			// call static constructor of message if needed
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);
		}
	}

	public static void Init()
	{
		NetworkServer.RegisterHandler<Wrapper>((_, wrapper) => OnServerReceive(wrapper));
		NetworkClient.RegisterHandler<Wrapper>(wrapper => OnClientReceive(wrapper));

		if (!QSBCore.DebugSettings.LogQSBMessages)
		{
			return;
		}

		var time = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
		_rxPath = Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, $"{time}_rx_log.txt");
		_txPath = Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, $"{time}_tx_log.txt");

		File.Create(_rxPath);
		File.Create(_txPath);
	}

	private static void OnServerReceive(QSBMessage msg)
	{
		if (msg == null)
		{
			return;
		}

		if (msg.To == uint.MaxValue)
		{
			NetworkServer.SendToAll<Wrapper>(msg);
		}
		else if (msg.To == 0)
		{
			NetworkServer.localConnection.Send<Wrapper>(msg);
		}
		else
		{
			var connection = msg.To.GetNetworkConnection();

			if (connection == default)
			{
				DebugLog.ToConsole($"Warning - Tried to handle message from disconnected(?) player.", MessageType.Warning);
				return;
			}

			connection.Send<Wrapper>(msg);
		}
	}

	private static void OnClientReceive(QSBMessage msg)
	{
		if (msg == null)
		{
			return;
		}

		if (PlayerTransformSync.LocalInstance == null)
		{
			DebugLog.ToConsole($"Warning - Tried to handle message {msg} before local player was established.", MessageType.Warning);
			return;
		}

		if (QSBPlayerManager.PlayerExists(msg.From))
		{
			var player = QSBPlayerManager.GetPlayer(msg.From);

			if (!player.IsReady
				&& player.PlayerId != QSBPlayerManager.LocalPlayerId
				&& player.State is ClientState.AliveInSolarSystem or ClientState.AliveInEye or ClientState.DeadInSolarSystem
				&& msg is not (PlayerInformationMessage or PlayerReadyMessage or RequestStateResyncMessage or ServerStateMessage))
			{
				//DebugLog.ToConsole($"Warning - Got message {msg} from player {msg.From}, but they were not ready. Asking for state resync, just in case.", MessageType.Warning);
				new RequestStateResyncMessage().Send();
			}
		}

		try
		{
			if (!msg.ShouldReceive)
			{
				return;
			}

			if (msg.From != QSBPlayerManager.LocalPlayerId)
			{
				SaveRXTX(msg, false);
				QSBPatch.Remote = true;
				msg.OnReceiveRemote();
				QSBPatch.Remote = false;
			}
			else
			{
				msg.OnReceiveLocal();
			}
		}
		catch (Exception ex)
		{
			DebugLog.ToConsole($"Error - Exception handling message {msg} : {ex}", MessageType.Error);
		}
	}

	#endregion

	public static void Send<M>(this M msg)
		where M : QSBMessage
	{
		if (PlayerTransformSync.LocalInstance == null)
		{
			DebugLog.ToConsole($"Warning - Tried to send message {msg} before local player was established.", MessageType.Warning);
			return;
		}

		msg.From = QSBPlayerManager.LocalPlayerId;
		SaveRXTX(msg, true);
		NetworkClient.Send<Wrapper>(msg);
	}

	public static void SendMessage<T, M>(this T worldObject, M msg)
		where T : IWorldObject
		where M : QSBWorldObjectMessage<T>
	{
		msg.ObjectId = worldObject.ObjectId;
		Send(msg);
	}

	public static void SaveRXTX(QSBMessage msg, bool transmit)
	{
		if (!QSBCore.DebugSettings.LogQSBMessages)
		{
			return;
		}

		if (msg 
			is ServerTimeMessage
			or SocketStateChangeMessage
			or OwnerQueueMessage 
			or GeyserMessage 
			or MeteorPreLaunchMessage 
			or MeteorLaunchMessage 
			or FragmentIntegrityMessage
			or LinkMessage
			or ShipLogFactSaveMessage
			or QuantumOwnershipMessage
			or PlayerMovementAudioFootstepMessage)
		{
			return;
		}

		var filepath = transmit ? _txPath : _rxPath;

		DebugLog.DebugWrite($"{(transmit ? "TX" : "RX")} {msg.GetType().Name} from:{msg.From} to:{msg.To}");

		var fileLines = File.ReadAllLines(filepath);

		var newlines = new List<string>();
		newlines.AddRange(fileLines);
		newlines.Add($"{msg.GetType().Name},{msg.From},{msg.To}");

		File.WriteAllLines(filepath, newlines);
	}
}

internal struct Wrapper : NetworkMessage
{
	public QSBMessage Msg;

	public static implicit operator QSBMessage(Wrapper wrapper) => wrapper.Msg;
	public static implicit operator Wrapper(QSBMessage msg) => new() { Msg = msg };
}

public static class ReaderWriterExtensions
{
	private static QSBMessage ReadQSBMessage(this NetworkReader reader)
	{
		var hash = reader.ReadInt();
		if (!QSBMessageManager._types.TryGetValue(hash, out var type))
		{
			DebugLog.DebugWrite($"unknown QSBMessage type with hash {hash}", MessageType.Error);
			return null;
		}
		var msg = (QSBMessage)FormatterServices.GetUninitializedObject(type);
		msg.Deserialize(reader);
		return msg;
	}

	private static void WriteQSBMessage(this NetworkWriter writer, QSBMessage msg)
	{
		var type = msg.GetType();
		var hash = type.FullName.GetStableHashCode();
		writer.WriteInt(hash);
		msg.Serialize(writer);
	}
}
