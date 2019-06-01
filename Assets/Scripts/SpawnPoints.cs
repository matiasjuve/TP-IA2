using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnPoints : MonoBehaviour
{
    public static SpawnPoints Instance;
    public List<Transform> spawnPoints = new List<Transform>();

    public void Start()
    {
        
        {

        }
        Instance = this;
    }
}
