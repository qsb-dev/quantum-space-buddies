using Mirror;
using QSB.Messaging;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages;

internal class KillGalaxiesMessage : QSBMessage
{
	private List<float> _deathDelays;

	public KillGalaxiesMessage(List<float> deathDelays) => _deathDelays = deathDelays;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.WriteList(_deathDelays);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_deathDelays = reader.ReadList<float>();
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var galaxyController = QSBWorldSync.GetUnityObject<MiniGalaxyController>();

		galaxyController._killTrigger.OnEntry -= galaxyController.OnEnterKillTrigger;

		galaxyController._galaxies = galaxyController.GetComponentsInChildren<MiniGalaxy>(true);
		for (var i = 0; i < galaxyController._galaxies.Length; i++)
		{
			galaxyController._galaxies[i].DieAfterSeconds(_deathDelays[i], true, AudioType.EyeGalaxyBlowAway);
		}

		galaxyController._forestIsDarkTime = Time.time + 65f;
		galaxyController.enabled = true;

		galaxyController._musicSource.SetLocalVolume(0f);
		galaxyController._musicSource.FadeIn(5f);
	}
}