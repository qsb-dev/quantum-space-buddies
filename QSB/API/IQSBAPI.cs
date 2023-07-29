using System;

namespace QSB.API;

public interface IQSBAPI
{
	uint GetLocalPlayerID();

	void SetCustomData<T>(uint playerId, string key, T data);
	T GetCustomData<T>(uint playerId, string key);

	void SendMessage<T>(string messageType, T data);
	void RegisterHandler<T>(string messageType, Action<T> action);
}
