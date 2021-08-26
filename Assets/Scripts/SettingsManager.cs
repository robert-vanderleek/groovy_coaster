using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;
using TMPro;
using HSVPicker;

public class SettingsManager : MonoBehaviour
{
	public Slider volumeSlider;
	public TextMeshProUGUI keyAValue;
	public TextMeshProUGUI keyBValue;
	public ColorPicker colorPicker;
	public Settings settings;
	private string settingsPath;

	private void Update()
	{
		//LayoutRebuilder.ForceRebuildLayoutImmediate(colorPicker.transform.parent.GetComponent<RectTransform>());
	}

	private void Awake()
	{
		settingsPath = Application.persistentDataPath + "/settings.json";
		LoadSettings();
	}

	public void LoadSettings()
	{
		if (File.Exists(settingsPath))
		{
			string json = File.ReadAllText(settingsPath);
			print(json);
			settings = JsonConvert.DeserializeObject<Settings>(json);
		}
		else
		{
			settings = new Settings();
		}
	}

	public void PopulateSettingsMenu()
	{
		volumeSlider.value = settings.volume;
		keyAValue.text = settings.keyA.ToString();
		keyBValue.text = settings.keyB.ToString();
		colorPicker.CurrentColor = new Color(settings.r, settings.g, settings.b);
	}

	public void SaveSettingsAsJSON()
	{
		string saveFile = JsonConvert.SerializeObject(settings);
		Debug.Log("Saving to path: " + settingsPath + " json: " + saveFile);
		File.WriteAllText(settingsPath, saveFile);
	}
}
