using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using HSVPicker;

public class MainMenuHandler : MonoBehaviour
{
	public SettingsManager settingsManager;
	public GameObject mainButtonsUIParent;
	public GameObject levelSelectUIParent;
	public GameObject settingsUIParent;
	public GameObject songPrefab;
	public GameObject scrollContentObj;
	public GameObject levelInfo;
	public GameObject beatmapGenerator;
	private string musicFilesPath;
	private List<AudioClip> clips;

	private void Awake()
	{
		Application.targetFrameRate = 144;
		clips = new List<AudioClip>();
	}

	private void Start()
	{
		musicFilesPath = Application.dataPath + @"\Music";
		DontDestroyOnLoad(levelInfo);
		PopulateSettings();
		StartCoroutine(GetClipList());
	}

	private void PopulateSettings()
	{
		settingsManager.PopulateSettingsMenu();

	}

	private void PopulateAvailableLevels()
	{
		GameObject songUIPanel;
		string songName;
		ToggleGroup toggleGroup = scrollContentObj.GetComponent<ToggleGroup>();
		for (int i = 0; i < clips.Count; i++)
		{
			print("Adding clip " + clips[i].name + " to panel + " + i);
			songUIPanel = GameObject.Instantiate(songPrefab);
			songUIPanel.transform.position = Vector3.zero;
			songUIPanel.name = clips[i].name;
			songUIPanel.transform.SetParent(scrollContentObj.transform, false);
			songName = clips[i].name.Split('-')[0];
			songUIPanel.GetComponentInChildren<TextMeshProUGUI>().text = songName + " - " + clips[i].length;
			//have to declare toggle inside loop so that the delegate doesn't just use the most recent reference
			Toggle toggle = songUIPanel.GetComponentInChildren<Toggle>();
			toggle.group = toggleGroup;
			toggle.onValueChanged.AddListener(delegate
			{
				OnToggleChange(toggle);
			});
			songUIPanel.GetComponentInChildren<AudioSource>().clip = clips[i];

			if (i == 0)
				toggle.isOn = true;
		}
		levelInfo.GetComponent<AudioSource>().clip = clips[0];
	}

	private IEnumerator GetClipList()
	{
		string songName;
		foreach (string file in Directory.EnumerateFiles(musicFilesPath, "*.mp3"))
		{
			songName = Path.GetFileNameWithoutExtension(file);
			using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(file, AudioType.MPEG))
			{
				yield return req.SendWebRequest();
				AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
				clip.name = songName;
				clips.Add(clip);
			}
		}
		PopulateAvailableLevels();
	}

	public void AddSong(AudioClip clip)
	{
		ToggleGroup toggleGroup = scrollContentObj.GetComponent<ToggleGroup>();
		GameObject songUIPanel = GameObject.Instantiate(songPrefab);
		songUIPanel.transform.position = Vector3.zero;
		songUIPanel.name = clip.name;
		string songName = clip.name.Split('-')[0];
		songUIPanel.transform.SetParent(scrollContentObj.transform, false);
		songUIPanel.GetComponentInChildren<TextMeshProUGUI>().text = songName + " - " + clip.length;
		Toggle toggle = songUIPanel.GetComponentInChildren<Toggle>();
		toggle.group = toggleGroup;
		toggle.onValueChanged.AddListener(delegate
		{
			OnToggleChange(toggle);
		});
	}

	public void OnToggleChange(Toggle toggle)
	{
		if (toggle.isOn)
		{
			levelInfo.GetComponent<AudioSource>().clip = toggle.transform.parent.GetComponent<AudioSource>().clip;
		}
	}

	public void OnStartButton()
	{
		scrollContentObj.GetComponent<ToggleGroup>().GetFirstActiveToggle();
		SceneManager.LoadScene(1);
	}

	public void OnLevelSelectBackButton()
	{
		mainButtonsUIParent.SetActive(true);
		levelSelectUIParent.SetActive(false);
	}

	public void OnPlayButton()
	{
		levelSelectUIParent.SetActive(true);
		mainButtonsUIParent.SetActive(false);
	}

	public void OnSettingsButton()
	{
		mainButtonsUIParent.SetActive(false);
		settingsUIParent.SetActive(true);
		//LayoutRebuilder.ForceRebuildLayoutImmediate(GameObject.Find("Color").GetComponent<RectTransform>());
	}

	public void OnSettingsBackButton()
	{
		settingsManager.SaveSettingsAsJSON();
		settingsUIParent.SetActive(false);
		mainButtonsUIParent.SetActive(true);
	}

	public void OnVolumeSliderChanged(Slider slider)
	{
		settingsManager.settings.volume = slider.value;
	}

	public void OnKeyAButtonRebind()
	{
		StartCoroutine(AwaitKeypressA());
	}

	public void OnKeyBButtonRebind()
	{
		StartCoroutine(AwaitKeypressB());
	}

	public IEnumerator AwaitKeypressA()
	{
		while (!Input.anyKeyDown)
		{
			yield return null;
		}

		foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKey(kcode))
			{
				print("keycode: " + kcode);
				settingsManager.settings.keyA = kcode;
				settingsManager.PopulateSettingsMenu();
				yield break;
			}
		}
	}

	public IEnumerator AwaitKeypressB()
	{
		while (!Input.anyKeyDown)
		{
			yield return null;
		}

		foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKey(kcode))
			{
				print("keycode: " + kcode);
				settingsManager.settings.keyB = kcode;
				settingsManager.PopulateSettingsMenu();
				yield break;
			}
		}
	}

	public void OnColorChange(ColorPicker picker)
	{
		settingsManager.settings.r = picker.R;
		settingsManager.settings.g = picker.G;
		settingsManager.settings.b = picker.B;
	}

	public void OnQuitButton()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
	  Application.Quit();
#endif
	}
}
