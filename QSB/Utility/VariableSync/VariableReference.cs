using QSB.Player;
using QSB.Player.TransformSync;
using System;

namespace QSB.Utility.VariableSync
{
	public class VariableReference<T>
	{
		public Func<T> Getter;
		public Action<T> Setter;

		public T Value
		{
			get
			{
				if (Getter != null)
				{
					return Getter();
				}
				else
				{
					if (PlayerTransformSync.LocalInstance != null && QSBPlayerManager.LocalPlayer.IsReady)
					{
						DebugLog.ToConsole($"Warning - Getter is null!", OWML.Common.MessageType.Warning);
					}
					
					return default;
				}
			}
			set
			{
				if (Setter != null)
				{
					Setter(value);
				}
				else
				{
					if (PlayerTransformSync.LocalInstance != null && QSBPlayerManager.LocalPlayer.IsReady)
					{
						DebugLog.ToConsole($"Warning - Setter is null!", OWML.Common.MessageType.Warning);
					}
				}
			}
		}
	}
}
