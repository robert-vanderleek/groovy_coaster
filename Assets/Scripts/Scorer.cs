using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Scorer
{
    public static int score = 0;
    public static int combo = 1;
    public static int longestCombo = int.MinValue;

    public static int GetScore()
    {
        return score;
    }

    public static int GetCombo()
    {
        return combo;
    }

    public static int Hit(GlobalEnums.BeatType type)
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
        //check if new longest combo
        if (combo > longestCombo)
            longestCombo = combo;

        ResetCombo();
    }

    private static void ResetCombo()
    {
        combo = 1;
    }
}
