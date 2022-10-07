using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.SlideProjectors.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;

public class QSBSlideProjector : WorldObject<SlideProjector>
{
	private uint _user;

	public override async UniTask Init(CancellationToken ct) =>
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

	public override void OnRemoval() =>
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}
		if (_user == player.PlayerId)
		{
			this.SendMessage(new UseSlideProjectorMessage(false));
		}
	}

	public override void SendInitialState(uint to) =>
		this.SendMessage(new UseSlideProjectorMessage(_user) { To = to });

	/// <summary>
	/// called both locally and remotely
	/// </summary>
	public void SetUser(uint user)
	{
		AttachedObject._interactReceiver.SetInteractionEnabled(user == 0 || user == _user);
		_user = user;

		if (user != 0)
		{
			if (AttachedObject._slideItem != null && AttachedObject.IsProjectorFullyLit())
			{
				AttachedObject._slideItem.slidesContainer.TryPlayMusicForCurrentSlideInclusive();
			}
		}
		else
		{
			Locator.GetSlideReelMusicManager().OnExitSlideProjector();
		}
	}
}
