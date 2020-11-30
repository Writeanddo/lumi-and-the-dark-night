using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokeTriggerEvent : MonoBehaviour
{
    public UnityEvent triggeredEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggeredEvent != null)
        {
            triggeredEvent.Invoke();
        }
    }
}
