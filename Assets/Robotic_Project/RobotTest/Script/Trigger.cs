using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Trigger : MonoBehaviour
{

    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit ;

    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }

    // Update is called once per frame
    void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke();
    }
}
