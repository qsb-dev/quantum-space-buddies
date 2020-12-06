using OWML.Common;
using QSB.OrbSync;
using QSB.TransformSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QSB.WorldSync
{
	public static class WorldRegistry
	{
		private static readonly List<WorldObject> WorldObjects = new List<WorldObject>();
		public static List<NomaiOrbTransformSync> OrbSyncList = new List<NomaiOrbTransformSync>();
		public static List<NomaiInterfaceOrb> OldOrbList = new List<NomaiInterfaceOrb>();
		public static List<CharacterDialogueTree> OldDialogueTrees = new List<CharacterDialogueTree>();

		public static void AddObject(WorldObject worldObject)
		{
			if (WorldObjects.Contains(worldObject))
			{
				return;
			}
			WorldObjects.Add(worldObject);
		}

		public static IEnumerable<T> GetObjects<T>()
		{
			return WorldObjects.OfType<T>();
		}

		public static T GetObject<T>(int id) where T : WorldObject
		{
			return GetObjects<T>().FirstOrDefault(x => x.ObjectId == id);
		}

		public static void RemoveObjects<T>() where T : WorldObject
		{
			WorldObjects.RemoveAll(x => x.GetType() == typeof(T));
		}

		public static void HandleSlotStateChange(NomaiInterfaceSlot slot, NomaiInterfaceOrb affectingOrb, bool state)
		{
			QSBOrbSlot qsbSlot = null;
			NomaiOrbTransformSync orbSync = null;
			var slotList = GetObjects<QSBOrbSlot>();
			if (slotList.Count() == 0)
			{
				return;
			}
			try
			{
				qsbSlot = slotList.First(x => x.InterfaceSlot == slot);
				orbSync = OrbSyncList.First(x => x.AttachedOrb == affectingOrb);
				if (orbSync.HasAuthority)
				{
					qsbSlot.HandleEvent(state);
				}
			}
			catch
			{
				DebugLog.DebugWrite("Error - Exception when handling slot state change."
					+ Environment.NewLine + $"Slot name {slot.name} to {state}"
					+ Environment.NewLine + $"SlotList count : {slotList?.Count()}"
					+ Environment.NewLine + $"QSBOrbSlot null? : {qsbSlot == null}"
					+ Environment.NewLine + $"NomaiOrbTransformSync null? : {orbSync == null}", MessageType.Error);
			}
		}

		public static void RaiseEvent(object instance, string eventName)
		{
			if (!(instance.GetType()
				.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)?
				.GetValue(instance) is MulticastDelegate multiDelegate))
			{
				return;
			}
			var delegateList = multiDelegate.GetInvocationList().ToList();
			foreach (var del in delegateList)
			{
				del.DynamicInvoke(instance);
			}
		}
	}
}