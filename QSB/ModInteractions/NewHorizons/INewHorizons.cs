using UnityEngine.Events;

namespace QSB.ModInteractions.NewHorizons;

internal interface INewHorizons
{
	/// <summary>
	/// An event invoked when NH has finished generating all planets for a new star system.
	/// Gives the name of the star system that was just loaded.
	/// </summary>
	UnityEvent<string> GetStarSystemLoadedEvent();
}
