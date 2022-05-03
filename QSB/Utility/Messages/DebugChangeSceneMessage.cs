using QSB.Messaging;

namespace QSB.Utility.Messages;

public class DebugChangeSceneMessage : QSBMessage<bool>
{
	public DebugChangeSceneMessage(bool solarSystem) : base(solarSystem) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		if (Data)
		{
			PlayerData._currentGameSave.warpedToTheEye = false;
			PlayerData.SaveCurrentGame();
			LoadManager.LoadSceneAsync(OWScene.SolarSystem, true, LoadManager.FadeType.ToBlack);
		}
		else
		{
			PlayerData.SaveWarpedToTheEye(60);
			LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, true, LoadManager.FadeType.ToWhite);
		}
	}
}
