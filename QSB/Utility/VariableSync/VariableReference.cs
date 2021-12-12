using System;

namespace QSB.Utility.VariableSync
{
	public readonly struct VariableReference<T>
	{
		private readonly BaseVariableSyncer _owner;
		private readonly Func<T> _getter;
		private readonly Action<T> _setter;

		public VariableReference(BaseVariableSyncer owner, Func<T> getter, Action<T> setter)
		{
			_owner = owner;
			_getter = getter;
			_setter = setter;
		}

		public T Value
		{
			get
			{
				if (_getter != null)
				{
					return _getter();
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
				if (_setter != null)
				{
					_setter(value);
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
