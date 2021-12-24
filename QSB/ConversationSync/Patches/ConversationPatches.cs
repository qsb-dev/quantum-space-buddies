using HarmonyLib;
using OWML.Common;
using QSB.ConversationSync.Messages;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.ConversationSync.Patches
{
	[HarmonyPatch]
	public class ConversationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.StartConversation))]
		public static void CharacterDialogueTree_StartConversation(CharacterDialogueTree __instance)
		{
			var index = QSBWorldSync.OldDialogueTrees.FindIndex(x => x == __instance);
			if (index == -1)
			{
				DebugLog.ToConsole($"Warning - Index for tree {__instance.name} was -1.", MessageType.Warning);
			}

			QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId = index;
			ConversationManager.Instance.SendConvState(index, true);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.EndConversation))]
		public static bool CharacterDialogueTree_EndConversation(CharacterDialogueTree __instance)
		{
			if (!__instance.enabled)
			{
				return false;
			}

			if (QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId == -1)
			{
				DebugLog.ToConsole($"Warning - Ending conversation with CurrentDialogueId of -1! Called from {__instance.name}", MessageType.Warning);
				return true;
			}

			ConversationManager.Instance.SendConvState(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId, false);
			ConversationManager.Instance.CloseBoxCharacter(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId);
			QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId = -1;
			ConversationManager.Instance.CloseBoxPlayer();
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.InputDialogueOption))]
		public static bool CharacterDialogueTree_InputDialogueOption(int optionIndex, DialogueBoxVer2 ____currentDialogueBox)
		{
			if (optionIndex < 0)
			{
				// in a page where there is no selectable options
				ConversationManager.Instance.CloseBoxPlayer();
				return true;
			}

			var selectedOption = ____currentDialogueBox.OptionFromUIIndex(optionIndex);
			ConversationManager.Instance.SendPlayerOption(selectedOption.Text);
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.GetNextPage))]
		public static void DialogueNode_GetNextPage(string ____name, List<string> ____listPagesToDisplay, int ____currentPage)
		{
			var key = ____name + ____listPagesToDisplay[____currentPage];
			// Sending key so translation can be done on client side - should make different language-d clients compatible
			QSBCore.UnityEvents.RunWhen(() => QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId != -1,
				() => ConversationManager.Instance.SendCharacterDialogue(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId, key));
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(RemoteDialogueTrigger), nameof(RemoteDialogueTrigger.ConversationTriggered))]
		public static bool ConversationTriggeredReplacement(RemoteDialogueTrigger __instance, ref bool __result, out RemoteDialogueTrigger.RemoteDialogueCondition dialogue)
		{
			dialogue = default;
			var maxValue = int.MaxValue;
			var num = -1;
			var sharedInstance = DialogueConditionManager.SharedInstance;
			for (var i = 0; i < __instance._listDialogues.Length; i++)
			{
				if (!__instance._activatedDialogues[i])
				{
					var flag = true;
					var flag2 = false;
					if (__instance._listDialogues[i].prereqConditions.Length == 0)
					{
						flag2 = true;
					}

					for (int j = 0; j < __instance._listDialogues[i].prereqConditions.Length; j++)
					{
						if (sharedInstance.GetConditionState(__instance._listDialogues[i].prereqConditions[j]))
						{
							flag2 = true;
						}
						else
						{
							flag = false;
						}
					}

					bool flag3 = false;
					RemoteDialogueTrigger.MultiConditionType prereqConditionType = __instance._listDialogues[i].prereqConditionType;
					if (prereqConditionType != RemoteDialogueTrigger.MultiConditionType.OR)
					{
						if (prereqConditionType == RemoteDialogueTrigger.MultiConditionType.AND && flag)
						{
							flag3 = true;
						}
					}
					else if (flag2)
					{
						flag3 = true;
					}

					if (flag3 && __instance._listDialogues[i].priority < maxValue)
					{
						dialogue = __instance._listDialogues[i];
						num = i;
					}
				}
			}

			if (num == -1)
			{
				__result = false;
				return false;
			}

			__instance._activatedDialogues[num] = true;

			QSBWorldSync.GetWorldFromUnity<QSBRemoteDialogueTrigger>(__instance)
				.SendMessage(new EnterRemoteDialogueMessage(num, __instance._listDialogues.IndexOf(dialogue)));

			__result = true;
			return false;
		}
	}
}