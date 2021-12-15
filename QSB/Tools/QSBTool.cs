using QSB.Player;
using UnityEngine;

namespace QSB.Tools
{
	public class QSBTool : PlayerTool
	{
		public PlayerInfo Player { get; set; }
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

		public virtual void OnEnable() => ToolGameObject?.SetActive(true);
		public virtual void OnDisable() => ToolGameObject?.SetActive(false);

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
			Player.AudioController.PlayEquipTool();
		}

		public override void UnequipTool()
		{
			base.UnequipTool();
			Player.AudioController.PlayUnequipTool();
		}
	}
}