using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.GalaxyMap;

public class CustomDialogueTree : MonoBehaviour
{
	public TextAsset _xmlCharacterDialogueAsset;
	public Transform _attentionPoint;
	public Vector3 _attentionPointOffset = Vector3.zero;
	public bool _turnOffFlashlight = true;
	public bool _turnOnFlashlight = true;

	private SingleInteractionVolume _interactVolume;
	private string _characterName;
	private bool _initialized;
	private IDictionary<string, QSBDialogueNode> _mapDialogueNodes;
	private List<DialogueOption> _listOptionNodes;
	private QSBDialogueNode _currentNode;
	private DialogueBoxVer2 _currentDialogueBox;
	private bool _wasFlashlightOn;
	private bool _timeFrozen;

	private const string SIGN_NAME = "SIGN";
	private const string RECORDING_NAME = "RECORDING";
	private const float MINIMUM_TIME_TEXT_VISIBLE = 0.1f;

	private void Awake()
	{
		_attentionPoint ??= transform;
		_initialized = false;
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		GlobalMessenger.AddListener("DialogueConditionsReset", new Callback(OnDialogueConditionsReset));
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", new Callback<DeathType>(OnPlayerDeath));
		enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("DialogueConditionsReset", new Callback(OnDialogueConditionsReset));
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", new Callback<DeathType>(OnPlayerDeath));
	}

	public void LateInitialize()
	{
		_mapDialogueNodes = new Dictionary<string, QSBDialogueNode>(ComparerLibrary.stringEqComparer);
		_listOptionNodes = new List<DialogueOption>();
		_initialized = LoadXml();
	}

	public void VerifyInitialized()
	{
		if (!_initialized)
		{
			LateInitialize();
		}
	}

	public void SetTextXml(TextAsset textAsset)
	{
		_xmlCharacterDialogueAsset = textAsset;
		if (_initialized)
		{
			LoadXml();
		}
	}

	public bool InConversation() => enabled;

	public InteractVolume GetInteractVolume() => _interactVolume;

