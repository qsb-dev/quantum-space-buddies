using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Utility;

public static class TextureHelper
{
	public static Texture2D LoadTexture(string relativePath, TextureWrapMode wrapMode, bool forceMaxResolution)
	{
		var path = QSBCore.Helper.Manifest.ModFolderPath + relativePath;

		if (!File.Exists(path))
		{
			return null;
		}

		var data = File.ReadAllBytes(path);
		var tex = forceMaxResolution
			? new Texture2D(1, 1, TextureFormat.RGB24, false)
			: new Texture2D(1, 1);
		tex.LoadImage(data);
		tex.wrapMode = wrapMode;
		return tex;
	}
}
