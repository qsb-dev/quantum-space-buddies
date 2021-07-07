using OWML.Utils;
using QuantumUNET;
using System;
using System.Linq;
using UnityEngine;

namespace QSB
{
	public class QSBNetworkLobby : QNetworkBehaviour
	{
		public bool CanEditName { get; set; }
		public string PlayerName { get; private set; }

		// TODO : Could delete a lot of this - shouldnt be possible to not have a profile and still play

		private readonly string[] _defaultNames = {
			"Arkose",
			"Chert",
			"Esker",
			"Hal",
			"Hornfels",
			"Feldspar",
			"Gabbro",
			"Galena",
			"Gneiss",
			"Gossan",
			"Marl",
			"Mica",
			"Moraine",
			"Porphy",
			"Riebeck",
			"Rutile",
			"Slate",
			"Spinel",
			"Tektite",
			"Tephra",
			"Tuff",
			"Jinha"
		};

		public void Awake()
		{
			PlayerName = GetPlayerName();
			CanEditName = true;
		}

		private string GetPlayerName()
		{
			var profileManager = StandaloneProfileManager.SharedInstance;
			profileManager.Initialize();
			var profile = profileManager.GetValue<StandaloneProfileManager.ProfileData>("_currentProfile");
			var profileName = profile?.profileName;
			return !string.IsNullOrEmpty(profileName)
				? profileName
				: _defaultNames.OrderBy(x => Guid.NewGuid()).First();
		}

		public void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 200f, 20f), "Name:");
			if (CanEditName)
			{
				PlayerName = GUI.TextField(new Rect(60, 10, 145, 20f), PlayerName);
			}
			else
			{
				GUI.Label(new Rect(60, 10, 145, 20f), PlayerName);
			}
		}
	}
}