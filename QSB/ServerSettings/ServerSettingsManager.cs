using QSB.Utility;
using UnityEngine;

namespace QSB.ServerSettings;

internal class ServerSettingsManager : MonoBehaviour, IAddComponentOnStart
{
	public static bool ServerShowPlayerNames;
	public static bool ShowPlayerNames => (ServerShowPlayerNames || QSBCore.IsHost) && QSBCore.ShowPlayerNames;
}
