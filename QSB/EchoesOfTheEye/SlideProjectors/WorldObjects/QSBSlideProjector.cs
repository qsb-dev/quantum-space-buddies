using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.SlideProjectors.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
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

	private void OnPlayerLeave(PlayerInfo obj) =>
		this.SendMessage(new UseSlideProjectorMessage(false));

	public override void SendInitialState(uint to) =>
		this.SendMessage(new UseSlideProjectorMessage(_user) { To = to });

	/// <summary>
	/// called both locally and remotely
	/// </summary>
	public void SetUser(uint user)
	{
		DebugLog.DebugWrite($"{this} - user = {user}");
		AttachedObject._interactReceiver.SetInteractionEnabled(user == 0 || user == _user);
		_user = user;
	}

	public void NextSlide()
	{
		var hasChangedSlide = false;
		if (AttachedObject._slideItem != null && AttachedObject._slideItem.slidesContainer.NextSlideAvailable())
		{
			hasChangedSlide = AttachedObject._slideItem.slidesContainer.IncreaseSlideIndex();
			if (hasChangedSlide)
			{
				if (AttachedObject._oneShotSource != null)
				{
					AttachedObject._oneShotSource.PlayOneShot(AudioType.Projector_Next);
				}

				if (AttachedObject.IsProjectorFullyLit())
				{
					AttachedObject._slideItem.slidesContainer.SetCurrentRead();
					AttachedObject._slideItem.slidesContainer.TryPlayMusicForCurrentSlideTransition(true);
				}
			}
		}

		if (AttachedObject._gearInterface != null)
		{
			var audioVolume = hasChangedSlide ? 0f : 0.5f;
			AttachedObject._gearInterface.AddRotation(45f, audioVolume);
		}
	}

	public void PreviousSlide()
	{
		var hasChangedSlide = false;
		if (AttachedObject._slideItem != null && AttachedObject._slideItem.slidesContainer.PrevSlideAvailable())
		{
			hasChangedSlide = AttachedObject._slideItem.slidesContainer.DecreaseSlideIndex();
			if (hasChangedSlide)
			{
				if (AttachedObject._oneShotSource != null)
				{
					AttachedObject._oneShotSource.PlayOneShot(AudioType.Projector_Prev);
				}

				if (AttachedObject.IsProjectorFullyLit())
				{
					AttachedObject._slideItem.slidesContainer.SetCurrentRead();
					AttachedObject._slideItem.slidesContainer.TryPlayMusicForCurrentSlideTransition(false);
				}
			}
		}

		if (AttachedObject._gearInterface != null)
		{
			var audioVolume = hasChangedSlide ? 0f : 0.5f;
			AttachedObject._gearInterface.AddRotation(-45f, audioVolume);
		}
	}
}
