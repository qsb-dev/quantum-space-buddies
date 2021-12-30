using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages
{
	internal class KillGalaxiesMessage : QSBMessage
	{
		private List<float> _deathDelays;

		public KillGalaxiesMessage(List<float> deathDelays) => _deathDelays = deathDelays;

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_deathDelays.Count);
			foreach (var item in _deathDelays)
			{
				writer.Write(item);
			}
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			var length = reader.ReadInt32();
			_deathDelays = new List<float>(length);
			for (var i = 0; i < length; i++)
			{
				_deathDelays.Add(reader.ReadSingle());
			}
		}

		public override void OnReceiveRemote()
		{
			var galaxyController = QSBWorldSync.GetUnityObjects<MiniGalaxyController>().First();

			galaxyController._killTrigger.OnEntry -= galaxyController.OnEnterKillTrigger;
			galaxyController._galaxies = galaxyController.GetComponentsInChildren<MiniGalaxy>(true);

			for (var i = 0; i < galaxyController._galaxies.Length; i++)
			{
				galaxyController._galaxies[i].DieAfterSeconds(_deathDelays[i], true, AudioType.EyeGalaxyBlowAway);
			}

			galaxyController._forestIsDarkTime = Time.time + 65f;
			galaxyController.enabled = true;

			galaxyController._musicSource.SetLocalVolume(0f);
			galaxyController._musicSource.FadeIn(5f, false, false, 1f);
		}
	}
}
