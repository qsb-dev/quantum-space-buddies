using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using QSBNH.QuantumPlanet.WorldObjects;

namespace QSBNH.QuantumPlanet;
public class QuantumPlanetManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;
	public override bool DlcOnly => false;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBQuantumPlanet, NewHorizons.Components.Quantum.QuantumPlanet>();
}
