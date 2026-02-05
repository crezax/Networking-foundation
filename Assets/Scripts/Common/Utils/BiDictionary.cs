using System;
using System.Collections.Generic;

class BiDictionary<TFirst, TSecond> {
  IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
  IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

  public void Add(TFirst first, TSecond second) {
    if (firstToSecond.ContainsKey(first) ||
        secondToFirst.ContainsKey(second)) {
      throw new ArgumentException("Duplicate first or second");
    }
    firstToSecond.Add(first, second);
    secondToFirst.Add(second, first);
  }

  public bool TryGetByFirst(TFirst first, out TSecond second) => firstToSecond.TryGetValue(first, out second);

  public bool TryGetBySecond(TSecond second, out TFirst first) => secondToFirst.TryGetValue(second, out first);

  public void RemoveByFirst(TFirst first) {
    TSecond second;
    if (TryGetByFirst(first, out second)) {
      firstToSecond.Remove(first);
      secondToFirst.Remove(second);
    }
  }

  public void RemoveBySecond(TSecond second) {
    TFirst first;
    if (TryGetBySecond(second, out first)) {
      firstToSecond.Remove(first);
      secondToFirst.Remove(second);
    }
  }
}
