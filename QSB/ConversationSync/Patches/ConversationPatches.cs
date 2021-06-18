using OWML.Common;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.ConversationSync.Patches
{
	public class ConversationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Postfix(nameof(DialogueNode_GetNextPage));
			Prefix(nameof(CharacterDialogueTree_InputDialogueOption));
			Prefix(nameof(CharacterDialogueTree_StartConversation));
			Prefix(nameof(CharacterDialogueTree_EndConversation));
		}

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

		public static void DialogueNode_GetNextPage(string ____name, List<string> ____listPagesToDisplay, int ____currentPage)
		{
			var key = ____name + ____listPagesToDisplay[____currentPage];
			// Sending key so translation can be done on client side - should make different language-d clients compatible
			QSBCore.UnityEvents.RunWhen(() => QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId != -1,
				() => ConversationManager.Instance.SendCharacterDialogue(QSBPlayerManager.LocalPlayer.CurrentCharacterDialogueTreeId, key));
		}
	}
}