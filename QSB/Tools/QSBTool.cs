using QSB.Player;
using QSB.PlayerBodySetup.Remote;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools;

[UsedInUnityProject]
public class QSBTool : PlayerTool
{
	public PlayerInfo Player { get; set; }
	public ToolType Type { get; set; }
	public GameObject ToolGameObject { get; set; }
	[SerializeField]
	private QSBDitheringAnimator _ditheringAnimator;

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
		_ditheringAnimator.SetVisible(false);
	}

	public virtual void OnEnable()
	{
		if (!Player?.FlyingShip ?? false)
		{
			ToolGameObject?.SetActive(true);
		}
	}

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

		if (!Player.FlyingShip)
		{
			ToolGameObject?.SetActive(true);
			Player.AudioController.PlayEquipTool();
		}

		if (_ditheringAnimator != null)
		{
			
			_ditheringAnimator.SetVisible(true, .2f);
		}
	}

	public override void UnequipTool()
	{
		base.UnequipTool();

		if (_ditheringAnimator != null)
		{
			_isDitheringOut = true;
			_ditheringAnimator.SetVisible(false, .2f);
			Delay.RunWhen(() => _ditheringAnimator.FullyInvisible, FinishDitherOut);
		}

		Player.AudioController.PlayUnequipTool();
	}

	public virtual void FinishDitherOut()
	{
		ToolGameObject?.SetActive(false);
		_isDitheringOut = false;
	}
}