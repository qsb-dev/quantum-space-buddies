//
// Source: https://stackoverflow.com/questions/255341/getting-multiple-keys-of-specified-value-of-a-generic-dictionary#255630
//

using System;
using System.Collections.Generic;

namespace DiscordMirror;

internal class BiDictionary<TFirst, TSecond>
{
	private readonly IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
	private readonly IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

	public void Add(TFirst first, TSecond second)
	{
		if (firstToSecond.ContainsKey(first) ||
			secondToFirst.ContainsKey(second))
		{
			throw new ArgumentException("Duplicate first or second");
		}
		firstToSecond.Add(first, second);
		secondToFirst.Add(second, first);
	}

	public bool TryGetByFirst(TFirst first, out TSecond second) => firstToSecond.TryGetValue(first, out second);

	public void Remove(TFirst first)
	{
		secondToFirst.Remove(firstToSecond[first]);
		firstToSecond.Remove(first);
	}

	public bool TryGetBySecond(TSecond second, out TFirst first) => secondToFirst.TryGetValue(second, out first);

	public TSecond GetByFirst(TFirst first) => firstToSecond[first];

	public TFirst GetBySecond(TSecond second) => secondToFirst[second];
}
