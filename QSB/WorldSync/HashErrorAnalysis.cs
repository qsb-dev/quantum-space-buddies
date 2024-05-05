using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using QSB.Utility;
using QSB.Player.Messages;
using QSB.Messaging;
using QSB.Utility.Deterministic;

namespace QSB.WorldSync;

public class HashErrorAnalysis
{
	public static Dictionary<string, HashErrorAnalysis> Instances = new();

	private readonly string _managerName;
	
	private readonly List<(string hash, string path)> _paths = new();

	public HashErrorAnalysis(string managerName) => _managerName = managerName;

	public void OnReceiveMessage(string deterministicPath) => _paths.Add((deterministicPath.GetMD5Hash(), deterministicPath));

	public void AllDataSent(uint from)
	{
		var serverObjects = QSBWorldSync.GetWorldObjectsFromManager(_managerName);

		var serverDetPaths = serverObjects.Select(x => x.AttachedObject.DeterministicPath());
		var serverDetPathDict = serverDetPaths.Select(path => (path.GetMD5Hash(), path)).ToList<(string hash, string path)>();

		var serverDoesNotHave = new List<string>();
		var clientDoesNotHave = new List<string>();

		foreach (var (hash, path) in serverDetPathDict)
		{
			if (!_paths.Any(x => x.hash == hash))
			{
				// client does not contain something from the server
				clientDoesNotHave.Add(path);
			}
		}

		foreach (var (hash, path) in _paths)
		{
			if (!serverDetPathDict.Any(x => x.hash == hash))
			{
				// client does not contain something from the server
				serverDoesNotHave.Add(path);
			}
		}

		DebugLog.ToConsole($"{_managerName} - Client is missing :", MessageType.Error);
		foreach (var item in clientDoesNotHave)
		{
			DebugLog.ToConsole($"- {item}", MessageType.Error);
		}

		DebugLog.ToConsole($"{_managerName} - Client has extra :", MessageType.Error);
		foreach (var item in serverDoesNotHave)
		{
			DebugLog.ToConsole($"- {item}", MessageType.Error);
		}
		Instances.Remove(_managerName);

		new PlayerKickMessage(from, $"WorldObject hash error for {_managerName}").Send();
	}
}
