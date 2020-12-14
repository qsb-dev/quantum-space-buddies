using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.DeathSync
{
	public static class Necronomicon
	{
		private static readonly Dictionary<DeathType, string[]> DeathDictionary = new Dictionary<DeathType, string[]>
		{
			{ DeathType.Default, new[]
			{
				"{0} died"
			} },
			{ DeathType.Impact, new[]
			{
				"{0} forgot to use retro-rockets",
				"{0} bonked into the ground too hard",
				"{0} went splat"
			} },
			{ DeathType.Asphyxiation, new[]
			{
				"{0} forgot to breathe",
				"{0} forgot how to breathe",
				"{0} forgot to check their oxygen meter",
				"{0} lacked oxygen",
				"{0} didn't need air anyway"
			} },
			{ DeathType.Energy, new[]
			{
				"{0} was cooked",
				"{0} failed the Hotshot achievement",
				"{0} forgot to install an AC unit",
				"{0} got too hot"
			} },
			{ DeathType.Supernova, new[]
			{
				"{0} ran out of time",
				"{0} got vaporized",
				"{0} lost track of time",
				"{0} got front row seats to the supernova",
				"{0} heard the End of Times music",
				"{0} watched the sun go kaboom",
				"{0} became cosmic marshmallow",
				"{0} photosynthesized too much"
			} },
			{ DeathType.Digestion, new[]
			{
				"{0} was eaten",
				"{0} found a fish",
				"{0} encountered an evil creature",
				"{0} followed the light, then was followed by it",
				"{0} messed with the wrong species of fish"
			} },
			{ DeathType.BigBang, new[]
			{
				"{0} sacrificed themself for the universe",
				"{0} knows the true meaning of sacrifice",
				"{0} won at the cost of their life"
			} },
			{ DeathType.Crushed, new[]
			{
				"{0} went through the tunnel too slow",
				"{0} didn't make it out in time",
				"{0} was squished",
				"{0} thought the Sunless City was safe",
				"{0} was buried"
			} },
			{ DeathType.Meditation, new[]
			{
				"{0} took a deep breath and died",
				"{0} fell asleep",
				"{0} got killed by Gabbro's advice"
			} },
			{ DeathType.TimeLoop, new[]
			{
				"{0} ran out of time",
				"{0} was caught by a statue",
				"{0}'s memories were pilfered",
				"{0}'s memories fell into a black hole",
				"{0}'s universe was eaten by Grobletombus"
			} },
			{ DeathType.Lava, new[]
			{
				"{0} tried to swim in lava",
				"{0} didn't know what the glowy orange liquid was",
				"{0} slipped in lava",
				"{0} became one with the glowing gooey rock"
			} },
			{ DeathType.BlackHole, new[]
			{
				"{0} should visit the Ash Twin Project again",
				"{0} waited inside the Ash Twin Project",
				"{0} chased their memories"
			} }
		};

		public static string GetPhrase(DeathType deathType) => 
            DeathDictionary[deathType].OrderBy(x => Guid.NewGuid()).First();
    }
}