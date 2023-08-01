using Mirror;
using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.Messaging;
using System.Linq;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers.Messages;

public class InitialStateMessage : QSBWorldObjectMessage<QSBEclipseCodeController>
{
	private int _selectedDial;
	private int[] _dialSelectedSymbols;

	public InitialStateMessage(EclipseCodeController4 eclipseCodeController)
	{
		_selectedDial = eclipseCodeController._selectedDial;
		_dialSelectedSymbols = eclipseCodeController._dials.Select(x => x.GetSymbolSelected()).ToArray();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(_selectedDial);
		writer.Write(_dialSelectedSymbols);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_selectedDial = reader.Read<int>();
		_dialSelectedSymbols = reader.Read<int[]>();
	}

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._selectedDial = _selectedDial;
		WorldObject.AttachedObject._currentSelectorPosY = WorldObject.AttachedObject._dials[_selectedDial].transform.localPosition.y;
		for (var i = 0; i < WorldObject.AttachedObject._selectors.Length; i++)
		{
			WorldObject.AttachedObject._selectors[i].SetLocalPositionY(WorldObject.AttachedObject._currentSelectorPosY);
		}

		for (var i = 0; i < WorldObject.AttachedObject._dials.Length; i++)
		{
			var dial = WorldObject.AttachedObject._dials[i];
			dial._symbolSelected = _dialSelectedSymbols[i];
			dial.InstantRotate();
		}
	}
}
