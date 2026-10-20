using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace KOOP
{
    [RequireComponent(typeof(KeyboundEvent))]
    public abstract class KeyboundEventHandler : MonoBehaviour
    {
        #if UNITY_EDITOR
        private void Reset()
        {
            KeyboundEvent keyboundEvent = GetComponent<KeyboundEvent>();
            if (keyboundEvent == null)
            {
                return;
            }

            if (keyboundEvent.EventHandler == null)
                keyboundEvent.EventHandler = new();

            UnityEventTools.RemovePersistentListener(keyboundEvent.EventHandler, EventMethod);
            UnityEventTools.AddPersistentListener(keyboundEvent.EventHandler, EventMethod);
        }
        #endif

        protected abstract void EventMethod();
    }
}