using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner;

internal class CustomAutoSlideProjector : MonoBehaviour
{
	public float _defaultSlideDuration;
	public float _endPauseDuration;

	[SerializeField]
	public CustomSlideCollectionContainer _slideCollectionItem;

	public OWLight2 _light;

	[SerializeField]
	[Space]
	private OWAudioSource _oneShotAudio;

	private float _lastSlidePlayTime;
	private float _startPausingEndTime;
	private bool _isPlaying;
	private bool _isPausingEnd;

	protected void Awake()
	{
		if (this._slideCollectionItem != null)
		{
			this._slideCollectionItem.onSlideTextureUpdated += this.OnSlideTextureUpdated;
			this._slideCollectionItem.Initialize();
			this._slideCollectionItem.enabled = false;
		}
		else
		{
			DebugLog.DebugWrite($"COLLECTION ITEM NULL IN AWAKE", OWML.Common.MessageType.Error);
		}

		base.enabled = false;
	}

	protected void OnDestroy()
	{
		if (this._slideCollectionItem != null)
		{
			this._slideCollectionItem.onSlideTextureUpdated -= this.OnSlideTextureUpdated;
		}
	}

	public bool IsPlaying()
	{
		return this._isPlaying;
	}

	public void Play(bool reset)
	{
		if (this._isPlaying)
		{
			return;
		}

		this._light.SetActivation(true);
		if (reset)
		{
			this._slideCollectionItem.ResetSlideIndex();
		}

		this.UpdateSlideTexture();
		this._lastSlidePlayTime = Time.time;
		this._isPlaying = true;
		base.enabled = true;
	}

	public void Stop()
	{
		if (!this._isPlaying)
		{
			return;
		}

		this._isPlaying = false;
		base.enabled = false;
		this._slideCollectionItem.enabled = false;
	}

	public void TurnOff()
	{
		this.Stop();
		this._oneShotAudio.PlayOneShot(global::AudioType.Lantern_Remove, 1f);
		this._light.SetActivation(false);
	}

	public void SetSlideCollection(CustomSlideCollectionContainer collection)
	{
		if (this._slideCollectionItem != null)
		{
			if (this._isPlaying)
			{
				this._slideCollectionItem.enabled = false;
			}

			this._slideCollectionItem.onSlideTextureUpdated -= this.OnSlideTextureUpdated;
		}

		this._slideCollectionItem = collection;
		this._slideCollectionItem.onSlideTextureUpdated += this.OnSlideTextureUpdated;
		this._slideCollectionItem.Initialize();
		if (this._isPlaying)
		{
			this.UpdateSlideTexture();
		}
	}

	protected virtual void Update()
	{
		if (this._isPlaying)
		{
			if (this._isPausingEnd)
			{
				if (Time.time >= this._endPauseDuration + this._startPausingEndTime)
				{
					this._isPausingEnd = false;
					this.FirstSlide();
				}

				return;
			}

			if (Time.time >= this.GetCurrentSlidePlayDuration() + this._lastSlidePlayTime)
			{
				if (!this._slideCollectionItem.isEndOfSlide)
				{
					this.NextSlide();
					return;
				}

				if (this._endPauseDuration > 0f)
				{
					this._isPausingEnd = true;
					this._startPausingEndTime = Time.time;
					return;
				}

				this.FirstSlide();
			}
		}
	}

	private void OnSlideTextureUpdated()
	{
		this.UpdateSlideTexture();
	}

	private void UpdateSlideTexture()
	{
		if (_light == null)
		{
			DebugLog.DebugWrite($"- Light is null!");
		}

		if (_slideCollectionItem == null)
		{
			DebugLog.DebugWrite($"- slide collection item is null!");
		}

		this._light.GetLight().cookie = this._slideCollectionItem.GetCurrentSlideTexture();
	}

	private void FirstSlide()
	{
		this._slideCollectionItem.ResetSlideIndex();
		this._lastSlidePlayTime = Time.time;
		if (this._oneShotAudio != null)
		{
			this._oneShotAudio.PlayOneShot(global::AudioType.Projector_Next, 1f);
		}
	}

	private void NextSlide()
	{
		this._slideCollectionItem.IncreaseSlideIndex();
		this._lastSlidePlayTime = Time.time;
		if (this._oneShotAudio != null)
		{
			this._oneShotAudio.PlayOneShot(global::AudioType.Projector_Next, 1f);
		}
	}

	private float GetCurrentSlidePlayDuration()
	{
		return this._defaultSlideDuration;
	}
}
