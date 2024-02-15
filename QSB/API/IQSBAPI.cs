using System;
using OWML.Common;
using UnityEngine;
using UnityEngine.Events;

public interface IQSBAPI
{
	#region General

	/// <summary>
	/// If called, all players connected to YOUR hosted game must have this mod installed.
	/// </summary>
	void RegisterRequiredForAllPlayers(IModBehaviour mod);

	/// <summary>
	/// Returns if the current player is the host.
	/// </summary>
	bool GetIsHost();

	/// <summary>
	/// Returns if the current player is in multiplayer.
	/// </summary>
	bool GetIsInMultiplayer();

	#endregion

	#region Player

	/// <summary>
	/// Returns the player ID of the current player.
	/// </summary>
	uint GetLocalPlayerID();

	/// <summary>
	/// Returns the name of a given player.
	/// </summary>
	/// <param name="playerID">The ID of the player you want the name of.</param>
	string GetPlayerName(uint playerID);

	/// <summary>
	/// Returns the body object of a given player. The pivot of this object is at the player's feet.
	/// </summary>
	/// <param name="playerID">The ID of the player you want the body of.</param>
	GameObject GetPlayerBody(uint playerID);

	/// <summary>
	/// Returns the camera object of a given player. The pivot of this object is at the player's point of view.
	/// </summary>
	/// <param name="playerID">The ID of the player you want the camera of.</param>
	GameObject GetPlayerCamera(uint playerID);

	/// <summary>
	/// Returns the hud marker component of a given player.
	/// </summary>
	/// <param name="playerID">The ID of the player you want the hud marker of.</param>
	HUDDistanceMarker GetPlayerHUDMarker(uint playerID);

	/// <summary>
	/// Returns the map marker component of a given player.
	/// </summary>
	/// <param name="playerID">The ID of the player you want the map marker of.</param>
	MonoBehaviour GetPlayerMapMarker(uint playerID);

	/// <summary>
	/// Returns true if a given player has fully loaded into the game. If the local player is still loading into the game, this will return false.
	/// </summary>
	/// <param name="playerID">The ID of the player.</param>
	bool GetPlayerReady(uint playerID);

	/// <summary>
	/// Returns true if the given player is dead.
	/// </summary>
	/// <param name="playerID">The ID of the player.</param>
	bool GetPlayerDead(uint playerID);

	/// <summary>
	/// Returns the list of IDs of all connected players.
	///
	/// The first player in the list is the host.
	/// </summary>
	uint[] GetPlayerIDs();

	/// <summary>
	/// Invoked when any player (local or remote) joins the game.
	/// </summary>
	UnityEvent<uint> OnPlayerJoin();

	/// <summary>
	/// Invoked when any player (local or remote) leaves the game.
	/// </summary>
	UnityEvent<uint> OnPlayerLeave();

	/// <summary>
	/// Sets some arbitrary data for a given player.
	/// </summary>
	/// <typeparam name="T">The type of the data. If not serializable, data will not be synced.</typeparam>
	/// <param name="playerId">The ID of the player.</param>
	/// <param name="key">The unique key to access this data by.</param>
	/// <param name="data">The data to set.</param>
	void SetCustomData<T>(uint playerId, string key, T data);

	/// <summary>
	/// Returns some arbitrary data from a given player.
	/// </summary>
	/// <typeparam name="T">The type of the data.</typeparam>
	/// <param name="playerId">The ID of the player.</param>
	/// <param name="key">The unique key of the data you want to access.</param>
	/// <returns>The data requested. If key is not valid, returns default.</returns>
	T GetCustomData<T>(uint playerId, string key);

	#endregion

	#region Messaging

	/// <summary>
	/// Sends a message containing arbitrary data to every player.
	///
	/// Keep your messages under around 1100 bytes.
	/// </summary>
	/// <typeparam name="T">The type of the data being sent. This type must be serializable.</typeparam>
	/// <param name="messageType">The unique key of the message.</param>
	/// <param name="data">The data to send.</param>
	/// <param name="to">The player to send this message to. (0 is the host, uint.MaxValue means every player)</param>
	/// <param name="receiveLocally">If true, the action given to <see cref="RegisterHandler{T}"/> will also be called on the same client that is sending the message.</param>
	void SendMessage<T>(string messageType, T data, uint to = uint.MaxValue, bool receiveLocally = false);

	/// <summary>
	/// Registers an action to be called when a message is received.
	/// </summary>
	/// <typeparam name="T">The type of the data in the message.</typeparam>
	/// <param name="messageType">The unique key of the message.</param>
	/// <param name="handler">The action to be ran when the message is received. The uint is the player ID that sent the messsage.</param>
	void RegisterHandler<T>(string messageType, Action<uint, T> handler);

	#endregion

	#region Chat

	/// <summary>
	/// Invoked when a chat message is received.
	/// The string is the message body.
	/// The uint is the player who sent the message. If it's a system message, this is uint.MaxValue.
	/// </summary>
	UnityEvent<string, uint> OnChatMessage();

	/// <summary>
	/// Sends a message in chat.
	/// </summary>
	/// <param name="message">The text of the message.</param>
	/// <param name="systemMessage">If false, the message is sent as if the local player wrote it manually. If true, the message has no player attached to it, like the player join messages.</param>
	/// <param name="color">The color of the message.</param>
	void SendChatMessage(string message, bool systemMessage, Color color);

	#endregion
}
