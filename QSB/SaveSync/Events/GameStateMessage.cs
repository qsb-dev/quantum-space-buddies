using QSB.Messaging;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;

namespace QSB.SaveSync.Events
{
	internal class GameStateMessage : PlayerMessage
	{
		public bool WarpedToTheEye { get; set; }
		public float SecondsRemainingOnWarp { get; set; }
		public bool LaunchCodesGiven { get; set; }
		public int LoopCount { get; set; }
		public bool[] KnownFrequencies { get; set; }
		public Dictionary<int, bool> KnownSignals { get; set; } = new();

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			WarpedToTheEye = reader.ReadBoolean();
			SecondsRemainingOnWarp = reader.ReadSingle();
			LaunchCodesGiven = reader.ReadBoolean();
			LoopCount = reader.ReadInt32();

			var frequenciesLength = reader.ReadInt32();
			var knownFrequencies = KnownFrequencies;
			Array.Resize(ref knownFrequencies, frequenciesLength);
			KnownFrequencies = knownFrequencies;
			for (var i = 0; i < frequenciesLength; i++)
			{
				KnownFrequencies[i] = reader.ReadBoolean();
			}

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
			writer.Write(WarpedToTheEye);
			writer.Write(SecondsRemainingOnWarp);
			writer.Write(LaunchCodesGiven);
			writer.Write(LoopCount);

			writer.Write(KnownFrequencies.Length);
			foreach (var item in KnownFrequencies)
			{
				writer.Write(item);
			}

			writer.Write(KnownSignals.Count);
			foreach (var item in KnownSignals)
			{
				writer.Write(item.Key);
				writer.Write(item.Value);
			}
		}
	}
}
