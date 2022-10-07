using HarmonyLib;
using OWML.Common;
using QSB.ConversationSync.Messages;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ConversationSync.Patches;

[HarmonyPatch]
public class ConversationPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DialogueConditionManager), nameof(DialogueConditionManager.SetConditionState))]
	public static void SetConditionState(string conditionName, bool conditionState) =>
		new DialogueConditionMessage(conditionName, conditionState).Send();

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.StartConversation))]
	public static void CharacterDialogueTree_StartConversation(CharacterDialogueTree __instance)
	{
		var worldObject = __instance.GetWorldObject<QSBCharacterDialogueTree>();

		QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree = worldObject;
		ConversationManager.Instance.SendConvState(worldObject, true);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.EndConversation))]
	public static bool CharacterDialogueTree_EndConversation(CharacterDialogueTree __instance)
	{
		if (!__instance.enabled)
		{
			return false;
		}

		if (QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree == null)
		{
			DebugLog.ToConsole($"Warning - Ending conversation with null CurrentCharacterDialogueTree! Called from {__instance.name}", MessageType.Warning);
			return true;
		}

		ConversationManager.Instance.SendConvState(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree, false);
		ConversationManager.Instance.CloseBoxCharacter(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree.ObjectId);
		QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree = null;
		ConversationManager.Instance.CloseBoxPlayer();
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.InputDialogueOption))]
	public static bool CharacterDialogueTree_InputDialogueOption(CharacterDialogueTree __instance, int optionIndex)
	{
		if (optionIndex < 0)
		{
			// in a page where there is no selectable options
			ConversationManager.Instance.CloseBoxPlayer();
			return true;
		}

		var selectedOption = __instance._currentDialogueBox.OptionFromUIIndex(optionIndex);
		// BUG: uses translated value instead of key
		ConversationManager.Instance.SendPlayerOption(selectedOption.Text);
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.GetNextPage))]
	public static void DialogueNode_GetNextPage(DialogueNode __instance)
	{
		var key = __instance._name + __instance._listPagesToDisplay[__instance._currentPage];
		// Sending key so translation can be done on client side - should make different language-d clients compatible
		Delay.RunWhen(() => QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree != null,
			() => ConversationManager.Instance.SendCharacterDialogue(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTree.ObjectId, key));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RemoteDialogueTrigger), nameof(RemoteDialogueTrigger.ConversationTriggered))]
	public static bool ConversationTriggeredReplacement(RemoteDialogueTrigger __instance, out bool __result, out RemoteDialogueTrigger.RemoteDialogueCondition dialogue)
	{
		dialogue = default;
		var dialogueIndex = -1;
		for (var i = 0; i < __instance._listDialogues.Length; i++)
		{
			if (!__instance._activatedDialogues[i])
			{
				var allConditionsMet = true;
				var anyConditionsMet = __instance._listDialogues[i].prereqConditions.Length == 0;

				foreach (var prereqCondition in __instance._listDialogues[i].prereqConditions)
				{
					if (DialogueConditionManager.SharedInstance.GetConditionState(prereqCondition))
					{
						anyConditionsMet = true;
					}
					else
					{
						allConditionsMet = false;
					}
				}

				var conditionsMet = false;
				var prereqConditionType = __instance._listDialogues[i].prereqConditionType;
				if (prereqConditionType != RemoteDialogueTrigger.MultiConditionType.OR)
				{
					if (prereqConditionType == RemoteDialogueTrigger.MultiConditionType.AND && allConditionsMet)
					{
						conditionsMet = true;
					}
				}
				else if (anyConditionsMet)
				{
					conditionsMet = true;
				}

				if (conditionsMet && __instance._listDialogues[i].priority < int.MaxValue)
				{
					dialogue = __instance._listDialogues[i];
					dialogueIndex = i;
				}
			}
		}

		if (dialogueIndex == -1)
		{
			__result = false;
			return false;
		}

		__instance._activatedDialogues[dialogueIndex] = true;
		__result = true;

		__instance.GetWorldObject<QSBRemoteDialogueTrigger>()
			.SendMessage(new EnterRemoteDialogueMessage(dialogueIndex));

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameSave), nameof(GameSave.SetPersistentCondition))]
	public static void SetPersistentCondition(string condition, bool state) =>
		new PersistentConditionMessage(condition, state).Send();

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DialogueConditionManager), nameof(DialogueConditionManager.AddCondition))]
	public static void AddCondition(string conditionName, bool conditionState) =>
		new DialogueConditionMessage(conditionName, conditionState).Send();
}