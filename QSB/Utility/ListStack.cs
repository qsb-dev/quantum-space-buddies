using System;
using System.Collections;
using System.Collections.Generic;

namespace QSB.Utility;

/// <summary>
/// A LIFO collection with List<> functionality.
/// </summary>
public class ListStack<T> : IEnumerable<T>
{
	private List<T> _items = new();

	public int Count => _items.Count;

	private readonly bool _removeDuplicates;

	/// <param name="removeDuplicates">If true, all elements equal to the added item will be removed prior to adding the new element.</param>
	public ListStack(bool removeDuplicates)
	{
		_removeDuplicates = removeDuplicates;
	}

	/// <summary>
	/// Removes all items from the stack.
	/// </summary>
	public void Clear()
		=> _items.Clear();

	/// <summary>
	/// Pushes an element onto the front of the stack.
	/// </summary>
	public void Push(T item)
	{
		if (_removeDuplicates && _items.Contains(item))
		{
			RemoveAll(x => EqualityComparer<T>.Default.Equals(x, item));
		}

		_items.Add(item);
	}

	/// <summary>
	/// Pops an element off the front of the stack.
	/// </summary>
	public T PopFromFront()
	{
		if (_items.Count > 0)
		{
			var temp = _items[_items.Count - 1];
			_items.RemoveAt(_items.Count - 1);
			return temp;
		}

		return default;
	}

	/// <summary>
	/// Pops an element off the back of the stack and shifts the entire stack backwards.
	/// </summary>
	public T PopFromBack()
	{
		if (_items.Count == 0)
		{
			return default;
		}

		var firstElement = _items[0];
		_items.RemoveAt(0);
		return firstElement;
	}

	/// <summary>
	/// Returns the element at the front of the stack.
	/// </summary>
	public T PeekFront() => _items.Count > 0
		? _items[_items.Count - 1]
		: default;

	/// <summary>
	/// Returns the element at the back of the stack.
	/// </summary>
	public T PeekBack() => _items.Count > 0
		? _items[0]
		: default;

	/// <summary>
	/// Removes the element at the given index, where 0 is the back of the stack. The stack will shift backwards to fill empty space.
	/// </summary>
	public void RemoveAt(int index)
		=> _items.RemoveAt(index);

	/// <summary>
	/// Removes the first occurence (back to front) of an item.
	/// </summary>
	public bool Remove(T item)
		=> _items.Remove(item);

	/// <summary>
	/// Removes all elements that match the given predicate.
	/// </summary>
	public int RemoveAll(Predicate<T> match)
		=> _items.RemoveAll(match);

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
	public IEnumerator GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
}
