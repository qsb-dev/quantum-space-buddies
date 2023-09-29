using System;

namespace QSB.EchoesOfTheEye.Ghosts.WorldObjects;

public class QSBPrisonerEffects : QSBGhostEffects
{
	public override void PlaySleepAnimation(bool remote = false)
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayGrabAnimation(bool remote = false)
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayBlowOutLanternAnimation(bool fast = false, bool remote = false)
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlaySnapNeckAnimation(bool remote = false)
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}
}
