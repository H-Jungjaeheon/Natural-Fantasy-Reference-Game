using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private State state;

    public Unit(State state)
    {
        this.state = state;
    }
    public void SetState(State state)
    {
        this.state = state;
    }
    public void Act()
    {
        state.NowAction();
    }
}
