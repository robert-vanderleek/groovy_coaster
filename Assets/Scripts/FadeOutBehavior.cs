using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutBehavior : MonoBehaviour
{
    public GameObject blackOutSquare;
    private int fadeSpeed = 1;

    public IEnumerator FadeBlackOutSquare(bool fadeToBlack)
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
