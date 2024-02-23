using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Menus;

internal class PreflightChecklistAdjustment : MonoBehaviour, IAddComponentOnStart
{
	private string[] _preflightOptionsToRemove = new string[]
	{
		"UIElement-FreezeTimeTranslating",
		"UIElement-FreezeTimeShipLog",
		"UIElement-FreezeTimeConversations",
		"UIElement-FreezeTimeTranslator",
		"UIElement-FreezeTimeDialogue"
	};

	private MenuOption[] DestroyFreezeTimeOptions(MenuOption[] options)
	{
		var remainingMenuOptions = new List<MenuOption>();
		foreach (var preflightChecklistOption in options)
		{
			if (_preflightOptionsToRemove.Contains(preflightChecklistOption.name))
			{
				GameObject.Destroy(preflightChecklistOption.gameObject);
			}
			else
			{
				remainingMenuOptions.Add(preflightChecklistOption);
			}
		}
		return remainingMenuOptions.ToArray();
	}

	public void Awake()
	{
		QSBSceneManager.OnPostSceneLoad += (_, loadScene) =>
		{
			if (QSBCore.IsInMultiplayer && loadScene.IsUniverseScene())
			{
				// PREFLIGHT MENU IN THE SHIP
				var suitMenuManager = GameObject.FindObjectOfType<SuitMenuManager>()._mainMenu;
				suitMenuManager._menuOptions = DestroyFreezeTimeOptions(suitMenuManager._menuOptions);

				// Remove cosmetic elements from ship preflight checklist
				var suitOptionsMenu = GameObject.Find("PauseMenu/PreFlightCanvas/OptionsMenu-Panel/SuitOptionsDisplayPanel/SuitOptionsMainMenu/");
				GameObject.Destroy(suitOptionsMenu.transform.Find("FreezeTimeImage").gameObject);
				GameObject.Destroy(suitOptionsMenu.transform.Find("Box-FreezeTimeBorder").gameObject);


				// PREFLIGHT MENU IN THE OPTIONS MENU
				var settingsMenuView = GameObject.FindObjectOfType<SettingsMenuView>();
				settingsMenuView._listSettingsOptions = DestroyFreezeTimeOptions(settingsMenuView._listSettingsOptions);

				// This one also points to the same options, have to remove the destroyed objects from it
				var menuGameplayPreFlight = GameObject.Find("PauseMenu/OptionsCanvas/OptionsMenu-Panel/OptionsDisplayPanel/GameplayMenu/MenuGameplayPreFl/").GetComponent<Menu>();
				Delay.RunNextFrame(() => menuGameplayPreFlight._menuOptions = menuGameplayPreFlight._menuOptions.Where(x => x != null).ToArray());
			}
		};
	}
}
