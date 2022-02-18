using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Tools
{
	public class QSBTool : PlayerTool
	{
		public PlayerInfo Player { get; set; }
		public ToolType Type { get; set; }
		public GameObject ToolGameObject
		{
			get => _toolGameObject;

			set
			{
				_toolGameObject = value;
				Delay.RunFramesLater(5, () =>
				{
					_ditheringAnimator = _toolGameObject.AddComponent<DitheringAnimator>();
					// get inactive renderers too
					_ditheringAnimator._renderers = _ditheringAnimator
						.GetComponentsInChildren<Renderer>(true)
						.Select(x => x.gameObject.GetAddComponent<OWRenderer>())
						.ToArray();
				});
			}
		}
		private GameObject _toolGameObject;
		private DitheringAnimator _ditheringAnimator;

		public DampedSpringQuat MoveSpring
		{
			get => _moveSpring;
			set => _moveSpring = value;
		}

		public Transform StowTransform
		{
			get => _stowTransform;
			set => _stowTransform = value;
		}

		public Transform HoldTransform
		{
			get => _holdTransform;
			set => _holdTransform = value;
		}

		public float ArrivalDegrees
		{
			get => _arrivalDegrees;
			set => _arrivalDegrees = value;
		}

		protected bool _isDitheringOut;

		public override void Start()
		{
			base.Start();
			ToolGameObject?.SetActive(false);
		}

		public virtual void OnEnable() => ToolGameObject?.SetActive(true);

		public virtual void OnDisable()
		{
			if (!_isDitheringOut)
			{
				ToolGameObject?.SetActive(false);
			}
		}

		public void ChangeEquipState(bool equipState)
		{
			if (equipState)
			{
				EquipTool();
				return;
			}

			UnequipTool();
		}

		public override void EquipTool()
		{
			base.EquipTool();

			if (_ditheringAnimator != null && _ditheringAnimator._renderers != null)
			{
				ToolGameObject?.SetActive(true);
				_ditheringAnimator.SetVisible(true, 5f);
			}

			Player.AudioController.PlayEquipTool();
		}

		public override void UnequipTool()
		{
			base.UnequipTool();

			if (_ditheringAnimator != null && _ditheringAnimator._renderers != null)
			{
				_isDitheringOut = true;
				_ditheringAnimator.SetVisible(false, 5f);
				Delay.RunWhen(() => _ditheringAnimator._visibleFraction == 0, FinishDitherOut);
			}

			Player.AudioController.PlayUnequipTool();
		}

		public virtual void FinishDitherOut()
		{
			ToolGameObject?.SetActive(false);
			_isDitheringOut = false;
		}
	}
}