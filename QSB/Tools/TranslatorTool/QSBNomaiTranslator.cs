using UnityEngine;

namespace QSB.Tools.TranslatorTool
{
	class QSBNomaiTranslator : QSBTool
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
			=> _translatorProp.OnFinishUnequipAnimation();

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

				if (_currentNomaiText is NomaiWallText)
				{
					var nomaiTextLine = (_currentNomaiText as NomaiWallText).GetClosestTextLineByCenter(raycastHit.point);
					if (_lastLineLocked)
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
						_translatorProp.SetNomaiTextLine(nomaiTextLine);
					}
					else
					{
						_translatorProp.ClearNomaiTextLine();
						_lastHighlightedTextLine = null;
						_lastLineWasTranslated = false;
						_lastLineLocked = false;
					}
				}
				else if (_currentNomaiText is NomaiComputer)
				{
					var closestRing = (_currentNomaiText as NomaiComputer).GetClosestRing(raycastHit.point, out var num2);
					if (closestRing)
					{
						distToClosestTextCenter = Mathf.Min(num2 * 2f, 1f);
						_translatorProp.SetNomaiComputerRing(closestRing);
					}
				}
				else if (_currentNomaiText is NomaiVesselComputer)
				{
					var closestRing2 = (_currentNomaiText as NomaiVesselComputer).GetClosestRing(raycastHit.point, out var num3);
					if (closestRing2)
					{
						distToClosestTextCenter = Mathf.Min(num3 * 2f, 1f);
						_translatorProp.SetNomaiVesselComputerRing(closestRing2);
					}
				}
				else if (_currentNomaiText is GhostWallText)
				{
					var ghostWallText = _currentNomaiText as GhostWallText;
					_translatorProp.SetNomaiTextLine(ghostWallText.GetTextLine());
				}
			}
			else
			{
				_translatorProp.ClearNomaiTextLine();
				_translatorProp.ClearNomaiComputerRing();
				_translatorProp.ClearNomaiVesselComputerRing();
			}

			_translatorProp.SetTooCloseToTarget(tooCloseToTarget);
		}
	}
}
