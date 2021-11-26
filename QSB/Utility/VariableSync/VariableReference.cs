using System;

namespace QSB.Utility.VariableSync
{
	public class VariableReference<T>
	{
		private Func<T> _getter;
		private Action<T> _setter;

		public VariableReference(Func<T> getter, Action<T> setter)
		{
			_getter = getter;
			_setter = setter;
		}

		public T Value
		{
			get => _getter();
			set => _setter(value);
		}
	}
}
