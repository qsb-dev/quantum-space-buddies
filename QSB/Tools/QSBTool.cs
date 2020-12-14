using UnityEngine;

namespace QSB.Tools
{
	public class QSBTool : PlayerTool
	{
		public ToolType Type { get; set; }
		public GameObject ToolGameObject { get; set; }

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

		public void OnEnable() => ToolGameObject?.SetActive(true);
        public void OnDisable() => ToolGameObject?.SetActive(false);

		public void ChangeEquipState(bool equipState)
		{
			if (equipState)
			{
				EquipTool();
				return;
			}
			UnequipTool();
		}
	}
}