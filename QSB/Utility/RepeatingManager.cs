using System.Collections.Generic;
using UnityEngine;

namespace QSB.Utility
{
	class RepeatingManager : MonoBehaviour
	{
		public static List<IRepeating> Repeatings = new List<IRepeating>();

		private const float TimeInterval = 0.1f;
		private float _checkTimer = TimeInterval;

		void Update()
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
