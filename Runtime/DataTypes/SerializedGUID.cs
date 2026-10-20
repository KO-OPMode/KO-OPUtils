using System;
using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// A GUID wrapper that Unity can serialize to disk. This is a class instead of a struct
    /// since it contains double the payload of a regular GUID, so we don't want to copy it all over the place.
    /// </summary>
    /// <owner>jonagill</owner>
    [Serializable]
    public sealed class SerializedGuid :
        ISerializationCallbackReceiver,
        IComparable<SerializedGuid>,
        IEquatable<SerializedGuid>
    {
        [SerializeField]
        private byte[] _bytes;

        [NonSerialized]
        private Guid _value;

        public ref Guid Value => ref _value;
        
        public SerializedGuid(in Guid guid) => Value = guid;

        public static SerializedGuid NewEmpty() => new(Guid.Empty);

        public static SerializedGuid NewGuid() => new(Guid.NewGuid());

        public bool IsEmpty() => Value == Guid.Empty;

        public string ToBase64() => Value.EncodeBase64String();
        public override string ToString() => Value.ToString();

        #region IEquatable & IComparable

        public bool Equals(in Guid guid) => Value == guid;

        public bool Equals(SerializedGuid other) => other != null && Value == other.Value;

        public override bool Equals(object obj) => Equals(obj as SerializedGuid);

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(SerializedGuid other) => Value.CompareTo(other.Value);
        
#endregion
        
#region ISerializationCallbackReceiver
        
        void ISerializationCallbackReceiver.OnBeforeSerialize() => _bytes = _value.ToByteArray();

        void ISerializationCallbackReceiver.OnAfterDeserialize() => _value =
            ((_bytes == null || _bytes.Length == 0) ? Guid.Empty : new Guid(_bytes));
        
#endregion
    }
}