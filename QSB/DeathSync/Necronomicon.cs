using QSB.Localisation;
using System;
using System.Collections.Generic;

namespace QSB.DeathSync;

public static class Necronomicon
{
	public static string GetPhrase(DeathType deathType, int index)
		=> QSBLocalisation.Current.DeathMessages.ContainsKey(deathType)
			? QSBLocalisation.Current.DeathMessages[deathType][index]
			: null;

	public static int GetRandomIndex(DeathType deathType)
		=> QSBLocalisation.Current.DeathMessages.ContainsKey(deathType)
			? new Random().Next(0, QSBLocalisation.Current.DeathMessages[deathType].Length)
			: -1;
}