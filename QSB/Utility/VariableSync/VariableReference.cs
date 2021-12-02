using System;

namespace QSB.Utility.VariableSync
{
	public class VariableReference<T>
	{
		private BaseVariableSyncer _owner;

		public Func<T> Getter;
		public Action<T> Setter;

		public VariableReference(BaseVariableSyncer owner)
			=> _owner = owner;

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
					if (_owner.Ready)
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
					if (_owner.Ready)
					{
						DebugLog.ToConsole($"Warning - Setter is null!", OWML.Common.MessageType.Warning);
					}
				}
			}
		}
	}
}
