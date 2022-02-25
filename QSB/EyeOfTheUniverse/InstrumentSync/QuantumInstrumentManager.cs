using Cysharp.Threading.Tasks;
using QSB.EyeOfTheUniverse.InstrumentSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EyeOfTheUniverse.InstrumentSync;

internal class QuantumInstrumentManager : WorldObjectManager
{
	public override WorldObjectType WorldObjectType => WorldObjectType.Eye;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBQuantumInstrument, QuantumInstrument>();
}