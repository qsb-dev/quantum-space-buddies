using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner;

public class CustomSlideCollectionContainer : MonoBehaviour
{
	[SerializeField]
	public CustomSlideCollection _slideCollection = new(0);

	private bool _changeSlidesAllowed;
	private int _currentSlideIndex;
	private bool _initialized;

	public int slideCount
	{
		get
		{
			if (this._slideCollection.slides == null)
			{
				return 0;
			}

			return this._slideCollection.slides.Length;
		}
	}

	public bool isEndOfSlide
	{
		get
		{
			return this._currentSlideIndex >= this.slideCount - 1;
		}
	}

	public int slideIndex
	{
		get
		{
			return this._currentSlideIndex;
		}
		set
		{
			if (!this._changeSlidesAllowed)
			{
				return;
			}

			if (this._currentSlideIndex == value)
			{
				return;
			}

			var forward = this._currentSlideIndex < value;
			this._currentSlideIndex = value;
			if (this._currentSlideIndex > this._slideCollection.slides.Length - 1)
			{
				this._currentSlideIndex = 0;
			}

			if (this._currentSlideIndex < 0)
			{
				this._currentSlideIndex = this._slideCollection.slides.Length - 1;
			}

			this.GetCurrentSlide().Display(this, forward);
		}
	}

	public OWEvent onSlideTextureUpdated = new(1);
	public OWEvent onEndOfSlides = new(1);

	public void Initialize()
	{
		if (this._initialized)
		{
			return;
		}

		this._changeSlidesAllowed = true;
		this._initialized = true;
	}

	public CustomSlide GetCurrentSlide()
	{
		if (this._slideCollection.slides.Length == 0)
		{
			return null;
		}

		return this._slideCollection.slides[this._currentSlideIndex];
	}

	public Texture GetCurrentSlideTexture()
	{
		if (this._slideCollection.slides.Length == 0)
		{
			DebugLog.DebugWrite($"NO SLIDES!", OWML.Common.MessageType.Error);
			return null;
		}

		return this.GetCurrentSlide().GetTexture();
	}

	public void ResetSlideIndex()
	{
		this.slideIndex = 0;
		this.GetCurrentSlide().SetOwner(this);
	}

	public bool IncreaseSlideIndex()
	{
		if (!this._changeSlidesAllowed)
		{
			return false;
		}

		var slideIndex = this.slideIndex;
		this.slideIndex = slideIndex + 1;
		if (this.slideIndex == 0)
		{
			this.onEndOfSlides.Invoke();
		}

		return true;
	}

	public bool DecreaseSlideIndex()
	{
		if (!this._changeSlidesAllowed)
		{
			return false;
		}

		var slideIndex = this.slideIndex;
		this.slideIndex = slideIndex - 1;
		return true;
	}

	public void SetChangeSlidesAllowed(bool allowed)
	{
		this._changeSlidesAllowed = allowed;
	}
}
