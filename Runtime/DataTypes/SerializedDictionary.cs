using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityCollections;
using UnityEngine;
using UnityEngine.Assertions;

namespace KOOP
{
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : 
        IDictionary<TKey, TValue>, 
        IReadOnlyDictionary<TKey, TValue>, 
        ISerializationCallbackReceiver,
        ISerializedDictionary
    {
        private static FieldInfo _dictionaryVersionField;
        private static StringBuilder _stringBuilder = new();
        
        [System.Serializable]
        public struct Entry
        {
            public TKey Key;
            public TValue Value;
        }
        
        [SerializeField] private List<Entry> _serializedData = new();
        
        [NonSerialized] private Dictionary<TKey, TValue> _dictionary = new();
        [NonSerialized] private int? _lastSerializedVersion = null;
        
        static SerializedDictionary()
        {
            var dictionaryType = typeof(Dictionary<TKey, TValue>);
            _dictionaryVersionField = dictionaryType.GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_dictionaryVersionField == null)
            {
                // Currently, _version seems correct, but Rider's decompiler shows source code using version
                // Let's check for both just to be safe
                _dictionaryVersionField = dictionaryType.GetField("version", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            
            Assert.IsNotNull(_dictionaryVersionField, $"Unable to find Dictionary.version field. Have we updated C# versions?");
        }
        
        public bool TryGetError(out string error)
        {
            _stringBuilder.Clear();
            CollectErrorsInternal(ref _stringBuilder);

            if (_stringBuilder.Length > 0)
            {
                error = _stringBuilder.ToString();
                return true;
            }

            error = null;
            return false;
        }

        protected virtual void CollectErrorsInternal(ref StringBuilder stringBuilder)
        {
            for (var i = 0; i < _serializedData.Count; i++)
            {
                if (_serializedData[i].Key == null)
                {
                    stringBuilder.AppendLine($"Missing key detected at index {i}.");
                }

                for (var j = i + 1; j < _serializedData.Count; j++)
                {
                    if (KeysEqual(_serializedData[i].Key, _serializedData[j].Key))
                    { 
                        stringBuilder.AppendLine($"Identical keys detected at indices {i} and {j}.\n");
                    }
                }
            }
        }

        
        public void OnBeforeSerialize()
        {
            // Perform version checking if we managed to retrieve our dictionary's internal version field
            if (TryGetDictionaryVersion(out var version))
            {
                if (_lastSerializedVersion.HasValue && _lastSerializedVersion.Value == version)
                {
                    return;
                }
                
                _lastSerializedVersion = version;
            }
            
            // Dictionary order is non-deterministic, so just clearing _serializedData
            // and re-adding everything may scramble our order
            // Manually handle removes, updates, and adds to preserve ordering as best we can
            
            // Copy our data to a scratch dictionary that we can modify
            // This allows us to avoid doing N^2 comparisons by naively searching through the whole collection repeatedly
            using var scratchDictionary = new PooledDictionary<TKey, TValue>();
            foreach (var kvp in _dictionary)
            {
                scratchDictionary.Add(kvp.Key, kvp.Value);
            }

            // Go through our list of existing serialized data
            for (var i = _serializedData.Count - 1; i >= 0; i--)
            {
                var serializedEntry = _serializedData[i];
                if (!scratchDictionary.TryGetValue(serializedEntry.Key, out var value))
                {
                    // Remove data that no longer exists in the dictionary
                    _serializedData.RemoveAt(i);
                }
                else
                {
                    // Update entries that still exist
                    serializedEntry.Value = value;
                    _serializedData[i] = serializedEntry;
                    
                    // Remove from the scratch dictionary to mark that we've processed that entry
                    scratchDictionary.Remove(serializedEntry.Key);
                }
            }
            
            // Add any values that still exist in the dictionary to the end of the list
            foreach (var kvp in scratchDictionary)
            {
                _serializedData.Add(new Entry() { Key = kvp.Key, Value = kvp.Value });
            }
        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();
            foreach (var data in _serializedData)
            {
                if (!_dictionary.ContainsKey(data.Key))
                {
                    _dictionary.Add(data.Key, data.Value);
                }
                else
                {
                    Debug.LogWarning($"{nameof(SerializedDictionary<TKey, TValue>)}: Already contains data for key {data.Key}. Please re-assign or remove elements with duplicate keys.");
                }
            }

            if (TryGetDictionaryVersion(out var version))
            {
                // Mark that we've processed this version of the dictionary data
                // so we don't immediately stomp additions in OnBeforeSerialize()
                _lastSerializedVersion = version;
            }
        }

        private bool TryGetDictionaryVersion(out int version)
        {
            // Perform version checking if we managed to retrieve our dictionary's internal version field
            if (_dictionaryVersionField != null)
            {
                // Unity calls OnBeforeSerialize() constantly when the Inspector is open
                // Don't bother updating anything if our internal dictionary hasn't actually changed
                version = (int) _dictionaryVersionField.GetValue(_dictionary);
                return true;
            }

            version = -1;
            return false;
        }

        private bool KeysEqual(TKey keyA, TKey keyB)
        {
            // Use EqualityComparer to perform type-specific comparison
            // This allows us to avoid boxing struct types, which regular object.Equals() performs
            return EqualityComparer<TKey>.Default.Equals(keyA, keyB);
        }
        
        #region Dictionary API
        
        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;
        
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }
        
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }
        
        public void Clear() => _dictionary.Clear();
        
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }
        
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
        
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => _dictionary.Add(item.Key, item.Value);
        
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
        }
        
        #endregion
        
        #region Editor

        public void EditorSortSerializedData()
        {
            // Trigger a re-serialize if our data has changed so that our serialized data is up to date
            OnBeforeSerialize();
            
            _serializedData.Sort((a, b) =>
            {
                // If the values are directly comparable, use that
                if (a.Key is IComparable<TKey> comparableA)
                {
                    return comparableA.CompareTo(b.Key);
                }
                
                // Otherwise, just compare our default labels
                return string.Compare(a.Key.ToString(), b.Key.ToString(), StringComparison.OrdinalIgnoreCase);
            });
        }
        
        #endregion
    }

    /// <summary>
    /// Interface for accessing type non-specific information from a serialized dictionary
    /// </summary>
    public interface ISerializedDictionary
    {
        public bool TryGetError(out string error);
        
        #region Editor
        
        public void EditorSortSerializedData();
        
        #endregion
    }

    /// <summary>
    /// Interface that types can implement if they wish to specify a custom label when used as the key in a SerializedDictionary
    /// </summary>
    public interface ISerializedDictionaryKey
    {
        public string GetElementLabel();
        public bool TryGetError(out string error);
    }

    /// <summary>
    /// Interface that types can implement if they wish to support custom search filtering when used as the value in a SerializedDictionary
    /// </summary>
    public interface ISerializedDictionaryValue
    {
        public bool MatchesFilter(string filter);
        public string GetElementLabelSuffix();
        public bool TryGetError(out string error);
    }
}
