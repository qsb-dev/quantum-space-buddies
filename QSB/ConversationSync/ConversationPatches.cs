using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.ConversationSync
{
    public static class ConversationPatches
    {
        public static void StartConversation(CharacterDialogueTree __instance)
        {
            var index = WorldRegistry.OldDialogueTrees.FindIndex(x => x == __instance);
            PlayerRegistry.LocalPlayer.CurrentDialogueID = index;
            DebugLog.DebugWrite($"Start converstation id {index}");
        }

        public static void EndConversation()
        {
            PlayerRegistry.LocalPlayer.CurrentDialogueID = -1;
        }

        public static bool InputDialogueOption(int optionIndex, DialogueBoxVer2 ____currentDialogueBox)
        {
            if (optionIndex < 0)
            {
                // in a page where there is no selectable options
                return true;
            }

            var selectedOption = ____currentDialogueBox.OptionFromUIIndex(optionIndex);
            ConversationManager.Instance.SendPlayerOption(selectedOption.Text);
            return true;
        }

        public static void GetNextPage(string ____name, List<string> ____listPagesToDisplay, int ____currentPage)
        {
            var key = ____name + ____listPagesToDisplay[____currentPage];
            // Sending key so translation can be done on client side - should make different language-d clients compatible
            QSB.Helper.Events.Unity.RunWhen(() => PlayerRegistry.LocalPlayer.CurrentDialogueID != -1,
                () => ConversationManager.Instance.SendCharacterDialogue(PlayerRegistry.LocalPlayer.CurrentDialogueID, key));
        }

        public static void AddPatches()
        {
            QSB.Helper.HarmonyHelper.AddPostfix<DialogueNode>("GetNextPage", typeof(ConversationPatches), nameof(GetNextPage));
            QSB.Helper.HarmonyHelper.AddPrefix<CharacterDialogueTree>("InputDialogueOption", typeof(ConversationPatches), nameof(InputDialogueOption));
            QSB.Helper.HarmonyHelper.AddPostfix<CharacterDialogueTree>("StartConversation", typeof(ConversationPatches), nameof(StartConversation));
            QSB.Helper.HarmonyHelper.AddPostfix<CharacterDialogueTree>("EndConversation", typeof(ConversationPatches), nameof(EndConversation));
        }
    }
}
