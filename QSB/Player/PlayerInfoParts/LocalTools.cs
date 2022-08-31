using OWML.Common;
using QSB.Utility;

namespace QSB.Player;

public partial class PlayerInfo
{
	public PlayerProbeLauncher LocalProbeLauncher
	{
		get
		{
			if (!IsLocalPlayer)
			{
				DebugLog.ToConsole("Warning - Tried to access local-only property LocalProbeLauncher in PlayerInfo for non local player!", MessageType.Warning);
				return null;
			}

			return (PlayerProbeLauncher)Locator.GetToolModeSwapper().GetProbeLauncher();
		}
	}

	public Flashlight LocalFlashlight
	{
		get
		{
			if (!IsLocalPlayer)
			{
				DebugLog.ToConsole("Warning - Tried to access local-only property LocalFlashlight in PlayerInfo for non local player!", MessageType.Warning);
				return null;
			}

			return Locator.GetFlashlight();
		}
	}

	public Signalscope LocalSignalscope
	{
		get
		{
			if (!IsLocalPlayer)
			{
				DebugLog.ToConsole("Warning - Tried to access local-only property LocalSignalscope in PlayerInfo for non local player!", MessageType.Warning);
				return null;
			}

			return Locator.GetToolModeSwapper().GetSignalScope();
		}
	}

	public NomaiTranslator LocalTranslator
	{
		get
		{
			if (!IsLocalPlayer)
			{
				DebugLog.ToConsole("Warning - Tried to access local-only property LocalTranslator in PlayerInfo for non local player!", MessageType.Warning);
				return null;
			}

			return Locator.GetToolModeSwapper().GetTranslator();
		}
	}
}
