using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Animation
{
    public class QSBFlashlight : MonoBehaviour
    {
		void Awake()
		{
			GlobalMessenger.AddListener("TurnOnFlashlight", TurnOn);
			GlobalMessenger.AddListener("TurnOffFlashlight", TurnOff);
		}
        private void Start()
        {
            this._baseForward = this._basePivot.forward;
            this._baseRotation = this._basePivot.rotation;
        }

		public void TurnOn()
		{
			if (!this._flashlightOn)
			{
				for (int i = 0; i < this._lights.Length; i++)
				{
					this._lights[i].GetLight().enabled = true;
				}
				this._flashlightOn = true;
				//Locator.GetPlayerAudioController().PlayTurnOnFlashlight();
				Quaternion rotation = this._root.rotation;
				this._basePivot.rotation = rotation;
				this._baseRotation = rotation;
				this._baseForward = this._basePivot.forward;
			}
		}

		public void TurnOff()
		{
			if (this._flashlightOn)
			{
				for (int i = 0; i < this._lights.Length; i++)
				{
					this._lights[i].GetLight().enabled = false;
				}
				this._flashlightOn = false;
				//Locator.GetPlayerAudioController().PlayTurnOffFlashlight();
			}
		}
		private void FixedUpdate()
		{
			Quaternion lhs = Quaternion.FromToRotation(this._basePivot.up, this._root.up) * Quaternion.FromToRotation(this._baseForward, this._root.forward);
			Quaternion b = lhs * this._baseRotation;
			this._baseRotation = Quaternion.Slerp(this._baseRotation, b, 6f * Time.deltaTime);
			this._basePivot.rotation = this._baseRotation;
			this._baseForward = this._basePivot.forward;
			this._wobblePivot.localRotation = OWUtilities.GetWobbleRotation(0.3f, 0.15f) * Quaternion.identity;
		}

		public OWLight2[] _lights;

		public OWLight2 _illuminationCheckLight;

		public Transform _root;

		public Transform _basePivot;

		public Transform _wobblePivot;

		private bool _flashlightOn;

		private Vector3 _baseForward;

		private Quaternion _baseRotation;
	}
}
