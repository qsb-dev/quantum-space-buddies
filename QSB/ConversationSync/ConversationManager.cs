using OWML.Common;
using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.ConversationSync
{
    public class ConversationManager : MonoBehaviour
    {
        public static ConversationManager Instance { get; private set; }
        public AssetBundle ConversationAssetBundle { get; private set; }
        private GameObject BoxPrefab;
        public Dictionary<CharacterDialogueTree, GameObject> BoxMappings = new Dictionary<CharacterDialogueTree, GameObject>();

        private void Start()
        {
            Instance = this;

            ConversationAssetBundle = QSB.Helper.Assets.LoadBundle("assets/conversation");
            DebugLog.LogState("ConversationBundle", ConversationAssetBundle);

            BoxPrefab = ConversationAssetBundle.LoadAsset<GameObject>("assets/dialoguebubble.prefab");
            var font = (Font)Resources.Load(@"fonts\english - latin\spacemono-bold");
            if (font == null)
            {
                DebugLog.ToConsole("Error - Font is null!", MessageType.Error);
            }
            BoxPrefab.GetComponent<Text>().font = font;
            DebugLog.LogState("BoxPrefab", BoxPrefab);
        }

        public void SendPlayerOption(string text)
        {
            GlobalMessenger<uint, string, ConversationType>.FireEvent(EventNames.QSBConversation, PlayerRegistry.LocalPlayerId, text, ConversationType.Player);
        }

        public void SendCharacterDialogue(int id, string text)
        {
            GlobalMessenger<uint, string, ConversationType>.FireEvent(EventNames.QSBConversation, (uint)id, text, ConversationType.Character);
        }

        public void CloseBoxPlayer()
        {
            GlobalMessenger<uint, string, ConversationType>.FireEvent(EventNames.QSBConversation, PlayerRegistry.LocalPlayerId, "", ConversationType.EndPlayer);
        }

        public void CloseBoxCharacter(int id)
        {
            GlobalMessenger<uint, string, ConversationType>.FireEvent(EventNames.QSBConversation, (uint)id, "", ConversationType.EndCharacter);
        }

        public void DisplayPlayerConversationBox(uint playerId, string text)
        {
            Destroy(PlayerRegistry.GetPlayer(playerId).CurrentDialogueBox);
            if (playerId == PlayerRegistry.LocalPlayerId)
            {
                DebugLog.ToConsole("Error - Cannot display conversation box for local player!", MessageType.Error);
                return;
            }
            var newBox = Instantiate(BoxPrefab);
            newBox.SetActive(false);
            newBox.transform.parent = PlayerRegistry.GetPlayer(playerId).Body.transform;
            newBox.transform.localPosition = new Vector3(0, 25, 0);
            newBox.transform.rotation = PlayerRegistry.GetPlayer(playerId).Body.transform.rotation;
            var lookAt = newBox.AddComponent<FaceActiveCamera>();
            lookAt.SetValue("_useLookAt", false);
            lookAt.SetValue("_localFacingVector", Vector3.back);
            lookAt.SetValue("_localRotationAxis", Vector3.up);
            newBox.GetComponent<Text>().text = text;
            newBox.SetActive(true);

            PlayerRegistry.GetPlayer(playerId).CurrentDialogueBox = newBox;
        }

        public void DisplayCharacterConversationBox(int index, string text)
        {
            var oldDialogueTree = WorldRegistry.OldDialogueTrees[index];
            if (BoxMappings.ContainsKey(oldDialogueTree))
            {
                Destroy(BoxMappings[oldDialogueTree]);
                BoxMappings.Remove(oldDialogueTree);
            }

            var newBox = Instantiate(BoxPrefab);
            newBox.SetActive(false);
            newBox.transform.parent = oldDialogueTree.gameObject.transform;
            newBox.transform.localPosition = new Vector3(0, 2, 0);
            newBox.transform.rotation = oldDialogueTree.gameObject.transform.rotation;
            var lookAt = newBox.AddComponent<FaceActiveCamera>();
            lookAt.SetValue("_useLookAt", false);
            lookAt.SetValue("_localFacingVector", Vector3.back);
            lookAt.SetValue("_localRotationAxis", Vector3.up);
            newBox.GetComponent<Text>().text = text;
            newBox.SetActive(true);

            BoxMappings.Add(oldDialogueTree, newBox);
        }
    }
}
