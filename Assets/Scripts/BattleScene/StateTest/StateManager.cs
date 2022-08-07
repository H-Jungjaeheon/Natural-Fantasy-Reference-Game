using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    void Start()
    {
        Unit unit = new Unit(new Jump());
        unit.Act();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
