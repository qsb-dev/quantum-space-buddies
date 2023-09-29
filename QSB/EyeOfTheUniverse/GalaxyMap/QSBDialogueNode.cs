using System.Collections.Generic;

namespace QSB.EyeOfTheUniverse.GalaxyMap;

public class QSBDialogueNode
{
	private List<string> _listPagesToDisplay;

	public string Name { get; set; }
	public List<DialogueOption> ListDialogueOptions { get; set; }
	public List<string> ListEntryCondition { get; set; }
	public List<string> ConditionsToSet { get; set; }
	public string PersistentConditionToSet { get; set; }
	public string PersistentConditionToDisable { get; set; }
	public string[] DBEntriesToSet { get; set; }
	public DialogueText DisplayTextData { get; set; }
	public string TargetName { get; set; }
	public List<string> ListTargetCondition { get; set; }
	public QSBDialogueNode Target { get; set; }
	public int CurrentPage { get; private set; }

	public QSBDialogueNode()
	{
		Name = "";
		CurrentPage = -1;
		TargetName = "";
		Target = null;
		ListEntryCondition = new List<string>();
		_listPagesToDisplay = new List<string>();
		ListTargetCondition = new List<string>();
		ListDialogueOptions = new List<DialogueOption>();
		ConditionsToSet = new List<string>();
		PersistentConditionToSet = string.Empty;
		PersistentConditionToDisable = string.Empty;
		DBEntriesToSet = new string[0];
	}

	public void RefreshConditionsData() => _listPagesToDisplay = DisplayTextData.GetDisplayStringList();

	public void GetNextPage(out string mainText, ref List<DialogueOption> options)
	{
		if (HasNext())
		{
			CurrentPage++;
		}

		mainText = _listPagesToDisplay[CurrentPage].Trim();
		if (!HasNext())
		{
			for (var i = 0; i < ListDialogueOptions.Count; i++)
			{
				options.Add(ListDialogueOptions[i]);
			}
		}
	}

	public bool HasNext() => CurrentPage + 1 < _listPagesToDisplay.Count;

	public string GetOptionsText(out int count)
	{
		var text = "";
		count = 0;
		for (var i = 0; i < ListDialogueOptions.Count; i++)
		{
			text += ListDialogueOptions[i].Text;
			text += "\n";
			count++;
		}

		return text;
	}

	public bool EntryConditionsSatisfied()
	{
		var result = true;
		if (ListEntryCondition.Count == 0)
		{
			return false;
		}

		var sharedInstance = DialogueConditionManager.SharedInstance;
		for (var i = 0; i < ListEntryCondition.Count; i++)
		{
			var text = ListEntryCondition[i];
			if (sharedInstance.ConditionExists(text))
			{
				if (!sharedInstance.GetConditionState(text))
				{
					result = false;
				}
			}
			else if (!PlayerData.PersistentConditionExists(text))
			{
				if (!PlayerData.GetPersistentCondition(text))
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}

		return result;
	}

	public bool TargetNodeConditionsSatisfied()
	{
		var result = true;
		if (ListTargetCondition.Count == 0)
		{
			return result;
		}

		for (var i = 0; i < ListTargetCondition.Count; i++)
		{
			var id = ListTargetCondition[i];
			if (!Locator.GetShipLogManager().IsFactRevealed(id))
			{
				result = false;
			}
		}

		return result;
	}

	public void SetNodeCompleted()
	{
		CurrentPage = -1;
		var sharedInstance = DialogueConditionManager.SharedInstance;
		for (var i = 0; i < ConditionsToSet.Count; i++)
		{
			sharedInstance.SetConditionState(ConditionsToSet[i], true);
		}

		if (PersistentConditionToSet != string.Empty)
		{
			PlayerData.SetPersistentCondition(PersistentConditionToSet, true);
			sharedInstance.SetConditionState(PersistentConditionToSet, true);
		}

		if (PersistentConditionToDisable != string.Empty)
		{
			PlayerData.SetPersistentCondition(PersistentConditionToDisable, false);
			sharedInstance.SetConditionState(PersistentConditionToDisable);
		}

		for (var j = 0; j < DBEntriesToSet.Length; j++)
		{
			Locator.GetShipLogManager().RevealFact(DBEntriesToSet[j]);
		}
	}
}