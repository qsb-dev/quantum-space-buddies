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
		public bool DrawGui { get => _drawGui && DebugMode; set => _drawGui = value; }

		[JsonProperty("drawLines")]
		private bool _drawLines;
		public bool DrawLines { get => _drawLines && DebugMode; set => _drawLines = value; }

		[JsonProperty("showQuantumVisibilityObjects")]
		private bool _showQuantumVisibilityObjects;
		public bool ShowQuantumVisibilityObjects { get => _showQuantumVisibilityObjects && DebugMode; set => _showQuantumVisibilityObjects = value; }

		[JsonProperty("showDebugLabels")]
		private bool _showDebugLabels;
		public bool ShowDebugLabels { get => _showDebugLabels && DebugMode; set => _showDebugLabels = value; }

		[JsonProperty("avoidTimeSync")]
		private bool _avoidTimeSync;
		public bool AvoidTimeSync { get => _avoidTimeSync && DebugMode; set => _avoidTimeSync = value; }

		[JsonProperty("skipTitleScreen")]
		private bool _skipTitleScreen;
		public bool SkipTitleScreen { get => _skipTitleScreen && DebugMode; set => _skipTitleScreen = value; }

		[JsonProperty("greySkybox")]
		private bool _greySkybox;
		public bool GreySkybox { get => _greySkybox && DebugMode; set => _greySkybox = value; }

		[JsonProperty("playerIdInLogs")]
		private bool _playerIdInLogs;
		public bool PlayerIdInLogs { get => _playerIdInLogs && DebugMode; set => _playerIdInLogs = value; }
	}
}
