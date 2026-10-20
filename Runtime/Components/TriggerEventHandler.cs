using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple exposed event handler for OnTriggerEnter and OnTriggerExit.
/// </summary>
public class TriggerEventHandler : MonoBehaviour
{
    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider> {}

    public TriggerEvent TriggerEnterHandler;
    public TriggerEvent TriggerExitHandler;

    private void OnTriggerEnter(Collider other) => TriggerEnterHandler?.Invoke(other);
    private void OnTriggerExit(Collider other) => TriggerExitHandler?.Invoke(other);
}