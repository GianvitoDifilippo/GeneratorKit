#pragma warning disable RS1024 // Compare symbols correctly

using GeneratorKit.Interpret.Frame;
using GeneratorKit.Utils;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;

namespace GeneratorKit.TestHelpers;

internal class FakeFrameProvider : IFrameProvider
{
  private readonly List<Dictionary<string, object?>> _values;

  public FakeFrameProvider()
  {
    _values = new List<Dictionary<string, object?>>();
  }

  public IReadOnlyDictionary<string, object?> this[int index] => _values[index];

  public IDictionary<ISymbol, object?> GetFrame(int capacity = 0)
  {
    var dictionary = new Dictionary<ISymbol, object?>(capacity, SymbolDefinitionEqualityComparer.Default);
    var values = new Dictionary<string, object?>(capacity);
    _values.Add(values);
    return new FakeFrameDictionary(values, dictionary);
  }

  private class FakeFrameDictionary : IDictionary<ISymbol, object?>
  {
    private readonly Dictionary<string, object?> _values;
    private readonly Dictionary<ISymbol, object?> _dictionary;

    public FakeFrameDictionary(Dictionary<string, object?> values, Dictionary<ISymbol, object?> dictionary)
    {
      _values = values;
      _dictionary = dictionary;
    }

    public object? this[ISymbol key]
    {
      get => _dictionary[key];
      set
      {
        _values[key.Name] = value;
        _dictionary[key] = value;
      }
    }

    public ICollection<ISymbol> Keys => _dictionary.Keys;

    public ICollection<object?> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<ISymbol, object?>>)_dictionary).IsReadOnly;

    public void Add(ISymbol key, object? value)
    {
      _values.Add(key.Name, value);
      _dictionary.Add(key, value);
    }

    public void Add(KeyValuePair<ISymbol, object?> item)
    {
      ((ICollection<KeyValuePair<string, object?>>)_values).Add(KeyValuePair.Create(item.Key.Name, item.Value));
      ((ICollection<KeyValuePair<ISymbol, object?>>)_dictionary).Add(item);
    }

    public void Clear()
    {
      _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<ISymbol, object?> item)
    {
      return ((ICollection<KeyValuePair<ISymbol, object?>>)_dictionary).Contains(item);
    }

    public bool ContainsKey(ISymbol key)
    {
      return _dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<ISymbol, object?>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<ISymbol, object?>>)_dictionary).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<ISymbol, object?>> GetEnumerator()
    {
      return _dictionary.GetEnumerator();
    }

    public bool Remove(ISymbol key)
    {
      if (_dictionary.Remove(key))
      {
        _values.Remove(key.Name);
        return true;
      }
      return false;
    }

    public bool Remove(KeyValuePair<ISymbol, object?> item)
    {
      if (((ICollection<KeyValuePair<ISymbol, object?>>)_dictionary).Remove(item))
      {
        ((ICollection<KeyValuePair<string, object?>>)_values).Remove(KeyValuePair.Create(item.Key.Name, item.Value));
        return true;
      }
      return false;
    }

    public bool TryGetValue(ISymbol key, out object? value)
    {
      return _dictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)_dictionary).GetEnumerator();
    }
  }

}