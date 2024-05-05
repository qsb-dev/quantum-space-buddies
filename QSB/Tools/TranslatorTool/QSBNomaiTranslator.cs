using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.TranslatorTool;

[UsedInUnityProject]
public class QSBNomaiTranslator : QSBTool
{
	public static float distToClosestTextCenter = 1f;

	public const float MAX_INTERACT_RANGE = 25f;

	public Transform RaycastTransform;
	private Collider _lastHitCollider;
	private QSBNomaiTranslatorProp _translatorProp;
	private NomaiText _currentNomaiText;
	private NomaiTextLine _lastHighlightedTextLine;
	private bool _lastLineWasTranslated;
	private bool _lastLineLocked;
	private float _lastLineDist;

	private void Awake()
	{
		_lastHitCollider = null;
		_translatorProp = this.GetRequiredComponentInChildren<QSBNomaiTranslatorProp>();
		_currentNomaiText = null;
	}

	public override void OnDisable()
	{
		if (!_isDitheringOut)
		{
			_translatorProp.OnFinishUnequipAnimation();
		}
	}

	public override void EquipTool()
	{
		base.EquipTool();
		_translatorProp.OnEquipTool();
	}

	public override void UnequipTool()
	{
		base.UnequipTool();
		_translatorProp.OnUnequipTool();
	}

	public override void FinishDitherOut()
	{
		base.FinishDitherOut();
		_translatorProp.OnFinishUnequipAnimation();
	}

	public void SetScroll(float scrollPosition)
		=> _translatorProp.SetScroll(scrollPosition);

	public void UpdateTranslating(bool translating)
		=> _translatorProp.UpdateTranslating(translating);

	public override void Update()
	{
		base.Update();
		if (!_isEquipped)
		{
			return;
		}

		distToClosestTextCenter = 1f;
		var tooCloseToTarget = false;
		var num = float.MaxValue;
		if (Physics.Raycast(RaycastTransform.position, RaycastTransform.forward, out var raycastHit, 25f, OWLayerMask.blockableInteractMask))
		{
			_lastHitCollider = raycastHit.collider;
			_currentNomaiText = _lastHitCollider.GetComponent<NomaiText>();

			if (_currentNomaiText != null && !_currentNomaiText.CheckAllowFocus(raycastHit.distance, RaycastTransform.forward))
			{
				_currentNomaiText = null;
			}

			num = raycastHit.distance;
		}
		else
		{
			_lastHitCollider = null;
			_currentNomaiText = null;
		}

		if (_currentNomaiText != null)
		{
			tooCloseToTarget = num < _currentNomaiText.GetMinimumReadableDistance();

			if (_currentNomaiText is NomaiWallText nomaiWallText)
			{
				var nomaiTextLine = nomaiWallText.GetClosestTextLineByCenter(raycastHit.point);
				if (_lastLineLocked && _lastHighlightedTextLine != null)
				{
					var distToCenter = _lastHighlightedTextLine.GetDistToCenter(raycastHit.point);
					if (distToCenter > _lastLineDist + 0.1f)
					{
						_lastHighlightedTextLine = nomaiTextLine;
						_lastLineWasTranslated = nomaiTextLine != null && nomaiTextLine.IsTranslated();
						_lastLineLocked = false;
					}
					else
					{
						nomaiTextLine = _lastHighlightedTextLine;
					}

					if (distToCenter < _lastLineDist)
					{
						_lastLineDist = distToCenter;
					}
				}
				else if (_lastHighlightedTextLine != null && _lastHighlightedTextLine.IsTranslated() && !_lastLineWasTranslated)
				{
					_lastLineWasTranslated = true;
					_lastLineDist = _lastHighlightedTextLine.GetDistToCenter(raycastHit.point);
					_lastLineLocked = true;
				}
				else
				{
					_lastHighlightedTextLine = nomaiTextLine;
					_lastLineWasTranslated = nomaiTextLine != null && nomaiTextLine.IsTranslated();
				}

				if (nomaiTextLine && !nomaiTextLine.IsHidden() && nomaiTextLine.IsActive())
				{
					distToClosestTextCenter = Vector3.Distance(raycastHit.point, nomaiTextLine.GetWorldCenter());
					_translatorProp.SetNomaiText(_currentNomaiText, nomaiTextLine.GetEntryID());
					_translatorProp.SetNomaiTextLine(nomaiTextLine);
				}
				else
				{
					_translatorProp.ClearNomaiText();
					_translatorProp.ClearNomaiTextLine();
					_lastHighlightedTextLine = null;
					_lastLineWasTranslated = false;
					_lastLineLocked = false;
				}
			}
			else if (_currentNomaiText is NomaiComputer nomaiComputer)
			{
				var closestRing = nomaiComputer.GetClosestRing(raycastHit.point, out var num2);
				if (closestRing)
				{
					distToClosestTextCenter = Mathf.Min(num2 * 2f, 1f);
					_translatorProp.SetNomaiText(_currentNomaiText, closestRing.GetEntryID());
					_translatorProp.SetNomaiComputerRing(closestRing);
				}
			}
			else if (_currentNomaiText is NomaiVesselComputer nomaiVesselComputer)
			{
				var closestRing2 = nomaiVesselComputer.GetClosestRing(raycastHit.point, out var num3);
				if (closestRing2)
				{
					distToClosestTextCenter = Mathf.Min(num3 * 2f, 1f);
					_translatorProp.SetNomaiText(_currentNomaiText, closestRing2.GetEntryID());
					_translatorProp.SetNomaiVesselComputerRing(closestRing2);
				}
			}
			else if (_currentNomaiText is GhostWallText ghostWallText)
			{
				_translatorProp.SetTargetingGhostText(true);
				_translatorProp.SetNomaiTextLine(ghostWallText.GetTextLine());
			}
			else if (raycastHit.textureCoord2 == Vector2.zero)
			{
				_translatorProp.SetNomaiText(this._currentNomaiText);
			}
			else
			{
				var textID = Mathf.RoundToInt(raycastHit.textureCoord2.x * 10f);
				_translatorProp.SetNomaiText(this._currentNomaiText, textID);
			}
		}
		else
		{
			_translatorProp.ClearNomaiText();
			_translatorProp.ClearNomaiTextLine();
			_translatorProp.ClearNomaiComputerRing();
			_translatorProp.ClearNomaiVesselComputerRing();
			_translatorProp.SetTargetingGhostText(false);
		}

		_translatorProp.SetTooCloseToTarget(tooCloseToTarget);
	}
}