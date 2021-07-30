using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorer
{
    public int score;
    public int combo = 1;

    public int GetScore()
    {
        return score;
    }

    public int GetCombo()
    {
        return combo;
    }

    public int Hit(GlobalEnums.BeatType type, float? heldTime = null)
    {
        //switch on type to score
        float points;

        switch (type)
        {
            case GlobalEnums.BeatType.Single:
                points = 500;
                break;

            case GlobalEnums.BeatType.TwoKey:
                points = 1000;
                break;

            case GlobalEnums.BeatType.Held:
                if (heldTime == null)
                {
                    Debug.LogError("no held time passed in!");
                    points = 500;
                }
                else
                {
                    points = 500 * (float)heldTime;
                }
                break;

            default:
                Debug.LogError("Unrecognized beat type!");
                points = 0;
                break;
        }
        int gainedPoints = combo * (int)points;
        score += gainedPoints;
        combo++;
        return gainedPoints;
    }

    public void Miss()
    {
        ResetCombo();
    }

    private void ResetCombo()
    {
        combo = 1;
    }
}
