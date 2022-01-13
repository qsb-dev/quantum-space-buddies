using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;

namespace QSB.TriggerSync
{
	public class TriggerManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		private static TriggerLink[] _triggerLinks;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			_triggerLinks?.ForEach(x => x.Dispose());
			_triggerLinks = QSBWorldSync.GetUnityObjects<OWTriggerVolume>()
				.Select((x, i) => new TriggerLink(i, x))
				.ToArray();
		}

		public static TriggerLink GetTriggerLink(int id)
		{
			if (!_triggerLinks.TryGet(id, out var triggerLink))
			{
				DebugLog.ToConsole($"no trigger link with id {id}", MessageType.Error);
			}

			return triggerLink;
		}
	}
}
