using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Tools.TranslatorTool.Messages;
using UnityEngine;

namespace QSB.Tools.TranslatorTool.Patches;

internal class TranslatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.Update))]
	public static bool Update(NomaiTranslatorProp __instance)
	{
		var targetTranslated = __instance._nomaiTextComponent != null && __instance._nomaiTextComponent.IsTranslated(__instance._currentTextID);
		var canTranslate = !__instance._isTargetingGhostText && !targetTranslated && __instance._textNodeToDisplay != null;

		__instance._centerTranslatePrompt.SetVisibility(canTranslate && !__instance._hasUsedTranslator);
		__instance._translatePrompt.SetVisibility(canTranslate && __instance._hasUsedTranslator);
		if (!__instance._hasUsedTranslator && targetTranslated && Locator.GetPlayerSuit().IsWearingHelmet())
		{
			PlayerData.SetPersistentCondition("HAS_USED_TRANSLATOR", true);
			__instance._hasUsedTranslator = true;
		}

		__instance.UpdateTimeFreeze(targetTranslated, __instance._nomaiTextComponent);
		if (__instance._textNodeToDisplay != null && __instance._textNodeToDisplay != __instance._lastTextNodeToDisplay)
		{
			__instance.StopTranslating();
			__instance.SwitchTextNode(__instance._textNodeToDisplay);
		}

		__instance._lastTextNodeToDisplay = __instance._textNodeToDisplay;
		if (__instance._isTargetingGhostText)
		{
			__instance._textField.text = UITextLibrary.GetString(__instance._isTooCloseToTarget
				? UITextType.TranslatorTooCloseWarning
				: UITextType.TranslatorUntranslatableWarning);
		}
		else if (__instance._textNodeToDisplay == null)
		{
			__instance._textField.text = "";
			__instance._pageNumberTextField.text = "";
			__instance.StopTranslating();
		}
		else if (__instance._isTooCloseToTarget)
		{
			__instance._textField.text = UITextLibrary.GetString(UITextType.TranslatorTooCloseWarning);
		}
		else
		{
			__instance.DisplayTextNode();
		}

		__instance._textField.rectTransform.sizeDelta = new Vector2(__instance._textField.rectTransform.sizeDelta.x, __instance._textField.preferredHeight);
		__instance._scrollRect.content.sizeDelta = new Vector2(__instance._scrollRect.content.sizeDelta.x, __instance._textField.preferredHeight);
		var num = __instance._textField.fontSize / (__instance._textField.preferredHeight - __instance._scrollRect.viewport.sizeDelta.y);
		if (OWInput.IsNewlyPressed(InputLibrary.toolOptionUp, InputMode.All))
		{
			__instance._scrollRect.verticalNormalizedPosition = Mathf.Clamp01(__instance._scrollRect.verticalNormalizedPosition + num);
			new TranslatorScrollMessage(__instance._scrollRect.verticalNormalizedPosition).Send();
		}

		if (OWInput.IsNewlyPressed(InputLibrary.toolOptionDown, InputMode.All))
		{
			__instance._scrollRect.verticalNormalizedPosition = Mathf.Clamp01(__instance._scrollRect.verticalNormalizedPosition - num);
			new TranslatorScrollMessage(__instance._scrollRect.verticalNormalizedPosition).Send();
		}

		__instance._scrollPrompt.SetVisibility(__instance._textField.preferredHeight > __instance._scrollRect.viewport.sizeDelta.y);
		var hasPages = __instance._inNomaiAudioVolume
			&& __instance._nomaiTextComponent != null
			&& __instance._nomaiTextComponent.IsTranslated(1)
			&& !__instance._isTranslating
			&& __instance._nomaiTextComponent.GetNumTextBlocks() > 1;

		var currentPage = __instance._currentTextID;
		if (hasPages)
		{
			var numTextBlocks = __instance._nomaiTextComponent.GetNumTextBlocks();
			if (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft, InputMode.All))
			{
				currentPage = Mathf.Max(1, __instance._currentTextID - 1);
			}

			if (OWInput.IsNewlyPressed(InputLibrary.toolOptionRight, InputMode.All))
			{
				currentPage = Mathf.Min(numTextBlocks, __instance._currentTextID + 1);
			}

			if (currentPage != __instance._currentTextID)
			{
				__instance._currentTextID = currentPage;
				if (__instance._nomaiTextComponent != null && __instance._inNomaiAudioVolume)
				{
					__instance._textNodeToDisplay = __instance._nomaiTextComponent.GetTextNode(__instance._currentTextID);
					__instance.SetNomaiAudioArrowEmissions();
				}
			}

			if (__instance._pageNumberTextField != null)
			{
				__instance._pageNumberStrBuilder.Length = 0;
				__instance._pageNumberStrBuilder.Append(currentPage);
				__instance._pageNumberStrBuilder.Append(" / ");
				__instance._pageNumberStrBuilder.Append(numTextBlocks);
				if (__instance._pageNumberTextField.text != __instance._pageNumberStrBuilder.ToString())
				{
					__instance._pageNumberTextField.text = __instance._pageNumberStrBuilder.ToString();
				}
			}
		}
		else if (__instance._pageNumberTextField != null && __instance._pageNumberTextField.text != "")
		{
			__instance._pageNumberTextField.text = "";
		}

		__instance._pagePrompt.SetVisibility(hasPages);

		return false;
	}

	[HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.DisplayTextNode))]
	public static bool DisplayTextNode(NomaiTranslatorProp __instance)
	{
		if (!__instance._nomaiTextComponent.IsTranslated(__instance._currentTextID))
		{
			if (OWInput.IsPressed(InputLibrary.toolActionPrimary, 0f) || (__instance._inNomaiAudioVolume && __instance._currentTextID > 1))
			{
				__instance._translationTimeElapsed += Time.deltaTime;
				if (!__instance._isTranslating)
				{
					new IsTranslatingMessage(true).Send();
					__instance._isTranslating = true;
					__instance._audioController.PlayTranslateAudio();
				}
			}
			else if (__instance._isTranslating)
			{
				new IsTranslatingMessage(false).Send();
				__instance.StopTranslating();
			}
		}
		else if (__instance._nomaiTextComponent.IsTranslated(__instance._currentTextID))
		{
			__instance._translationTimeElapsed += Time.deltaTime;
		}

		var isDynamic = false;
		string text;
		if (__instance._translationTimeElapsed == 0f && !__instance._nomaiTextComponent.IsTranslated(__instance._currentTextID))
		{
			text = __instance._inNomaiAudioVolume ? UITextLibrary.GetString(UITextType.TranslatorUntranslatedRecordingWarning) : UITextLibrary.GetString(UITextType.TranslatorUntranslatedWritingWarning);
		}
		else
		{
			if (__instance._translationTimeElapsed > (float)(__instance._numTranslatedWords + 1) * __instance._perWordTranslationTime && __instance._numTranslatedWords < __instance._listDisplayWords.Count)
			{
				__instance._listDisplayWordsByLength[__instance._numTranslatedWords].BeginTranslation(__instance._perWordTranslationTime);
				__instance._numTranslatedWords++;
			}

			for (var i = 0; i < __instance._listDisplayWords.Count; i++)
			{
				__instance._listDisplayWordsByLength[i].UpdateDisplayText(Time.deltaTime);
			}

			__instance._strBuilder.Length = 0;
			var allWordsTranslated = true;
			for (var j = 0; j < __instance._listDisplayWords.Count; j++)
			{
				var translatorWord = __instance._listDisplayWords[j];
				__instance._strBuilder.Append(translatorWord.DisplayText);
				if (!translatorWord.IsTranslated())
				{
					allWordsTranslated = false;
				}
			}

			text = __instance._strBuilder.ToString();
			if (allWordsTranslated)
			{
				__instance.StopTranslating();
				__instance._nomaiTextComponent.SetAsTranslated(__instance._currentTextID);
				__instance.SetNomaiAudioArrowEmissions();
				if (text.Contains("</i>") || text.Contains("</b>") || text.Contains("</u>") || text.Contains("</size>"))
				{
					isDynamic = true;
				}
			}
		}

		if (isDynamic && __instance._textField.font != __instance._dynamicFontInUse)
		{
			__instance._textField.font = __instance._dynamicFontInUse;
		}
		else if (!isDynamic && __instance._textField.font != __instance._fontInUse)
		{
			__instance._textField.font = __instance._fontInUse;
		}

		__instance._textField.text = text;

		return false;
	}
}
