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
            DebugLog.DebugWrite("START CONVO " + index);
        }

        public static bool InputDialogueOption(int optionIndex, DialogueBoxVer2 ____currentDialogueBox)
        {
            if (optionIndex < 0)
            {
                return true;
            }

            var selectedOption = ____currentDialogueBox.OptionFromUIIndex(optionIndex);
            ConversationManager.Instance.SendPlayerOption(selectedOption.Text);
            return true;
        }

        public static void GetNextPage(DialogueNode __instance, string ____name, List<string> ____listPagesToDisplay, int ____currentPage)
        {
            DebugLog.DebugWrite("Name is : " + __instance.Name);
            DebugLog.DebugWrite("Target Name is : " + __instance.TargetName);
            var key = ____name + ____listPagesToDisplay[____currentPage];
            var mainText = TextTranslation.Translate(key).Trim();
            ConversationManager.Instance.SendCharacterDialogue(0, mainText);
        }

        public static void AddPatches()
        {
            QSB.Helper.HarmonyHelper.AddPostfix<DialogueNode>("GetNextPage", typeof(ConversationPatches), nameof(GetNextPage));
            QSB.Helper.HarmonyHelper.AddPrefix<CharacterDialogueTree>("InputDialogueOption", typeof(ConversationPatches), nameof(InputDialogueOption));
            QSB.Helper.HarmonyHelper.AddPostfix<CharacterDialogueTree>("StartConversation", typeof(ConversationPatches), nameof(StartConversation));
        }
    }
}
