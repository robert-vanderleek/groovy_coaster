using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;

public class Orchestrator : MonoBehaviour
{
	public GameObject walker;
	public GameObject indicatorPrefab;
	public GameObject failedPrefab;

	private Beatmap map;
	private string mapFilesPath = @"C:\Users\Robert-Desktop\Rythym Game\Assets\Beatmaps";
	private string songName;
	private AudioClip songClip;
	private List<GameObject> indicators;
	private AudioSource audioSource;

	public KeyCode rightKey;
	public KeyCode leftKey;
	public KeyCode downKey;
	public KeyCode upKey;

	private bool songHasStarted = false;
	private float previousFrameTime;
	private float lastReportedPlayheadPosition;
	private float songTime;
	private int currIndicatorIndex = 0;

	private float okHit = .15f;
	private float goodHit = .1f;
	private float perfectHit = .05f;

	void Start()
	{
		Application.targetFrameRate = 144;
		audioSource = GetComponent<AudioSource>();
		GameObject levelInfo = GameObject.Find("LevelInfo");

		if (levelInfo != null)
		{
			songClip = levelInfo.GetComponent<AudioSource>().clip;
			audioSource.clip = songClip;
			print("found level info, loading from dynamic clip: " + songClip.name);
		}
		else
		{
			songClip = audioSource.clip;
			print("level info is null, loading from pre-determined clip: " + songClip.name);
		}
		indicators = new List<GameObject>();
		songName = songClip.name + "map";
		string jsonMap = File.ReadAllText(Path.Combine(mapFilesPath, songName + ".json"));
		print("loading beatmap from: " + Path.Combine(mapFilesPath, songName + ".json"));
		map = JsonUtility.FromJson<Beatmap>(jsonMap);
		walker.GetComponent<LineWalker>().Init();
		SpawnBeatIndicators();
		GetComponent<LineDrawer>().SetPoints(walker.GetComponent<LineWalker>().points);
		GameUIController.Instance.UpdateScoreAndComboText();
        GameUIController.Instance.countDownEnd += SongStarted;
		StartCoroutine(GameUIController.Instance.ShowCountDown());
	}

	private void SongStarted()
	{
		previousFrameTime = Time.time;
		lastReportedPlayheadPosition = 0;
		audioSource.Play();
		songHasStarted = true;
		walker.GetComponent<LineWalker>().SetReady();
	}

	void Update()
	{
		if (!songHasStarted)
			return; 

		songTime += Time.time - previousFrameTime;
		previousFrameTime = Time.time;
		if (audioSource.time != lastReportedPlayheadPosition)
		{
			songTime = (songTime + audioSource.time) / 2;
			lastReportedPlayheadPosition = audioSource.time;
		}

		if (currIndicatorIndex > map.beats.Count - 1)
        {
			GameUIController.Instance.HandleEndOfSong();
			return;
        }

		//update which indicator we are looking at
		UpdateCurrentIndicator();
		
		if (Input.GetKeyDown(KeyCode.F))
		{
			float difference = Math.Abs(map.beats[currIndicatorIndex].time - songTime);

			if (difference < perfectHit)
			{
				OnHitBeat(currIndicatorIndex, GlobalEnums.HitType.Perfect, GlobalEnums.BeatType.Single);
				currIndicatorIndex++;
			}
			else if (difference < goodHit)
			{
				OnHitBeat(currIndicatorIndex, GlobalEnums.HitType.Good, GlobalEnums.BeatType.Single);
				currIndicatorIndex++;
			}
			else if (difference < okHit)
			{
				OnHitBeat(currIndicatorIndex, GlobalEnums.HitType.OK, GlobalEnums.BeatType.Single);
				currIndicatorIndex++;
			}
			else
			{
				OnMissBeat(currIndicatorIndex);
				currIndicatorIndex++;
			}
			return;

			GlobalEnums.BeatType currType = map.beats[currIndicatorIndex].beatType;

			switch (currType)
			{
				case GlobalEnums.BeatType.Single: //single hit beat, check we're within times and give points
					break;

				case GlobalEnums.BeatType.TwoKey: //double hit beat, check two keys down and give points
					break;

				default:
					Debug.LogError("Unrecognized beat type: " + currType);
					break;
			}
		}
	}

	private void UpdateCurrentIndicator()
	{
		float timeOfCurrIndicator = map.beats[currIndicatorIndex].time;
		if (songTime > timeOfCurrIndicator + okHit && currIndicatorIndex + 1 <= map.beats.Count)
		{
			currIndicatorIndex++;
		}

		if (currIndicatorIndex >= 1)
		{
			int tempIndex = currIndicatorIndex - 1;
			//print($"{tempIndex} has been hit or passed: {!map.beats[tempIndex].hasBeenPassed}, within lookback time: {map.beats[tempIndex].time > (songTime - lookBackTime)}");
			while (tempIndex >= 0 && !map.beats[tempIndex].hasBeenPassed)
			{
				//print($"{tempIndex} has not been hit");
				OnMissBeat(tempIndex);
				tempIndex--;
			}
		}
	}

	private void OnHitBeat(int index, GlobalEnums.HitType hitType, GlobalEnums.BeatType beatType)
	{
		int gainedScore = Scorer.Hit(GlobalEnums.BeatType.Single);
		map.beats[index].hasBeenPassed = true;
		indicators[index].transform.GetChild(0).GetComponent<ParticleSystem>().Play();
		indicators[index].transform.GetChild(1).GetComponent<ParticleSystem>().Play();
		//play good sound/particle effect
		indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
		GameUIController.Instance.UpdateScoreAndComboText();
		GameUIController.Instance.UpdateFeedbackText(hitType);
	}

	public void OnMissBeat(int index)
	{
		Scorer.Miss();
		//need to check if it's close enough to destroy or just ignore input?
		map.beats[index].hasBeenPassed = true;
		indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
		GameObject failedX = GameObject.Instantiate(failedPrefab);
		failedX.transform.position = indicators[index].transform.position;
		Destroy(failedX, 1f);
		GameUIController.Instance.UpdateScoreAndComboText();
		GameUIController.Instance.UpdateFeedbackText(GlobalEnums.HitType.Miss);
	}

	private void SpawnBeatIndicators()
	{
		GameObject indicatorsParent = GameObject.Find("Indicators");
		GameObject indicator;
		LineWalker lineWalker = walker.GetComponent<LineWalker>();
		print("map contains " + map.beats.Count + " beats");
		for (int i = 0; i < map.beats.Count; i++)
		{
			//we have non-normalized time already, add normalized time
			map.beats[i].normalizedTime = map.beats[i].time / songClip.length;
			indicator = GameObject.Instantiate(indicatorPrefab);
			indicator.transform.position = lineWalker.GetPoint(map.beats[i].normalizedTime);
			indicator.transform.parent = indicatorsParent.transform;
			indicators.Add(indicator);
		}
	}
}
