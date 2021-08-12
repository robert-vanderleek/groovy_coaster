using System.Collections.Generic;
using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
	[System.Serializable]
	public enum BeatType
	{
		Single,
		TwoKey
	}

	[System.Serializable]
	public enum HitType
	{
		Miss,
		OK,
		Good,
		Perfect
	}

	public static string[] feedbackTexts = { "Miss!", "OK!", "Good!", "Perfect!" };
}
