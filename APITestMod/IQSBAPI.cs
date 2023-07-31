using OWML.Common;
using UnityEngine.Events;

public interface IQSBAPI
{
	/// <summary>
	/// If called, all players connected to YOUR hosted game must have this mod installed.
	/// </summary>
	void RegisterRequiredForAllPlayers(IModBehaviour mod);

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
	/// Returns the list of IDs of all connected players.
	/// </summary>
	uint[] GetPlayerIDs();

	/// <summary>
	/// Invoked when a player joins the game.
	/// </summary>
	UnityEvent<uint> OnPlayerJoin();

	/// <summary>
	/// Invoked when a player leaves the game.
	/// </summary>
	UnityEvent<uint> OnPlayerLeave();

	/// <summary>
	/// Sets some arbitrary data for a given player.
	/// </summary>
	/// <typeparam name="T">The type of the data.</typeparam>
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

	/// <summary>
	/// Sends a message containing arbitrary data to every player.
	/// </summary>
	/// <typeparam name="T">The type of the data being sent. This type must be serializable.</typeparam>
	/// <param name="messageType">The unique key of the message.</param>
	/// <param name="data">The data to send.</param>
	/// <param name="receiveLocally">If true, the action given to <see cref="RegisterHandler{T}"/> will also be called on the same client that is sending the message.</param>
	void SendMessage<T>(string messageType, T data, bool receiveLocally = false);

	/// <summary>
	/// Registers an action to be called when a message is received.
	/// </summary>
	/// <typeparam name="T">The type of the data in the message.</typeparam>
	/// <param name="messageType">The unique key of the message.</param>
	/// <param name="handler">The action to be ran when the message is received. The uint is the player ID that sent the messsage.</param>
	void RegisterHandler<T>(string messageType, Action<uint, T> handler);
}
