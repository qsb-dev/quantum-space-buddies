using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;


namespace QSB.ConversationSync.WorldObjects;

public class QSBCharacterDialogueTree : WorldObject<CharacterDialogueTree>
{
	public string OverridenCurentNodeName { get; private set; }
	//No need to send who is talking to this tree, as  ConversationManager.Instance.GetPlayerTalkingToTree already syncs that (hopefully)
	//Things needed to be synced:
	//1 - Is it talking to someone? (bool) (the who, again, is hopefully handled by ConversationManager.Instance.GetPlayerTalkingToTree)
	//2 - Where in the tree is the conversation? (tree node or a way to find the tree node, _currentNode (TargetName)) (includes _listOptionNodes)

	public override void SendInitialState(uint to) 
	{
		string currentNodeName = string.Empty;
		if(AttachedObject._currentNode != null)
        {
			currentNodeName = AttachedObject._currentNode.Name;
		}
		this.SendMessage(new CharacterDialogueTreeMessage(currentNodeName) { To = to });
	}

	public void SetInConversation(string currentNodeName) 
	{
        if (!string.IsNullOrEmpty(currentNodeName)) 
		{
			OverridenCurentNodeName = currentNodeName;
			AttachedObject.StartConversation();
			OverridenCurentNodeName = string.Empty;//So that it only overrides the dialogue tree once
		}
	}
}
