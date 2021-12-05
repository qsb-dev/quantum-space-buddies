using QSB.Messaging;
using QSB.Utility;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;

namespace QSB.SaveSync.Events
{
	internal class GameStateMessage : PlayerMessage
	{
		public bool InSolarSystem { get; set; }
		public bool InEye { get; set; }
		public int LoopCount { get; set; }
		public bool[] KnownFrequencies { get; set; }
		public Dictionary<int, bool> KnownSignals { get; set; } = new();

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			// in solarsystem
			InSolarSystem = reader.ReadBoolean();

			// in eye
			InEye = reader.ReadBoolean();

			// Loop count
			LoopCount = reader.ReadInt32();

			// Known Frequencies
			var frequenciesLength = reader.ReadInt32();
			var knownFrequencies = KnownFrequencies;
			Array.Resize(ref knownFrequencies, frequenciesLength);
			KnownFrequencies = knownFrequencies;
			for (var i = 0; i < frequenciesLength; i++)
			{
				KnownFrequencies[i] = reader.ReadBoolean();
			}

			// Known signals
			var signalsLength = reader.ReadInt32();
			KnownSignals.Clear();
			for (var i = 0; i < signalsLength; i++)
			{
				var key = reader.ReadInt32();
				var value = reader.ReadBoolean();
				KnownSignals.Add(key, value);
			}
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			// in solarsystem
			writer.Write(InSolarSystem);

			// in eye
			writer.Write(InEye);

			// Loop count
			writer.Write(LoopCount);

			// Known frequencies
			writer.Write(KnownFrequencies.Length);
			foreach (var item in KnownFrequencies)
			{
				writer.Write(item);
			}

			// Known signals
			writer.Write(KnownSignals.Count);
			foreach (var item in KnownSignals)
			{
				writer.Write(item.Key);
				writer.Write(item.Value);
			}
		}
	}
}
