using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USBPickedTrigger : MonoBehaviour
{
    public void ActivateUSBTrigger()
    {
        GameEvent.OnUSBPicked?.Invoke();

        Debug.Log("get key, frenzymode on");
    }
}
