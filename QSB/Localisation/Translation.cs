using Newtonsoft.Json;
using System.Collections.Generic;

namespace QSB.Localisation;

[JsonObject]
public class Translation
{
	[JsonProperty]
	public TextTranslation.Language Language;

	[JsonProperty]
	public string MainMenuHost;

	[JsonProperty]
	public string MainMenuConnect;

	[JsonProperty]
	public string PauseMenuDisconnect;

	[JsonProperty]
	public string PauseMenuStopHosting;

	[JsonProperty]
	public string PublicIPAddress;

	[JsonProperty]
	public string ProductUserID;

	[JsonProperty]
	public string Connect;

	[JsonProperty]
	public string Cancel;

	[JsonProperty]
	public string HostExistingOrNew;

	[JsonProperty]
	public string ExistingSave;

	[JsonProperty]
	public string NewSave;

	[JsonProperty]
	public string DisconnectAreYouSure;

	[JsonProperty]
	public string Yes;

	[JsonProperty]
	public string No;

	[JsonProperty]
	public string StopHostingAreYouSure;

	[JsonProperty]
	public string CopyProductUserIDToClipboard;

	[JsonProperty]
	public string Connecting;

	[JsonProperty]
	public string OK;

	[JsonProperty]
	public string ServerRefusedConnection;

	[JsonProperty]
	public string ClientDisconnectWithError;

	[JsonProperty]
	public string QSBVersionMismatch;

	[JsonProperty]
	public string OWVersionMismatch;

	[JsonProperty]
	public string DLCMismatch;

	[JsonProperty]
	public string GameProgressLimit;

	[JsonProperty]
	public string AddonMismatch;

	[JsonProperty]
	public string IncompatibleMod;

	[JsonProperty]
	public string PlayerJoinedTheGame;

	[JsonProperty]
	public string PlayerWasKicked;

	[JsonProperty]
	public string KickedFromServer;

	[JsonProperty]
	public string RespawnPlayer;

	[JsonProperty]
	public Dictionary<DeathType, string[]> DeathMessages;
}
