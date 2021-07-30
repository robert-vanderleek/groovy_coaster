using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Beat
{
    public GlobalEnums.BeatType beatType;
    public float normalizedTime;
    public float time;
    public bool hasBeenPassed = false;

    public Beat(float time, GlobalEnums.BeatType type)
    {
        this.time = time;
        this.beatType = type;
    }
}