using System.Collections.Generic;
using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.BodyCustomization;

public class BodyCustomizer : MonoBehaviour, IAddComponentOnStart
{
	private Dictionary<string, (Texture2D albedo, Texture2D normal)> skinMap = new();
	private Dictionary<string, Texture2D> jetpackMap = new();

	public AssetBundle SkinsBundle { get; private set; }

	public static BodyCustomizer Instance { get; private set; }

	private void Start()
	{
		Instance = this;
	}

	public void OnBundleLoaded(AssetBundle bundle)
	{
		SkinsBundle = bundle;
		LoadAssets();
	}

	private void LoadAssets()
	{
		DebugLog.DebugWrite($"Loading skin assets...", MessageType.Info);

		skinMap.Add("Default", (SkinsBundle.LoadAsset<Texture2D>("Assets/GameAssets/Texture2D/Traveller_HEA_Player_Skin_d.png"), SkinsBundle.LoadAsset<Texture2D>("Assets/GameAssets/Texture2D/Traveller_HEA_Player_Skin_n.png")));
		skinMap.Add("Type 1", LoadSkin("Type 1"));
		skinMap.Add("Type 2", LoadSkin("Type 2"));
		skinMap.Add("Type 3", LoadSkin("Type 3"));
		skinMap.Add("Type 4", LoadSkin("Type 4"));
		skinMap.Add("Type 5", LoadSkin("Type 5"));
		skinMap.Add("Type 6", LoadSkin("Type 6"));
		skinMap.Add("Type 7", LoadSkin("Type 7"));
		skinMap.Add("Type 8", LoadSkin("Type 8"));
		skinMap.Add("Type 9", LoadSkin("Type 9"));
		skinMap.Add("Type 10", LoadSkin("Type 10"));
		skinMap.Add("Type 11", LoadSkin("Type 11"));
		skinMap.Add("Type 12", LoadSkin("Type 12"));
		skinMap.Add("Type 13", LoadSkin("Type 13"));
		skinMap.Add("Type 14", LoadSkin("Type 14"));
		skinMap.Add("Type 15", LoadSkin("Type 15"));
		skinMap.Add("Type 16", LoadSkin("Type 16"));
		skinMap.Add("Type 17", LoadSkin("Type 17"));

		jetpackMap.Add("Orange", SkinsBundle.LoadAsset<Texture2D>("Assets/GameAssets/Texture2D/Props_HEA_Jetpack_d.png"));
		jetpackMap.Add("Yellow", LoadJetpack("yellow"));
		jetpackMap.Add("Red", LoadJetpack("red"));
		jetpackMap.Add("Pink", LoadJetpack("pink"));
		jetpackMap.Add("Purple", LoadJetpack("purple"));
		jetpackMap.Add("Dark Blue", LoadJetpack("darkblue"));
		jetpackMap.Add("Light Blue", LoadJetpack("lightblue"));
		jetpackMap.Add("Cyan", LoadJetpack("cyan"));
		jetpackMap.Add("Green", LoadJetpack("green"));
	}

	private (Texture2D d, Texture2D n) LoadSkin(string skinName)
	{
		var number = skinName.Replace($"Type ", "");
		return (SkinsBundle.LoadAsset<Texture2D>($"Assets/GameAssets/Texture2D/Skin Variations/{number}d.png"), SkinsBundle.LoadAsset<Texture2D>($"Assets/GameAssets/Texture2D/Skin Variations/{number}n.png"));
	}

	private Texture2D LoadJetpack(string jetpackName)
	{
		return SkinsBundle.LoadAsset<Texture2D>($"Assets/GameAssets/Texture2D/Jetpack Variations/{jetpackName}.png");
	}

	public void CustomizeRemoteBody(GameObject REMOTE_Traveller_HEA_Player_v2, string skinType, string jetpackType)
	{
		var headMesh = REMOTE_Traveller_HEA_Player_v2.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head");
		var skinMaterial = headMesh.GetComponent<SkinnedMeshRenderer>().material;

		skinMaterial.SetTexture("_MainTex", skinMap[skinType].albedo);
		skinMaterial.SetTexture("_BumpMap", skinMap[skinType].normal);

		var jetpackMesh = REMOTE_Traveller_HEA_Player_v2.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:Props_HEA_Jetpack");
		var jetpackMaterial = jetpackMesh.GetComponent<SkinnedMeshRenderer>().material;

		jetpackMaterial.SetTexture("_MainTex", jetpackMap[jetpackType]);
	}
}
