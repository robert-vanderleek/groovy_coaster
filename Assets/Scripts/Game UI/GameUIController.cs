using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
	//general UI objects 
	public GameObject UICountDown;
	public TextMeshProUGUI scoreAndComboText;
	public TextMeshProUGUI feedbackText;

	//end of game UI
	public GameObject endGameUIParent;
	public TextMeshProUGUI finalScoreText;
	public TextMeshProUGUI longestComboText;
	public TextMeshProUGUI songInfoText;

	//coroutine used to fade feedback text, tracked so we can interrupt before it's done
	private Coroutine fadeText;

	//used to fade to black
	public GameObject blackOutSquare;
	private float fadeSpeed = .2f;

	//signal orchestrator that countdown is done
	public delegate void OnCountDownEnd();
	public event OnCountDownEnd countDownEnd;

	public static GameUIController Instance { get { return _instance; } }
	private static GameUIController _instance;

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

	public IEnumerator ShowCountDown()
	{
		TextMeshProUGUI text = UICountDown.GetComponent<TextMeshProUGUI>();
		UICountDown.SetActive(true);

		for (int i = 3; i >= 0; i--)
		{
			if (i == 0)
			{
				text.text = "Go!";
			}
			else
			{
				text.text = i.ToString();
			}
			yield return new WaitForSecondsRealtime(1f);
		}
		countDownEnd?.Invoke();
		UICountDown.SetActive(false);
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

	public void HandleEndOfSong()
	{
		//fade to black, keeping score/combo on screen cuz it's fun
		endGameUIParent.SetActive(true);
		StartCoroutine(FadeBlackOutSquare());
		scoreAndComboText.enabled = false;
		longestComboText.text += Scorer.longestCombo;
		finalScoreText.text += Scorer.GetScore();
		songInfoText.text = GameObject.Find("Orchestrator").GetComponent<AudioSource>().clip.name;
	}

	public void ReturnToMainMenu()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}

	private IEnumerator FadeBlackOutSquare()
	{
		Color objColor = blackOutSquare.GetComponent<Image>().color;
		float fadeAmount;

		objColor.a = 0;
		blackOutSquare.SetActive(true);
		while (blackOutSquare.GetComponent<Image>().color.a < 1)
		{
			fadeAmount = objColor.a + (fadeSpeed * Time.deltaTime);
			print(fadeAmount + " " + fadeSpeed * Time.deltaTime);
			objColor = new Color(objColor.r, objColor.g, objColor.b, fadeAmount);
			blackOutSquare.GetComponent<Image>().color = objColor;
			yield return null;
		}

	}
}
