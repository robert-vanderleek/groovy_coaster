using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
    [System.Serializable]
    public enum BeatType
    {
        Single,
        TwoKey,
        Held
    }

    [System.Serializable]
    public enum HitType
    {
        OK,
        Good,
        Perfect
    }
}
