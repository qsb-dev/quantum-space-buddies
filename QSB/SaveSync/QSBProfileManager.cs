using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.SaveSync;

public static class QSBProfileManager
{
	public static List<QSBProfileData> _profiles = new();
	public static QSBProfileData _currentProfile;
	public static QSBProfileData mostRecentProfile
		=> Enumerable.FirstOrDefault(Enumerable.OrderByDescending(_profiles, (QSBProfileData profile) => profile.lastModifiedTime));
}
