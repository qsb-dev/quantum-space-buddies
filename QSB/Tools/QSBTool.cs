using QSB.Player;
using QSB.Utility;
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
				QSBCore.UnityEvents.FireInNUpdates(
					() => DitheringAnimator = _toolGameObject.AddComponent<DitheringAnimator>(),
					5);
			}
		}
		private GameObject _toolGameObject;
		public DitheringAnimator DitheringAnimator { get; set; }

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

			if (DitheringAnimator != null && DitheringAnimator._renderers != null)
			{
				ToolGameObject?.SetActive(true);
				DitheringAnimator.SetVisible(true, 5f);
			}
			
			Player.AudioController.PlayEquipTool();
		}

		public override void UnequipTool()
		{
			base.UnequipTool();

			if (DitheringAnimator != null && DitheringAnimator._renderers != null)
			{
				_isDitheringOut = true;
				DitheringAnimator.SetVisible(false, 5f);
				QSBCore.UnityEvents.RunWhen(() => DitheringAnimator._visibleFraction == 0, FinishDitherOut);
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