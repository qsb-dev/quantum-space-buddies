using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts;

internal class GhostManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	private static GhostHotelDirector _hotelDirector;
	private static GhostPartyPathDirector _partyPathDirector;
	private static GhostZone2Director _zone2Director;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBGhostController, GhostController>();
		QSBWorldSync.Init<QSBGhostEffects, GhostEffects>(typeof(PrisonerEffects));
		QSBWorldSync.Init<QSBPrisonerEffects, GhostEffects>(QSBWorldSync.GetUnityObjects<PrisonerEffects>());
		QSBWorldSync.Init<QSBGhostSensors, GhostSensors>();
		QSBWorldSync.Init<QSBGhostNodeMap, GhostNodeMap>();
		// to avoid disabled ghosts (TheCollector)
		QSBWorldSync.Init<QSBGhostBrain, GhostBrain>(QSBWorldSync.GetUnityObjects<GhostBrain>().Where(x => x.gameObject.activeSelf).SortDeterministic());
		QSBWorldSync.Init<QSBGhostGrabController, GhostGrabController>();

		_hotelDirector = QSBWorldSync.GetUnityObject<GhostHotelDirector>();
		_partyPathDirector = QSBWorldSync.GetUnityObject<GhostPartyPathDirector>();
		_zone2Director = QSBWorldSync.GetUnityObject<GhostZone2Director>();

		for (int i = 0; i < _hotelDirector._hotelDepthsGhosts.Length; i++)
		{
			_hotelDirector._hotelDepthsGhosts[i].OnIdentifyIntruder -= _hotelDirector.OnHotelDepthsGhostsIdentifiedIntruder;
			_hotelDirector._hotelDepthsGhosts[i].GetWorldObject<QSBGhostBrain>().OnIdentifyIntruder += CustomOnHotelDepthsGhostsIdentifiedIntruder;
		}

		for (var j = 0; j < _partyPathDirector._directedGhosts.Length; j++)
		{
			_partyPathDirector._directedGhosts[j].OnIdentifyIntruder -= _partyPathDirector.OnGhostIdentifyIntruder;
			_partyPathDirector._directedGhosts[j].GetWorldObject<QSBGhostBrain>().OnIdentifyIntruder += CustomOnGhostIdentifyIntruder;
		}

		for (int i = 0; i < _zone2Director._cityGhosts.Length; i++)
		{
			_zone2Director._cityGhosts[i].OnIdentifyIntruder -= _zone2Director.OnCityGhostsIdentifiedIntruder;
			_zone2Director._cityGhosts[i].GetWorldObject<QSBGhostBrain>().OnIdentifyIntruder += CustomOnCityGhostsIdentifiedIntruder;
		}

		var allCollisionGroups = Resources.FindObjectsOfTypeAll<SectorCollisionGroup>();
		var city = allCollisionGroups.First(x => x.name == "City");
		city.SetSector(_zone2Director._sector);
	}

	public static void CustomOnHotelDepthsGhostsIdentifiedIntruder(GhostBrain ghostBrain, QSBGhostData ghostData)
	{
		if (_hotelDirector._playerIdentifiedInDepths)
		{
			return;
		}

		var num = Random.Range(2f, 3f);
		for (var i = 0; i < _hotelDirector._hotelDepthsGhosts.Length; i++)
		{
			if (!(_hotelDirector._hotelDepthsGhosts[i] == ghostBrain) && _hotelDirector._hotelDepthsGhosts[i].HearGhostCall(ghostData.interestedPlayer.playerLocation.localPosition, num, false))
			{
				num += Random.Range(2f, 3f);
			}
		}
	}

	public static void CustomOnGhostIdentifyIntruder(GhostBrain ghostBrain, QSBGhostData ghostData)
	{
		float num = Random.Range(2f, 3f);
		for (int i = 0; i < _partyPathDirector._directedGhosts.Length; i++)
		{
			if (!(_partyPathDirector._directedGhosts[i] == ghostBrain))
			{
				bool flag = _partyPathDirector._directedGhosts[i].GetCurrentActionName() != GhostAction.Name.PartyPath || ((QSBPartyPathAction)_partyPathDirector._directedGhosts[i].GetWorldObject<QSBGhostBrain>().GetCurrentAction()).allowHearGhostCall;
				float num2 = Vector3.Distance(ghostBrain.transform.position, _partyPathDirector._directedGhosts[i].transform.position);
				if (flag && num2 < 50f && _partyPathDirector._directedGhosts[i].HearGhostCall(ghostData.interestedPlayer.playerLocation.localPosition, num, true))
				{
					_partyPathDirector._directedGhosts[i].GetWorldObject<QSBGhostBrain>().HintPlayerLocation(ghostData.interestedPlayer.player);
					num += Random.Range(2f, 3f);
				}
			}
		}
	}

	public static void CustomOnCityGhostsIdentifiedIntruder(GhostBrain ghostBrain, QSBGhostData ghostData)
	{
		if (_zone2Director._playerIdentifiedInCity)
		{
			return;
		}

		_zone2Director._playerIdentifiedInCity = true;
		float num = Random.Range(2f, 3f);
		for (int i = 0; i < _zone2Director._cityGhosts.Length; i++)
		{
			if (!(_zone2Director._cityGhosts[i] == ghostBrain) && _zone2Director._cityGhosts[i].HearGhostCall(ghostData.interestedPlayer.playerLocation.localPosition, num, false))
			{
				num += Random.Range(2f, 3f);
			}
		}
	}
}
