using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Animation.NPC.WorldObjects
{
	abstract class NpcAnimController<T> : WorldObject<T>, INpcAnimController
		where T : MonoBehaviour
	{
		public override void Init(T controller, int id)
		{
			ObjectId = id;
			AttachedObject = controller;
			DebugLog.DebugWrite($"init : {AttachedObject.name}");
		}

		public abstract CharacterDialogueTree GetDialogueTree();

		public virtual void StartConversation()
		{
			DebugLog.DebugWrite($"REMOTE start conversation : {AttachedObject.name}");
			QSBWorldSync.RaiseEvent(GetDialogueTree(), "OnStartConversation");
		}

		public virtual void EndConversation()
		{
			DebugLog.DebugWrite($"REMOTE end conversation : {AttachedObject.name}");
			QSBWorldSync.RaiseEvent(GetDialogueTree(), "OnEndConversation");
		}
	}
}
