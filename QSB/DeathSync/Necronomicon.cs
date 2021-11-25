using System;
using System.Collections.Generic;

namespace QSB.DeathSync
{
	public static class Necronomicon
	{
		private static readonly Dictionary<DeathType, string[]> Darkhold = new()
		{
			{
				DeathType.Default,
				new[] // Running out of health
			{
				"{0} died",
				"{0} was killed"
			}
			},
			{
				DeathType.Impact,
				new[] // Hitting the ground/wall/object
			{
				"{0} forgot to use retro-rockets",
				"{0} bonked into the ground too hard",
				"{0} hit the ground too hard",
				"{0} went splat",
				"{0} died",
				"{0} was killed",
				"{0} died due to impact",
				"{0} impacted the ground too hard"
			}
			},
			{
				DeathType.Asphyxiation,
				new[] // Running out of oxygen
			{
				"{0} forgot to breathe",
				"{0} asphyxiated",
				"{0} died due to asphyxiation",
				"{0} forgot how to breathe",
				"{0} forgot to check their oxygen",
				"{0} ran out of air",
				"{0} ran out of oxygen",
				"{0} didn't need air anyway"
			}
			},
			{
				DeathType.Energy,
				new[] // Electricity, sun, etc.
			{
				"{0} was cooked",
				"{0} died",
				"{0} was killed"
			}
			},
			{
				DeathType.Supernova,
				new[] // Supernova
			{
				"{0} ran out of time",
				"{0} burnt up",
				"{0} got vaporized",
				"{0} lost track of time",
				"{0} got front row seats to the supernova",
				"{0} heard the music",
				"{0} watched the sun go kaboom",
				"{0} became cosmic marshmallow",
				"{0} photosynthesized too much",
				"{0} died due to the supernova"
			}
			},
			{
				DeathType.Digestion,
				new[] // Anglerfish
			{
				"{0} was eaten",
				"{0} found a fish",
				"{0} encountered an evil creature",
				"{0} messed with the wrong fish",
				"{0} was digested",
				"{0} died due to digestion"
			}
			},
			{
				DeathType.BigBang,
				new[] // End of the game
			{
				// TODO : maybe don't show these?
				"{0} sacrificed themself for the universe",
				"{0} knows the true meaning of sacrifice",
				"{0} won at the cost of their life"
			}
			},
			{
				DeathType.Crushed,
				new[] // Crushed in sand
			{
				"{0} went through the tunnel too slow",
				"{0} didn't make it out in time",
				"{0} was squished",
				"{0} was crushed",
				"{0} was buried",
				"{0} went swimming in the sand",
				"{0} underestimated the danger of sand",
				"{0} died due to being crushed"
			}
			},
			{
				DeathType.TimeLoop,
				new[] // Escaping the supernova
			{
				"{0} ran out of time",
				"{0} lost track of time",
				"{0} watched the sun go kaboom"
			}
			},
			{
				DeathType.Lava,
				new[] // Lava
			{
				"{0} died in lava",
				"{0} was melted",
				"{0} tried to swim in lava",
				"{0} didn't know what the glowy orange liquid was",
				"{0} fell into lava",
				"{0} became one with the glowing gooey rock",
				"{0} died due to lava",
				"{0} got burnt in the lava"
			}
			},
			{
				DeathType.BlackHole,
				new[] // ATP core black hole
			{
				"{0} should visit the Ash Twin Project again",
				"{0} waited inside the Ash Twin Project",
				"{0} chased their memories"
			}
			},
			{
				DeathType.DreamExplosion,
				new[] // using the prototype
			{
				"{0} exploded",
				"{0} was an early adopter",
				"{0} went kaboom",
				"{0} was fried",
				"{0} died due to explosion",
				"{0} used the wrong artifact"
			}
			},
			{
				DeathType.CrushedByElevator,
				new[] // elevator-induced pancakeness
			{
				"{0} was crushed",
				"{0} was squished",
				"{0} was crushed by an elevator",
				"{0} stood under an elevator",
				"{0} became a flat-hearther",
				"{0} was squished by an elevator"
			}
			},
		};

		public static string GetPhrase(DeathType deathType, int index)
			=> !Darkhold.ContainsKey(deathType)
				? Darkhold[DeathType.Default][index]
				: Darkhold[deathType][index];

		public static int GetRandomIndex(DeathType deathType)
			=> new Random().Next(0, Darkhold[deathType].Length);
	}
}