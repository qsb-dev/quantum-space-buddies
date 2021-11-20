using System.Collections.Generic;
using UnityEngine;

namespace QSB.Utility
{
	internal class RepeatingManager : MonoBehaviour
	{
		public static List<IRepeating> Repeatings = new();

		private const float TimeInterval = 0.4f;
		private float _checkTimer = TimeInterval;

		private void Update()
		{
			_checkTimer += Time.unscaledDeltaTime;
			if (_checkTimer < TimeInterval)
			{
				return;
			}

			foreach (var repeat in Repeatings)
			{
				repeat.Invoke();
			}

			_checkTimer = 0;
		}
	}
}
