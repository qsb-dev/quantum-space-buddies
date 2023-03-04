using System;
using System.Collections;
using System.Collections.Generic;

namespace QSB.Utility;

public class ListStack<T> : IEnumerable<T>
{
	private readonly List<T> _items = new();

	public void Clear()
		=> _items.Clear();

	public void Push(T item)
	{
		if (_items.Contains(item))
		{
			RemoveAll(x => EqualityComparer<T>.Default.Equals(x, item));
		}

		_items.Add(item);
	}

	public T Pop()
	{
		if (_items.Count > 0)
		{
			var temp = _items[_items.Count - 1];
			_items.RemoveAt(_items.Count - 1);
			return temp;
		}

		return default;
	}

	public T Peek() => _items.Count > 0
		? _items[_items.Count - 1]
		: default;

	public void RemoveAt(int index)
		=> _items.RemoveAt(index);

	public bool Remove(T item)
		=> _items.Remove(item);

	public int RemoveAll(Predicate<T> match)
		=> _items.RemoveAll(match);

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
	public IEnumerator GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
}
