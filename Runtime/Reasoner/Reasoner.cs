using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KOOP 
{    
    /// <summary>
    /// A Reasoner is intended to be used when a single counter is being modified by multiple different sources.
    /// You simply provide a string reason for the counter to be incremented, and the reasoner returns an ID, which you can later use to remove your count.
    /// This allows each component to manage its own reason for the counter to be incremented, and the string reason can be used for easy debugging.
    /// </summary>
    public class Reasoner : IEnumerable
    {
        [System.Serializable]
        public struct Reason
        {
            public Reason(string reason, int id) { this.reason = reason; this.id = id; }
    
            public int id;
            public string reason;
        }
    
        /// <summary>
        /// Invoked after reasons have been added or removed to result in the HasReasons() method return a different value than before.
        /// Provides a boolean that returns the value of HasReasons().
        /// </summary>
        public event Action<bool> OnHasReasonsChanged;
    
        [HideInInspector]
        public int lastUsedId = -1;
    
        public List<Reason> reasons = new List<Reason>();
    
        const string genericReason = "generic";
    
        public static Reasoner operator ++(Reasoner r) { r.Increment(); return r; }
        public static Reasoner operator --(Reasoner r) { r.Decrement(); return r; }
        public static implicit operator bool(Reasoner r) => r != null && r.HasReasons();
    
        public int AddReason(string newReason)
        {
            int newId = ++lastUsedId;
            reasons.Add(new Reason(newReason, newId));
            
            if (reasons.Count == 1)
                OnHasReasonsChanged?.Invoke(true);
    
            return newId;
        }
        
        public bool TryRemoveReason(int reasonID) => RemoveReason(reasonID, false);
        public bool RemoveReason(int reasonID, bool warnIfNotFound = true)
        {
            var resultIndex = reasons.FindIndex(x => x.id == reasonID);
            if (resultIndex == -1)
            {
                if(warnIfNotFound)
                    Debug.LogWarning($"Attempted to remove reason with ID {reasonID} but failed: Reason not found.");
                return false;
            }
    
            reasons.RemoveAt(resultIndex);
            
            if (!HasReasons())
                OnHasReasonsChanged?.Invoke(false);
            
            return true;
        }
    
        public void Increment()
        {
            reasons.Add(new Reason(genericReason, ++lastUsedId));
            
            if (reasons.Count == 1)
                OnHasReasonsChanged?.Invoke(true);
        }
    
        public void Decrement()
        {
            foreach (Reason reason in reasons)
            {
                if (reason.reason == genericReason)
                {
                    reasons.Remove(reason);
                    
                    if (!HasReasons())
                        OnHasReasonsChanged?.Invoke(false);
                    
                    break;
                }
            }
        }
    
        public bool HasReasons()
        {
            return reasons.Count > 0;
        }
    
        public void ClearReasons()
        {
            bool hadReasons = HasReasons();
            reasons.Clear();
            
            if (hadReasons)
                OnHasReasonsChanged?.Invoke(false);
        }
    
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)reasons).GetEnumerator();
        }
    
        public string GetReasonList()
        {
            string[] reasonStrings = new string[reasons.Count];
            for (int i = 0; i < reasons.Count; i++)
                reasonStrings[i] = reasons[i].reason;
            return string.Join(", ", reasonStrings);
        }
    }
    
    /// <summary>
    /// A version of the Reasoner with a generic parameter. 
    /// This makes it function more akin to a Stack, where the Reasoner has a current "Top", which is considered the current value.
    /// </summary>
    public class Reasoner<T> : IEnumerable<Reasoner<T>.Reason>
    {
        [Serializable]
        public class Reason
        {
            public Reason(string reason, int id, T value) { this.reason = reason; this.id = id; this.value = value; }
    
            public int id;
            public string reason;
            public T value;
        }
        
        /// <summary>
        /// Invoked after reasons have been added or removed to result in the HasReasons() method return a different value than before.
        /// Provides a boolean that returns the value of HasReasons().
        /// </summary>
        public event Action<bool> OnHasReasonsChanged;
    
        [HideInInspector]
        public int lastUsedId = -1;
    
        public List<Reason> reasons = new List<Reason>();
    
        const string genericReason = "generic";
    
        public static implicit operator T(Reasoner<T> r) => r.Value;
    
        public int Count => reasons.Count;
    
        public T Value 
        {
            get 
            {
                if (!HasReasons()) 
                {
                    return default(T);
                }
    
                return reasons[Count - 1].value;
            }
        }
    
        public int AddReason(T value) => AddReason(genericReason, value);
        public int AddReason(string newReason, T value)
        {
            int newId = ++lastUsedId;
            reasons.Add(new Reason(newReason, newId, value));
            
            if (reasons.Count == 1)
                OnHasReasonsChanged?.Invoke(true);
    
            return newId;
        }
    
        public bool SetReasonValue(int reasonID, T value)
        {
            var resultIndex = reasons.FindIndex(x => x.id == reasonID);
            if (resultIndex == -1)
            {
                Debug.LogWarning($"Attempted to set reason with ID {reasonID} but failed: Reason not found.");
                return false;
            }
    
            reasons[resultIndex].value = value;
    
            return true;
        }
        
        public bool TryRemoveReason(int reasonID) => RemoveReason(reasonID, false);
    
        public bool RemoveReason(int reasonID, bool warnIfNotFound = true)
        {
            var resultIndex = reasons.FindIndex(x => x.id == reasonID);
            if (resultIndex == -1)
            {
                if(warnIfNotFound)
                    Debug.LogWarning($"Attempted to remove reason with ID {reasonID} but failed: Reason not found.");
                return false;
            }
    
            reasons.RemoveAt(resultIndex);
            
            if (!HasReasons())
                OnHasReasonsChanged?.Invoke(false);
            
            return true;
        }
    
        public bool HasReasons()
        {
            return reasons.Count > 0;
        }
    
        public void ClearReasons()
        {
            bool hadReasons = HasReasons();
            reasons.Clear();
            
            if (hadReasons)
                OnHasReasonsChanged?.Invoke(false);
        }
        
        public string GetReasonListString()
        {
            if (!HasReasons())
                return "";
            
            string[] reasonStrings = new string[reasons.Count];
            for (int i = 0; i < reasons.Count; i++)
                reasonStrings[i] = reasons[i].reason;
            return string.Join(", ", reasonStrings);
        }
    
        public IEnumerator<Reason> GetEnumerator()
        {
            return ((IEnumerable<Reason>)reasons).GetEnumerator();
        }
    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)reasons).GetEnumerator();
        }
    }
}