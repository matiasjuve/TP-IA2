using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class NamePosition : MonoBehaviour
{
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);
        transform.rotation = Quaternion.LookRotation(transform.position - target.position);
    }

    public void LookCamera()
    {
        
    }
}
