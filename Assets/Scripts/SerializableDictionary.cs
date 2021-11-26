using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<Key, Value> : Dictionary<Key, Value>, ISerializationCallbackReceiver
{
    [HideInInspector] [SerializeField] private List<Key> _keys = new List<Key>();
    [HideInInspector] [SerializeField] private List<Value> _values = new List<Value>();

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (var kvp in this)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();

        for (var i = 0; i != System.Math.Min(_keys.Count, _values.Count); i++)
        {
            Add(_keys[i], _values[i]);
        }
    }
}