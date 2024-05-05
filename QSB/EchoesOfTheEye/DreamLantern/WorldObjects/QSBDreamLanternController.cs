﻿using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System.Threading;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamLantern.WorldObjects;

public class QSBDreamLanternController : WorldObject<DreamLanternController>
{
	public DreamLanternItem DreamLanternItem { get; private set; }

	public Transform[] NonVMFocuserPetals;
	public Transform[] NonVMConcealerRoots;
	public Transform[] NonVMConcealerCovers;

	public override async UniTask Init(CancellationToken ct)
	{
		// Ghosts don't have the item and instead the effects are controlled by GhostEffects
		if (!IsGhostLantern)
		{
			DreamLanternItem = AttachedObject.GetComponent<DreamLanternItem>();

			if (DreamLanternItem == null) // ghost lanterns don't have DreamLanternItems attached
			{
				return;
			}

			if (DreamLanternItem._lanternType == DreamLanternType.Malfunctioning)
			{
				return;
			}

			AttachedObject._lensFlare.brightness = 0.5f; // ghost lanterns use this. in vanilla its 0
			// also has blue lens flare instead of green. keep it like that for gamplay or wtv
			AttachedObject._origLensFlareBrightness = AttachedObject._lensFlare.brightness;

			// Find non-viewmodel transforms for remote player animations

			var focuser = AttachedObject._worldModelGroup.transform.Find("Focuser");
			NonVMFocuserPetals = new Transform[5]
			{
				focuser.Find("Panel_01"),
				focuser.Find("Panel_02"),
				focuser.Find("Panel_03"),
				focuser.Find("Panel_04"),
				focuser.Find("Panel_05")
			};

			var lanternHood = AttachedObject._worldModelGroup.transform.Find("LanternHood");
			var hoodBottom = lanternHood.Find("Hood_Bottom");
			var hoodTop = lanternHood.Find("Hood_Top");
			NonVMConcealerRoots = new Transform[2]
			{
				hoodBottom,
				hoodTop
			};

			NonVMConcealerCovers = new Transform[6]
			{
				hoodTop.Find("Cover_01"),
				hoodTop.Find("Cover_02"),
				hoodTop.Find("Cover_03"),
				hoodBottom.Find("Cover_04"),
				hoodBottom.Find("Cover_05"),
				hoodBottom.Find("Cover_06")
			};
		}
	}

	public bool IsGhostLantern => AttachedObject.name == "GhostLantern"; // it's as shrimple as that
}
