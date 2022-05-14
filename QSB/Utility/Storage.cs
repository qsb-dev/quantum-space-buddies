using Newtonsoft.Json;

namespace QSB.Utility;

[JsonObject(MemberSerialization.OptIn)]
public class Storage
{
	[JsonProperty("lastUsedVersion")]
	public string LastUsedVersion;
}
