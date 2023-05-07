using System;
using System.Collections;
using System.Collections.Generic;

namespace QSB.Utility;

public class ListStack<T> : IEnumerable<T>
{
	private List<T> _items = new();

	public int Count => _items.Count;

	public readonly bool RemoveDuplicates;

	public ListStack(bool removeDuplicates)
	{
		RemoveDuplicates = removeDuplicates;
	}

	public void Clear()
		=> _items.Clear();

	public void Push(T item)
	{
		if (RemoveDuplicates && _items.Contains(item))
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

	public T RemoveFirstElementAndShift()
	{
		if (_items.Count == 0)
		{
			return default;
		}

		var firstElement = _items[0];

		if (_items.Count == 0)
		{
			return firstElement;
		}

		// shift list left
		// allocates blehhh who cares
		_items = _items.GetRange(1, _items.Count - 1);

		return firstElement;
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
