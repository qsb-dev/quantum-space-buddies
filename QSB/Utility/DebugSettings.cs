using Newtonsoft.Json;

namespace QSB.Utility
{
	public class DebugSettings
	{
		[JsonProperty("debugMode")]
		public bool DebugMode { get; set; }

		[JsonProperty("drawLines")]
		public bool DrawLines { get; set; }

		[JsonProperty("showQuantumVisibilityObjects")]
		public bool ShowQuantumVisibilityObjects { get; set; }

		[JsonProperty("showDebugLabels")]
		public bool ShowDebugLabels { get; set; }

		[JsonProperty("avoidTimeSync")]
		public bool AvoidTimeSync { get; set; }

		[JsonProperty("skipTitleScreen")]
		public bool SkipTitleScreen { get; set; }

		[JsonProperty("greySkybox")]
		public bool GreySkybox { get; set; }

		[JsonProperty("playerIdInLogs")]
		public bool PlayerIdInLogs { get; set; }
	}
}
