using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner;

[Serializable]
public class CustomSlide
{
	public Texture2D _image;
	private Texture2D _textureOverride;
	private CustomSlideCollectionContainer _owningItem;
	public bool expanded;

	public Texture2D textureOverride
	{
		get
		{
			return this._textureOverride;
		}
		set
		{
			if (this._textureOverride == value)
			{
				return;
			}

			this._textureOverride = value;
			this.InvokeTextureUpdate();
		}
	}

	public CustomSlide()
	{
		this._image = null;
		this.expanded = false;
	}

	public CustomSlide(CustomSlide other)
	{
		this._image = other._image;
		this.expanded = false;
	}

	public Texture GetTexture()
	{
		if (this._textureOverride != null)
		{
			return this._textureOverride;
		}

		if (_image == null)
		{
			DebugLog.DebugWrite($"IMAGE IS NULL!", OWML.Common.MessageType.Error);
		}

		return this._image;
	}

	public void Display(CustomSlideCollectionContainer owner, bool forward)
	{

		if (owner == null)
		{
			DebugLog.DebugWrite($"OWNER IS NULL IN DISPLAY", OWML.Common.MessageType.Error);
		}

		this._owningItem = owner;
		this.InvokeTextureUpdate();
	}

	public void InvokeTextureUpdate()
	{
		if (this._owningItem != null)
		{
			this._owningItem.onSlideTextureUpdated.Invoke();
		}
		else
		{
			DebugLog.DebugWrite($"OWNING ITEM IS NULL!", OWML.Common.MessageType.Error);
		}
	}

	public void SetChangeSlidesAllowed(bool allowed)
	{
		this._owningItem.SetChangeSlidesAllowed(allowed);
	}

	public void SetOwner(CustomSlideCollectionContainer owner)
	{
		this._owningItem = owner;
	}

	public static CustomSlide CreateSlide(Texture2D texture)
	{
		var slide = new CustomSlide
		{
			_image = texture
		};
		return slide;
	}
}
