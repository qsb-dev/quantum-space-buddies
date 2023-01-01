using HarmonyLib;
using QSB.Patches;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(ProfileMenuManager))]
internal class ProfileMenuManagerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;
	public override GameVendor PatchVendor => GameVendor.Epic | GameVendor.Steam;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.OnCreateProfileConfirm))]
	public static bool OnCreateProfileConfirm(ProfileMenuManager __instance)
	{
		__instance._inputPopupActivated = false;
		var inputPopup = __instance._createProfileAction.GetInputPopup();
		inputPopup.OnPopupValidate -= __instance.OnCreateProfileValidate;
		inputPopup.OnInputPopupValidateChar -= __instance.OnValidateChar;
		__instance._createProfileAction.OnSubmitAction -= __instance.OnCreateProfileConfirm;
		QSBStandaloneProfileManager.SharedInstance.TryCreateProfile(__instance._createProfileAction.GetInputString());
		inputPopup.CloseMenuOnOk(true);
		__instance.PopulateProfiles();
		__instance.SetCurrentProfileLabel();
		inputPopup.EnableMenu(false);
		if (__instance._firstTimeProfileCreation)
		{
			__instance._firstTimeProfileCreation = false;
			__instance.UpdatePopupPrompts();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.OnCreateProfileValidate))]
	public static bool OnCreateProfileValidate(ProfileMenuManager __instance, ref bool __result)
	{
		var inputPopup = __instance._createProfileAction.GetInputPopup();
		__result = QSBStandaloneProfileManager.SharedInstance.ValidateProfileName(inputPopup.GetInputText());
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.OnDeleteProfile))]
	public static bool OnDeleteProfile(ProfileMenuManager __instance)
	{
		if (__instance._lastSelectedProfileAction != null)
		{
			__instance._deleteProfileConfirmPopup = null;
			QSBStandaloneProfileManager.SharedInstance.DeleteProfile(__instance._lastSelectedProfileAction.GetLabelText());
			__instance.PopulateProfiles();
			__instance._lastSelectedProfileAction = null;
			Locator.GetMenuInputModule().SelectOnNextUpdate(__instance._createProfileButton);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.OnSwitchProfile))]
	public static bool OnSwitchProfile(ProfileMenuManager __instance)
	{
		if (__instance._lastSelectedProfileAction != null)
		{
			__instance._switchProfileConfirmPopup = null;
			if (QSBStandaloneProfileManager.SharedInstance.SwitchProfile(__instance._lastSelectedProfileAction.GetLabelText()))
			{
				__instance.PopulateProfiles();
				__instance.SetCurrentProfileLabel();
				__instance._lastSelectedProfileAction = null;
				Locator.GetMenuInputModule().SelectOnNextUpdate(__instance._createProfileButton);
				return false;
			}

			QSBStandaloneProfileManager.SharedInstance.OnBackupDataRestored += __instance.OnSwitchProfileDataRecoveryCompleted;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.OnSwitchProfileDataRecoveryCompleted))]
	public static bool OnSwitchProfileDataRecoveryCompleted(ProfileMenuManager __instance)
	{
		QSBStandaloneProfileManager.SharedInstance.OnBackupDataRestored -= __instance.OnSwitchProfileDataRecoveryCompleted;
		__instance.PopulateProfiles();
		__instance.SetCurrentProfileLabel();
		__instance._lastSelectedProfileAction = null;
		Locator.GetMenuInputModule().SelectOnNextUpdate(__instance._createProfileButton);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.OnValidateChar))]
	public static bool OnValidateChar(ProfileMenuManager __instance, char c, ref bool __result)
	{
		__result = __instance._createProfileAction.GetInputPopup().GetInputText().Length < QSBStandaloneProfileManager.SharedInstance.profileNameCharacterLimit
			&& QSBStandaloneProfileManager.SharedInstance.IsValidCharacterForProfileName(c);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.PopulateProfiles))]
	public static bool PopulateProfiles(ProfileMenuManager __instance)
	{
		if (__instance._listProfileElements == null)
		{
			__instance._listProfileElements = new List<GameObject>();
		}
		else
		{
			for (var i = 0; i < __instance._listProfileElements.Count; i++)
			{
				var requiredComponent = __instance._listProfileElements[i].GetRequiredComponent<TwoButtonActionElement>();
				__instance.ClearProfileElementListeners(requiredComponent);
				Object.Destroy(__instance._listProfileElements[i]);
			}

			__instance._listProfileElements.Clear();
		}

		if (__instance._listProfileUIElementLookup == null)
		{
			__instance._listProfileUIElementLookup = new List<ProfileMenuManager.ProfileElementLookup>();
		}
		else
		{
			__instance._listProfileUIElementLookup.Clear();
		}

		var array = QSBStandaloneProfileManager.SharedInstance.profiles.ToArray();
		var profileName = QSBStandaloneProfileManager.SharedInstance.currentProfile.profileName;
		var num = 0;
		Selectable selectable = null;
		for (var j = 0; j < array.Length; j++)
		{
			if (!(array[j].profileName == profileName))
			{
				var gameObject = Object.Instantiate<GameObject>(__instance._profileItemTemplate);
				gameObject.gameObject.SetActive(true);
				gameObject.transform.SetParent(__instance._profileListRoot.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				var componentsInChildren = gameObject.gameObject.GetComponentsInChildren<Text>();
				for (var k = 0; k < componentsInChildren.Length; k++)
				{
					__instance._fontController.AddTextElement(componentsInChildren[k], true, true, false);
				}

				num++;
				var requiredComponent2 = gameObject.GetRequiredComponent<TwoButtonActionElement>();
				var requiredComponent3 = requiredComponent2.GetRequiredComponent<Selectable>();
				__instance.SetUpProfileElementListeners(requiredComponent2);
				requiredComponent2.SetLabelText(array[j].profileName);
				var component = requiredComponent2.GetButtonOne().GetComponent<Text>();
				if (component != null)
				{
					__instance._fontController.AddTextElement(component, true, true, false);
				}

				component = requiredComponent2.GetButtonTwo().GetComponent<Text>();
				if (component != null)
				{
					__instance._fontController.AddTextElement(component, true, true, false);
				}

				if (num == 1)
				{
					var navigation = __instance._createProfileButton.navigation;
					navigation.selectOnDown = gameObject.GetRequiredComponent<Selectable>();
					__instance._createProfileButton.navigation = navigation;
					var navigation2 = requiredComponent3.navigation;
					navigation2.selectOnUp = __instance._createProfileButton;
					requiredComponent3.navigation = navigation2;
				}
				else
				{
					var navigation3 = requiredComponent3.navigation;
					var navigation4 = selectable.navigation;
					navigation3.selectOnUp = selectable;
					navigation3.selectOnDown = null;
					navigation4.selectOnDown = requiredComponent3;
					requiredComponent3.navigation = navigation3;
					selectable.navigation = navigation4;
				}

				__instance._listProfileElements.Add(gameObject);
				selectable = requiredComponent3;
				var profileElementLookup = new ProfileMenuManager.ProfileElementLookup
				{
					profileName = array[j].profileName,
					lastModifiedTime = array[j].lastModifiedTime,
					confirmSwitchAction = requiredComponent2.GetSubmitActionOne() as SubmitActionConfirm,
					confirmDeleteAction = requiredComponent2.GetSubmitActionTwo() as SubmitActionConfirm
				};
				__instance._listProfileUIElementLookup.Add(profileElementLookup);
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileMenuManager.SetCurrentProfileLabel))]
	public static bool SetCurrentProfileLabel(ProfileMenuManager __instance)
	{
		__instance._currenProfileLabel.text = UITextLibrary.GetString(UITextType.MenuProfile)
			+ " "
			+ QSBStandaloneProfileManager.SharedInstance.currentProfile.profileName;
		return false;
	}
}
