using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner;

public class CustomSlideCollection
{
	[SerializeField]
	public CustomSlide[] slides;

	public CustomSlideCollection(int startArrSize)
	{
		this.slides = new CustomSlide[startArrSize];
	}

	public CustomSlide this[int i]
	{
		get
		{
			return this.slides[i];
		}
	}
}
