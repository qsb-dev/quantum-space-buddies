using QSB.Animation.Player;
using QSB.Animation.Player.Thrusters;
using QSB.PlayerBodySetup.Remote;

namespace QSB.Player;

public partial class PlayerInfo
{
	public bool Visible => IsLocalPlayer || _ditheringAnimator == null || _ditheringAnimator.FullyVisible;
	public AnimationSync AnimationSync { get; }
	public JetpackAccelerationSync JetpackAcceleration { get; set; }
	internal QSBDitheringAnimator _ditheringAnimator;
	public DreamWorldSpawnAnimator DreamWorldSpawnAnimator { get; set; }
	public RemotePlayerFluidDetector FluidDetector { get; set; }
	public HelmetAnimator HelmetAnimator { get; set; }
}
