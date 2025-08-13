
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TriggerCollider : MonoBehaviour
{
    public UnityEvent UnityEvents;
    private void OnTriggerEnter(Collider other)
    {
        UnityEvents?.Invoke();
    }
}