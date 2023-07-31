using System;

namespace APITestMod;

// TODO: document
public interface IQSBAPI
{
	uint GetLocalPlayerID();

	void SetCustomData<T>(uint playerId, string key, T data);
	T GetCustomData<T>(uint playerId, string key);

	void SendMessage<T>(string messageType, T data, bool receiveLocally = false);
	void RegisterHandler<T>(string messageType, Action<uint, T> handler);
}