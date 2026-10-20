using UnityEngine;
using UnityEngine.Events;
using System;

namespace KOOP
{
    [AddComponentMenu("Debug/Keybound Event")]
    public class KeyboundEvent : MonoBehaviour
    {
        [Flags]
        public enum Modifier
        {
            None = 0,
            Shift = 1,
            Ctrl = 2,
            Alt = 4,
        } 

        public KeyCode Key;
        public Modifier Modifiers;
        public UnityEvent EventHandler;

        private void Update()
        {
            if (!Input.GetKeyDown(Key))
                return;

            if (Modifiers.HasFlag(Modifier.Shift) != Input.GetKey(KeyCode.LeftShift))
                return;

            if (Modifiers.HasFlag(Modifier.Ctrl) != Input.GetKey(KeyCode.LeftControl))
                return;

            if (Modifiers.HasFlag(Modifier.Alt) != Input.GetKey(KeyCode.LeftAlt))
                return;

            EventHandler?.Invoke();
        }
    }
}