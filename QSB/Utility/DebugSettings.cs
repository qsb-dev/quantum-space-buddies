using Newtonsoft.Json;

namespace QSB.Utility
{
	public class DebugSettings
	{
		[JsonProperty("debugMode")]
		public bool DebugMode { get; set; } = false;

		[JsonProperty("drawLines")]
		public bool DrawLines { get; set; } = false;

		[JsonProperty("showQuantumVisibilityObjects")]
		public bool ShowQuantumVisibilityObjects { get; set; } = false;

		[JsonProperty("showQuantumDebugBoxes")]
		public bool ShowQuantumDebugBoxes { get; set; } = false;

		[JsonProperty("avoidTimeSync")]
		public bool AvoidTimeSync { get; set; } = false;

		[JsonProperty("skipTitleScreen")]
		public bool SkipTitleScreen { get; set; } = false;
	}
}
