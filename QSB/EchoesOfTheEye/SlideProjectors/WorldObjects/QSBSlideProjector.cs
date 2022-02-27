using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.SlideProjectors.WorldObjects
{
	public class QSBSlideProjector : WorldObject<SlideProjector>
	{
		public uint ControllingPlayer;

		public override async UniTask Init(CancellationToken ct)
		{
			DebugLog.DebugWrite($"Init {this}");
		}

		public override void SendInitialState(uint to)
		{
			// todo SendInitialState
		}

		public void OnChangeAuthority(uint newOwner)
		{
			DebugLog.DebugWrite($"{this} change ControllingPlayer to {newOwner}");
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
}