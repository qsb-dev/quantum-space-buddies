using Newtonsoft.Json;

namespace QSB.Utility
{
	public class DebugSettings
	{
		[JsonProperty("useKcpTransport")]
		public bool UseKcpTransport;

		[JsonProperty("overrideAppId")]
		public int OverrideAppId = -1;

		[JsonProperty("dumpWorldObjects")]
		public bool DumpWorldObjects;

		[JsonProperty("debugMode")]
		public bool DebugMode;

		[JsonProperty("drawGui")]
		private bool _drawGui;
		public bool DrawGui => DebugMode && _drawGui;

		[JsonProperty("drawLines")]
		private bool _drawLines;
		public bool DrawLines => DebugMode && _drawLines;

		[JsonProperty("drawLabels")]
		private bool _drawLabels;
		public bool DrawLabels => DebugMode && _drawLabels;

		[JsonProperty("drawQuantumVisibilityObjects")]
		private bool _drawQuantumVisibilityObjects;
		public bool DrawQuantumVisibilityObjects => DebugMode && _drawQuantumVisibilityObjects;

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
