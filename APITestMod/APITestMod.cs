using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace APITestMod;

public class APITestMod : ModBehaviour
{
	public void Start()
	{
		var qsbAPI = ModHelper.Interaction.TryGetModApi<IQSBAPI>("Raicuparta.QuantumSpaceBuddies");
		var menuFrameworkAPI = ModHelper.Interaction.TryGetModApi<IMenuAPI>("_nebula.MenuFramework");

		LoadManager.OnCompleteSceneLoad += (oldScene, newScene) =>
		{
			if (newScene != OWScene.SolarSystem)
			{
				return;
			}

			var button = menuFrameworkAPI.PauseMenu_MakeSimpleButton("QSB Api Test");

			qsbAPI.OnPlayerJoin().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} joined the game!", MessageType.Success));
			qsbAPI.OnPlayerLeave().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} left the game!", MessageType.Success));

			button.onClick.AddListener(() =>
			{
				ModHelper.Console.WriteLine("TESTING QSB API!");

				ModHelper.Console.WriteLine($"Local Player ID : {qsbAPI.GetLocalPlayerID()}");

				ModHelper.Console.WriteLine("Player IDs :");

				foreach (var playerID in qsbAPI.GetPlayerIDs())
				{
					ModHelper.Console.WriteLine($" - id:{playerID} name:{qsbAPI.GetPlayerName(playerID)}");
				}

				ModHelper.Console.WriteLine("Setting custom data as \"QSB TEST STRING\"");
				qsbAPI.SetCustomData(qsbAPI.GetLocalPlayerID(), "APITEST.TESTSTRING", "QSB TEST STRING");
				ModHelper.Console.WriteLine($"Retreiving custom data : {qsbAPI.GetCustomData<string>(qsbAPI.GetLocalPlayerID(), "APITEST.TESTSTRING")}");

				ModHelper.Console.WriteLine("Sending string message test...");
				qsbAPI.RegisterHandler<string>("apitest-string", MessageHandler);
				qsbAPI.SendMessage("apitest-string", "STRING MESSAGE", receiveLocally: true);

				ModHelper.Console.WriteLine("Sending int message test...");
				qsbAPI.RegisterHandler<int>("apitest-int", MessageHandler);
				qsbAPI.SendMessage("apitest-int", 123, receiveLocally: true);

				ModHelper.Console.WriteLine("Sending float message test...");
				qsbAPI.RegisterHandler<float>("apitest-float", MessageHandler);
				qsbAPI.SendMessage("apitest-float", 3.14f, receiveLocally: true);
			});
		};
	}

	private void MessageHandler<T>(uint from, T data)
		=> ModHelper.Console.WriteLine($"Got : {data}");
}
