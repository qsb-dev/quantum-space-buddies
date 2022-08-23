using System.Collections.Generic;

namespace QSB.Localization;

public class Translation
{
	public TextTranslation.Language Language;
	public string MainMenuHost;
	public string MainMenuConnect;
	public string PauseMenuDisconnect;
	public string PauseMenuStopHosting;
	public string PublicIPAddress;
	public string LobbyID;
	public string Connect;
	public string Cancel;
	public string HostExistingOrNewOrCopy;
	public string HostNewOrCopy;
	public string HostExistingOrNew;
	public string ExistingSave;
	public string NewSave;
	public string CopySave;
	public string DisconnectAreYouSure;
	public string Yes;
	public string No;
	public string StopHostingAreYouSure;
	public string CopyLobbyID;
	public string Connecting;
	public string OK;
	public string ServerRefusedConnection;
	public string ClientDisconnectWithError;
	public string QSBVersionMismatch;
	public string OWVersionMismatch;
	public string DLCMismatch;
	public string GameProgressLimit;
	public string AddonMismatch;
	public string IncompatibleMod;
	public string PlayerJoinedTheGame;
	public string PlayerWasKicked;
	public string KickedFromServer;
	public string RespawnPlayer;
	public string TimeSyncTooFarBehind;
	public string TimeSyncWaitingForStartOfServer;
	public string TimeSyncTooFarAhead;
	public string TimeSyncWaitForAllToReady;
	public string TimeSyncWaitForAllToDie;
	public string GalaxyMapEveryoneNotPresent;
	public string YouAreDead;
	public string WaitingForRespawn;
	public string WaitingForAllToDie;
	public string AttachToShip;
	public string DetachFromShip;
	public Dictionary<DeathType, string[]> DeathMessages;
}
