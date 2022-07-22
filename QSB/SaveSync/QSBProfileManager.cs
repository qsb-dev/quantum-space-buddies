using QSB.Utility;
using System.Collections.Generic;

namespace QSB.SaveSync;

public static class QSBProfileManager
{
	public static readonly List<QSBProfileData> _profiles = new();
	public static QSBProfileData _currentProfile;
	public static QSBProfileData mostRecentProfile
		=> _profiles.MaxBy(x => x.lastModifiedTime);
}
