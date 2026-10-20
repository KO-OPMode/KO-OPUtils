using UnityCollections;
using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// Helper component that can prepend and append data to a GameObject's name at runtime
    /// </summary>
    public class GameObjectName : MonoBehaviour
    {
        
        private struct Token
        {
            public object Context;
            public string Text;
            public int Order;
        }
        
        private string _baseName;
        private PooledList<Token> _prefixes;
        private PooledList<Token> _suffixes;
        
        private void Awake()
        {
            _baseName = gameObject.name;
            EnsureLists();
        }

        private void OnDestroy()
        {
            _prefixes?.Dispose();
            _prefixes = null;
            
            _suffixes?.Dispose();
            _suffixes = null;
        }

        public void SetPrefix(object context, string prefix, int order = 0)
        {
            if (context == null)
            {
                Debug.LogError($"Must provide a context object to set a prefix.");
                return;
            }
            
            SetToken(context, prefix, order, ref _prefixes);
        }

        public void RemovePrefix(object context)
        {
            RemoveToken(context, ref _prefixes);
        }

        public void SetSuffix(object context, string suffix, int order = 0)
        {
            if (context == null)
            {
                Debug.LogError($"Must provide a context object to set a suffix.");
                return;
            }
            
            SetToken(context, suffix, order, ref _suffixes);
        }

        public void RemoveSuffix(object context)
        {
            RemoveToken(context, ref _suffixes);
        }

        private void SetToken(object context, string prefix, int order, ref PooledList<Token> tokens)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError($"{nameof(GameObjectName)} can only set tokens in Play mode.");
                return;
            }
            
            EnsureLists();

            var token = new Token()
            {
                Context = context,
                Text = prefix,
                Order = order
            };

            bool shouldAdd = true;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Context == context)
                {
                    // Update our token in place
                    tokens[i] = token;
                    shouldAdd = false;
                    break;
                }
            }

            if (shouldAdd)
            {
                tokens.Add(token);
            }
            
            SortList(tokens);
            RefreshName();
        }

        private void RemoveToken(object context, ref PooledList<Token> tokens)
        {
            if (tokens == null) return;

            bool removed = false;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Context == context)
                {
                    tokens.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (removed)
            {
                RefreshName();
            }
        }

        private void RefreshName()
        {
            using PooledList<string> tokens = new();
            foreach (var token in _prefixes)
            {
                tokens.Add(token.Text);
            }
            
            tokens.Add(_baseName);

            foreach (var token in _suffixes)
            {
                tokens.Add(token.Text);
            }

            gameObject.name = string.Join(" ", tokens);
        }

        private void EnsureLists()
        {
            _prefixes ??= new();
            _suffixes ??= new();
        }

        private void SortList(PooledList<Token> list)
        {
            list.BackingList.Sort((a, b) => a.Order.CompareTo(b.Order));
        }
    }
}