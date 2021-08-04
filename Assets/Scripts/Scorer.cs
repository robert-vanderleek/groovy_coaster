using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Scorer
{
    public static int score = 0;
    public static int combo = 1;

    public static int GetScore()
    {
        return score;
    }

    public static int GetCombo()
    {
        return combo;
    }

    public static int Hit(GlobalEnums.BeatType type, float? heldTime = null)
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

    public static void Miss()
    {
        ResetCombo();
    }

    private static void ResetCombo()
    {
        combo = 1;
    }
}
