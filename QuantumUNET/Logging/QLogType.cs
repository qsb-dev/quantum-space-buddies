using System;

namespace QuantumUNET.Logging
{
	[Flags]
	public enum QLogType
	{
		Debug,
		Log,
		Warning,
		Error,
		FatalError
	}
}
