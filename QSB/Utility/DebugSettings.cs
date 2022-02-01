using Newtonsoft.Json;

namespace QSB.Utility
{
	public class DebugSettings
	{
		[JsonProperty("useKcpTransport")]
		public bool UseKcpTransport { get; set; }

		[JsonProperty("overrideAppId")]
		public int OverrideAppId { get; set; } = -1;

		[JsonProperty("dumpWorldObjects")]
		public bool DumpWorldObjects { get; set; }

		[JsonProperty("debugMode")]
		public bool DebugMode { get; set; }

		[JsonProperty("drawGui")]
		private bool _drawGui;
		public bool DrawGui => DebugMode && _drawGui;

		[JsonProperty("drawLines")]
		private bool _drawLines;
		public bool DrawLines => DebugMode && _drawLines;

		[JsonProperty("showQuantumVisibilityObjects")]
		private bool _showQuantumVisibilityObjects;
		public bool ShowQuantumVisibilityObjects => DebugMode && _showQuantumVisibilityObjects;

		[JsonProperty("showDebugLabels")]
		private bool _showDebugLabels;
		public bool ShowDebugLabels => DebugMode && _showDebugLabels;

		[JsonProperty("avoidTimeSync")]
		private bool _avoidTimeSync;
		public bool AvoidTimeSync => DebugMode && _avoidTimeSync;

		[JsonProperty("skipTitleScreen")]
		private bool _skipTitleScreen;
		public bool SkipTitleScreen => DebugMode && _skipTitleScreen;

		[JsonProperty("greySkybox")]
		private bool _greySkybox;
		public bool GreySkybox => DebugMode && _greySkybox;

		[JsonProperty("playerIdInLogs")]
		private bool _playerIdInLogs;
		public bool PlayerIdInLogs => DebugMode && _playerIdInLogs;
	}
}
