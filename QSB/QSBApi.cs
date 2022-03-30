using OWML.Common;

namespace QSB;

public class QSBApi
{
	public void RegisterAddon(IModBehaviour addon) => QSBCore.Addons.Add(addon);
}
