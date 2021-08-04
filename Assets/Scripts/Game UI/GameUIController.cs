using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public TextMeshProUGUI scoreAndComboText;
    public TextMeshProUGUI feedbackText;
    private Coroutine fadeText;

    public GameObject blackOutSquare;
    private int fadeSpeed = 1;

    private static GameUIController _instance;

    public static GameUIController Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    
    public void UpdateScoreAndComboText()
	{
		scoreAndComboText.text = "Score: " + Scorer.GetScore() + "\nCombo: " + Scorer.GetCombo();
	}

	public void UpdateFeedbackText(GlobalEnums.HitType hitType)
	{
		if (fadeText != null)
			StopCoroutine(fadeText);

		RestoreFeedbackText();
		feedbackText.text = GlobalEnums.feedbackTexts[(int)hitType];
		fadeText = StartCoroutine(FadeFeedbackText(1f));
	}

	public IEnumerator FadeFeedbackText(float t)
	{
		feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, 1);
		while (feedbackText.color.a > 0.0f)
		{
			feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b, feedbackText.color.a - (Time.deltaTime / t));
			yield return null;
		}
	}

    public void RestoreFeedbackText()
    {
        feedbackText.color = new Color(feedbackText.color.r, feedbackText.color.g, feedbackText.color.b);
    }

    private IEnumerator FadeBlackOutSquare(bool fadeToBlack)
    {
        Color objColor = blackOutSquare.GetComponent<Image>().color;
        float fadeAmount;

        if (fadeToBlack)
        {
            objColor.a = 0;
            blackOutSquare.SetActive(true);
            while (blackOutSquare.GetComponent<Image>().color.a < 1)
            {
                fadeAmount = objColor.a + (fadeSpeed * Time.deltaTime);
                objColor = new Color(objColor.r, objColor.g, objColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objColor;
                yield return null;
            }
        }
        else
        {
            objColor.a = 1;
            while (blackOutSquare.GetComponent<Image>().color.a > 0)
            {
                fadeAmount = objColor.a - (fadeSpeed * Time.deltaTime);
                objColor = new Color(objColor.r, objColor.g, objColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objColor;
                yield return null;
            }
            blackOutSquare.SetActive(false);
        }
    }
}
