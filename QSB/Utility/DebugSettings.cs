using OWML.Common;

namespace QSB.Utility;

/// <summary>
/// purely organizational class to store all debug settings
/// </summary>
public class DebugSettings
{
	public bool DebugMode;

	private bool _logQSBMessages;
	public bool LogQSBMessages => DebugMode && _logQSBMessages;

	private bool _instanceIdInLogs;
	public bool InstanceIDInLogs => DebugMode && _instanceIdInLogs;

	private bool _hookDebugLogs;
	public bool HookDebugLogs => DebugMode && _hookDebugLogs;

	private bool _avoidTimeSync;
	public bool AvoidTimeSync => DebugMode && _avoidTimeSync;

	private bool _autoStart;
	public bool AutoStart => DebugMode && _autoStart;

	private int _latencySimulation;
	public int LatencySimulation => DebugMode ? _latencySimulation : 0;

	private bool _drawGUI;
	public bool DrawGUI => DebugMode && _drawGUI;

	private bool _drawLines;
	public bool DrawLines => DebugMode && _drawLines;

	private bool _drawLabels;
	public bool DrawLabels => DebugMode && _drawLabels;

	private bool _greySkybox;
	public bool GreySkybox => DebugMode && _greySkybox;

	public void Update(IModConfig config)
	{
		DebugMode = config.GetSettingsValue<bool>("debugMode");

		_instanceIdInLogs = config.GetSettingsValue<bool>("instanceIdInLogs");
		_hookDebugLogs = config.GetSettingsValue<bool>("hookDebugLogs");
		_avoidTimeSync = config.GetSettingsValue<bool>("avoidTimeSync");
		_autoStart = config.GetSettingsValue<bool>("autoStart");
		_drawGUI = config.GetSettingsValue<bool>("drawGui");
		_drawLines = config.GetSettingsValue<bool>("drawLines");
		_drawLabels = config.GetSettingsValue<bool>("drawLabels");
		_greySkybox = config.GetSettingsValue<bool>("greySkybox");
		_latencySimulation = config.GetSettingsValue<int>("latencySimulation");
		_logQSBMessages = config.GetSettingsValue<bool>("logQSBMessages");
	}
}