	private void Update()
	{
		if (_currentDialogueBox != null && OWInput.GetInputMode() == InputMode.Dialogue)
		{
			if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2))
			{
				if (!_currentDialogueBox.AreTextEffectsComplete())
				{
					_currentDialogueBox.FinishAllTextEffects();
					return;
				}

				if (_currentDialogueBox.TimeCompletelyRevealed() < 0.1f)
				{
					return;
				}

				var selectedOption = _currentDialogueBox.GetSelectedOption();
				if (!InputDialogueOption(selectedOption))
				{
					EndConversation();
					return;
				}
			}
			else
			{
				if (OWInput.IsNewlyPressed(InputLibrary.down) || OWInput.IsNewlyPressed(InputLibrary.down2))
				{
					_currentDialogueBox.OnDownPressed();
					return;
				}

				if (OWInput.IsNewlyPressed(InputLibrary.up) || OWInput.IsNewlyPressed(InputLibrary.up2))
				{
					_currentDialogueBox.OnUpPressed();
				}
			}
		}
	}

	private void SetEntryNode()
	{
		var list = new List<QSBDialogueNode>();
		list.AddRange(_mapDialogueNodes.Values);
		for (var i = list.Count - 1; i >= 0; i--)
		{
			if (list[i].EntryConditionsSatisfied())
			{
				_currentNode = list[i];
				return;
			}
		}

		Debug.LogWarning("CharacterDialogueTree " + _characterName + " no EntryConditions satisfied. Did you forget to add <EntryCondition>DEFAULT</EntryCondition> to a node?");
	}

	private bool LoadXml()
	{
		try
		{
			_mapDialogueNodes.Clear();
			_listOptionNodes.Clear();
			var sharedInstance = DialogueConditionManager.SharedInstance;
			var xelement = XDocument.Parse(OWUtilities.RemoveByteOrderMark(_xmlCharacterDialogueAsset)).Element("DialogueTree");
			var xelement2 = xelement.Element("NameField");
			if (xelement2 == null)
			{
				_characterName = "";
			}
			else
			{
				_characterName = xelement2.Value;
			}

			foreach (var xelement3 in xelement.Elements("DialogueNode"))
			{
				var dialogueNode = new QSBDialogueNode();
				if (xelement3.Element("Name") != null)
				{
					dialogueNode.Name = xelement3.Element("Name").Value;
				}
				else
				{
					Debug.LogWarning("Missing name on a Node");
				}

				var randomize = xelement3.Element("Randomize") != null;
				dialogueNode.DisplayTextData = new DialogueText(xelement3.Elements("Dialogue"), randomize);
				var xelement4 = xelement3.Element("DialogueTarget");
				if (xelement4 != null)
				{
					dialogueNode.TargetName = xelement4.Value;
				}

				var enumerable = xelement3.Elements("EntryCondition");
				if (enumerable != null)
				{
					foreach (var xelement5 in enumerable)
					{
						dialogueNode.ListEntryCondition.Add(xelement5.Value);
						if (!sharedInstance.ConditionExists(xelement5.Value))
						{
							sharedInstance.AddCondition(xelement5.Value);
						}
					}
				}

				var enumerable2 = xelement3.Elements("DialogueTargetShipLogCondition");
				if (enumerable2 != null)
				{
					foreach (var xelement6 in enumerable2)
					{
						dialogueNode.ListTargetCondition.Add(xelement6.Value);
					}
				}

				foreach (var xelement7 in xelement3.Elements("SetCondition"))
				{
					dialogueNode.ConditionsToSet.Add(xelement7.Value);
				}

				var xelement8 = xelement3.Element("SetPersistentCondition");
				if (xelement8 != null)
				{
					dialogueNode.PersistentConditionToSet = xelement8.Value;
				}

				var xelement9 = xelement3.Element("DisablePersistentCondition");
				if (xelement9 != null)
				{
					dialogueNode.PersistentConditionToDisable = xelement9.Value;
				}

				var xelement10 = xelement3.Element("RevealFacts");
				if (xelement10 != null)
				{
					var enumerable3 = xelement10.Elements("FactID");
					if (enumerable3 != null)
					{
						var list = new List<string>();
						foreach (var xelement11 in enumerable3)
						{
							list.Add(xelement11.Value);
						}

						dialogueNode.DBEntriesToSet = new string[list.Count];
						for (var i = 0; i < list.Count; i++)
						{
							dialogueNode.DBEntriesToSet[i] = list[i];
						}
					}
				}

				var list2 = new List<DialogueOption>();
				var xelement12 = xelement3.Element("DialogueOptionsList");
				if (xelement12 != null)
				{
					var enumerable4 = xelement12.Elements("DialogueOption");
					if (enumerable4 != null)
					{
						foreach (var xelement13 in enumerable4)
						{
							var dialogueOption = new DialogueOption
							{
								Text = OWUtilities.CleanupXmlText(xelement13.Element("Text").Value, false)
							};
							dialogueOption.SetNodeId(dialogueNode.Name, _characterName);
							var xelement14 = xelement13.Element("DialogueTarget");
							if (xelement14 != null)
							{
								dialogueOption.TargetName = xelement14.Value;
							}

							if (xelement13.Element("RequiredCondition") != null)
							{
								dialogueOption.ConditionRequirement = xelement13.Element("RequiredCondition").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.ConditionRequirement))
								{
									sharedInstance.AddCondition(dialogueOption.ConditionRequirement);
								}
							}

							foreach (var xelement15 in xelement13.Elements("RequiredPersistentCondition"))
							{
								dialogueOption.PersistentCondition.Add(xelement15.Value);
							}

							if (xelement13.Element("CancelledCondition") != null)
							{
								dialogueOption.CancelledRequirement = xelement13.Element("CancelledCondition").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.CancelledRequirement))
								{
									sharedInstance.AddCondition(dialogueOption.CancelledRequirement);
								}
							}

							foreach (var xelement16 in xelement13.Elements("CancelledPersistentCondition"))
							{
								dialogueOption.CancelledPersistentRequirement.Add(xelement16.Value);
							}

							foreach (var xelement17 in xelement13.Elements("RequiredLogCondition"))
							{
								dialogueOption.LogConditionRequirement.Add(xelement17.Value);
							}

							if (xelement13.Element("ConditionToSet") != null)
							{
								dialogueOption.ConditionToSet = xelement13.Element("ConditionToSet").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.ConditionToSet))
								{
									sharedInstance.AddCondition(dialogueOption.ConditionToSet);
								}
							}

							if (xelement13.Element("ConditionToCancel") != null)
							{
								dialogueOption.ConditionToCancel = xelement13.Element("ConditionToCancel").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.ConditionToCancel))
								{
									sharedInstance.AddCondition(dialogueOption.ConditionToCancel);
								}
							}

							list2.Add(dialogueOption);
							_listOptionNodes.Add(dialogueOption);
						}
					}
				}

				dialogueNode.ListDialogueOptions = list2;
				_mapDialogueNodes.Add(dialogueNode.Name, dialogueNode);
			}

			var list3 = new List<QSBDialogueNode>();
			list3.AddRange(_mapDialogueNodes.Values);
			for (var j = 0; j < list3.Count; j++)
			{
				var dialogueNode2 = list3[j];
				var targetName = dialogueNode2.TargetName;
				if (targetName != "")
				{
					if (_mapDialogueNodes.ContainsKey(targetName))
					{
						dialogueNode2.Target = _mapDialogueNodes[targetName];
					}
					else
					{
						Debug.LogError("Target Node: " + targetName + " does not exist", this);
					}
				}
				else
				{
					dialogueNode2.Target = null;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("XML Error in CharacterDialogueTree!\n" + ex.Message, this);
			return false;
		}

		return true;
	}

	public void StartConversation()
	{
		VerifyInitialized();
		enabled = true;
		if (!_timeFrozen && PlayerData.GetFreezeTimeWhileReadingConversations() && !Locator.GetGlobalMusicController().IsEndTimesPlaying())
		{
			_timeFrozen = true;
			OWTime.Pause(OWTime.PauseType.Reading);
		}

		Locator.GetToolModeSwapper().UnequipTool();
		GlobalMessenger.FireEvent("EnterConversation");
		Locator.GetPlayerAudioController().PlayDialogueEnter();
		_wasFlashlightOn = Locator.GetFlashlight().IsFlashlightOn();
		if (_wasFlashlightOn && _turnOffFlashlight)
		{
			Locator.GetFlashlight().TurnOff(false);
		}

		DialogueConditionManager.SharedInstance.ReadPlayerData();
		SetEntryNode();
		_currentDialogueBox = DisplayDialogueBox2();
		if (_attentionPoint != null && !PlayerState.InZeroG())
		{
			Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_attentionPoint, _attentionPointOffset, 2f);
		}

		if (PlayerState.InZeroG() && !_timeFrozen)
		{
			Locator.GetPlayerBody().GetComponent<Autopilot>().StartMatchVelocity(this.GetAttachedOWRigidbody().GetReferenceFrame());
		}
	}

	public void EndConversation()
	{
		if (!enabled)
		{
			return;
		}

		enabled = false;
		if (_timeFrozen)
		{
			_timeFrozen = false;
			OWTime.Unpause(OWTime.PauseType.Reading);
		}

		_interactVolume.ResetInteraction();
		Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
		GlobalMessenger.FireEvent("ExitConversation");
		Locator.GetPlayerAudioController().PlayDialogueExit();
		if (_wasFlashlightOn && _turnOffFlashlight && _turnOnFlashlight)
		{
			Locator.GetFlashlight().TurnOn(false);
		}

		GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>().OnEndDialogue();
		if (_currentNode != null)
		{
			_currentNode.SetNodeCompleted();
		}

		if (PlayerState.InZeroG())
		{
			var component = Locator.GetPlayerBody().GetComponent<Autopilot>();
			if (component.enabled)
			{
				component.Abort();
			}
		}
	}

	private bool InputDialogueOption(int optionIndex)
	{
		var result = true;
		var flag = true;
		if (optionIndex >= 0)
		{
			var selectedOption = _currentDialogueBox.OptionFromUIIndex(optionIndex);
			ContinueToNextNode(selectedOption);
		}
		else if (!_currentNode.HasNext())
		{
			ContinueToNextNode();
		}

		if (_currentNode == null)
		{
			_currentDialogueBox = null;
			flag = false;
			result = false;
		}
		else
		{
			_currentDialogueBox = DisplayDialogueBox2();
		}

		if (flag)
		{
			Locator.GetPlayerAudioController().PlayDialogueAdvance();
		}

		return result;
	}

	private DialogueBoxVer2 DisplayDialogueBox2()
	{
		var richText = "";
		var listOptions = new List<DialogueOption>();
		_currentNode.RefreshConditionsData();
		if (_currentNode.HasNext())
		{
			_currentNode.GetNextPage(out richText, ref listOptions);
		}

		var requiredComponent = GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>();
		requiredComponent.SetVisible(true);
		requiredComponent.SetDialogueText(richText, listOptions);
		if (_characterName == "")
		{
			requiredComponent.SetNameFieldVisible(false);
		}
		else
		{
			requiredComponent.SetNameFieldVisible(true);
			requiredComponent.SetNameField(TextTranslation.Translate(_characterName));
		}

		return requiredComponent;
	}

	private void ContinueToNextNode()
	{
		_currentNode.SetNodeCompleted();
		if (_currentNode.ListTargetCondition.Count > 0 && !_currentNode.TargetNodeConditionsSatisfied())
		{
			_currentNode = null;
			return;
		}

		_currentNode = _currentNode.Target;
	}

	private void ContinueToNextNode(DialogueOption selectedOption)
	{
		_currentNode.SetNodeCompleted();
		if (selectedOption.ConditionToSet != string.Empty)
		{
			DialogueConditionManager.SharedInstance.SetConditionState(selectedOption.ConditionToSet, true);
		}

		if (selectedOption.ConditionToCancel != string.Empty)
		{
			DialogueConditionManager.SharedInstance.SetConditionState(selectedOption.ConditionToCancel);
		}

		if (selectedOption.TargetName == string.Empty)
		{
			_currentNode = null;
			return;
		}

		if (!_mapDialogueNodes.ContainsKey(selectedOption.TargetName))
		{
			Debug.LogError("Cannot find target node " + selectedOption.TargetName);
			Debug.Break();
		}

		_currentNode = _mapDialogueNodes[selectedOption.TargetName];
	}

	private void OnDialogueConditionsReset()
	{
		if (_initialized)
		{
			LoadXml();
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		if (enabled)
		{
			EndConversation();
		}
	}
}