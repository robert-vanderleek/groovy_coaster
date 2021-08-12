using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Orchestrator : MonoBehaviour
{
	public GameObject walker;
	public GameObject indicatorPrefab;
	public GameObject twoHitIndicatorPrefab;
	public GameObject failedPrefab;

	private Beatmap map;
	private string mapFilesPath;
	private string songName;
	private AudioClip songClip;
	private List<GameObject> indicators;
	private AudioSource audioSource;

	public KeyCode left;
	public KeyCode right;
	private float leftPrevHitTime;
	private float rightPrevHitTime;

	private bool songHasStarted = false;
	private bool songHasFinished = false;
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
		leftPrevHitTime = Time.time;
		rightPrevHitTime = Time.time;
		mapFilesPath = Application.dataPath + @"\Beatmaps";
		audioSource = GetComponent<AudioSource>();
		GameObject levelInfo = GameObject.Find("LevelInfo");
		indicators = new List<GameObject>();

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

		if (songHasFinished)
		{
			print("song ended");
			GameUIController.Instance.HandleEndOfSong();
			this.enabled = false;
			return;
		}

		if (map.beats[currIndicatorIndex].beatType == GlobalEnums.BeatType.TwoKey)
			UpdateDoubleHitTimes();

		//sync w/ audio source
		songTime += Time.time - previousFrameTime;
		previousFrameTime = Time.time;
		if (audioSource.time != lastReportedPlayheadPosition)
		{
			songTime = (songTime + audioSource.time) / 2;
			lastReportedPlayheadPosition = audioSource.time;
		}

		UpdateCurrentIndicator();
		
		if (Input.anyKeyDown)
		{
			float difference = Math.Abs(map.beats[currIndicatorIndex].time - songTime);
			GlobalEnums.BeatType currType = map.beats[currIndicatorIndex].beatType;

			switch (currType)
			{
				case GlobalEnums.BeatType.Single: //single hit beat, check we're within times and give points
					if (Input.GetKeyDown(left) || Input.GetKeyDown(right))
					{
						CalculateHit(difference, currType);
					}
					break;

				case GlobalEnums.BeatType.TwoKey: //double hit beat, check two keys down and give points
					//print($"left prev: {leftPrevHitTime} left diff {Time.time - leftPrevHitTime} right prev: {rightPrevHitTime} right diff {Time.time - rightPrevHitTime}");
					if (leftPrevHitTime > (Time.time - .1f) && rightPrevHitTime > (Time.time - .1f))
					{
						CalculateHit(difference, currType);
					}
					break;

				default:
					Debug.LogError("Unrecognized beat type: " + currType);
					break;
			}
		}
	}

	private void UpdateDoubleHitTimes()
	{
		if (Input.GetKeyDown(left))
		{
			leftPrevHitTime = Time.time;
		}

		if (Input.GetKeyDown(right))
		{
			rightPrevHitTime = Time.time;
		}
	}

	private void CalculateHit(float difference, GlobalEnums.BeatType beatType)
	{
		if (difference < perfectHit)
		{
			OnHitBeat(currIndicatorIndex, GlobalEnums.HitType.Perfect, GlobalEnums.BeatType.Single);
		}
		else if (difference < goodHit)
		{
			OnHitBeat(currIndicatorIndex, GlobalEnums.HitType.Good, GlobalEnums.BeatType.Single);
		}
		else if (difference < okHit)
		{
			OnHitBeat(currIndicatorIndex, GlobalEnums.HitType.OK, GlobalEnums.BeatType.Single);
		}
		else
		{
			OnMissBeat(currIndicatorIndex);
		}

		if (map.beats[currIndicatorIndex].beatType == GlobalEnums.BeatType.TwoKey)
			ResetHitTimes();
	}

	private void ResetHitTimes()
	{
		leftPrevHitTime = rightPrevHitTime = -1;
	}

	private void UpdateCurrentIndicator()
	{
		float timeOfCurrIndicator = map.beats[currIndicatorIndex].time;
		if (songTime > timeOfCurrIndicator + okHit && currIndicatorIndex + 1 <= map.beats.Count)
		{
			IncrementCurrIndicatorIndex();
		}

		if (currIndicatorIndex >= 1)
		{
			int tempIndex = currIndicatorIndex - 1;
			//print($"{tempIndex} has been hit or passed: {!map.beats[tempIndex].hasBeenPassed}, within lookback time: {map.beats[tempIndex].time > (songTime - lookBackTime)}");
			while (tempIndex >= 0 && !map.beats[tempIndex].hasBeenPassed)
			{
				//print($"{tempIndex} has not been hit");
				OnPassBeat(tempIndex);
				tempIndex--;
			}
		}
	}

	private void OnHitBeat(int index, GlobalEnums.HitType hitType, GlobalEnums.BeatType beatType)
	{
		int gainedScore = Scorer.Hit(GlobalEnums.BeatType.Single);
		map.beats[index].hasBeenPassed = true;
		indicators[index].transform.GetChild(0).GetComponent<ParticleSystem>().Play();
		indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
		GameUIController.Instance.UpdateScoreAndComboText();
		GameUIController.Instance.UpdateFeedbackText(hitType);
		IncrementCurrIndicatorIndex();
	}

	public void OnMissBeat(int index)
	{
		Scorer.Miss();
		//if the indicator is far enough ahead just count miss and leave indicator
		if (map.beats[index].time - songTime > 1f)
			return;

		map.beats[index].hasBeenPassed = true;
		indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
		GameObject failedX = GameObject.Instantiate(failedPrefab);
		failedX.transform.position = indicators[index].transform.position;
		Destroy(failedX, 1f);
		GameUIController.Instance.UpdateScoreAndComboText();
		GameUIController.Instance.UpdateFeedbackText(GlobalEnums.HitType.Miss);
		IncrementCurrIndicatorIndex();
	}

	public void OnPassBeat(int index)
	{
		Scorer.Miss();
		map.beats[index].hasBeenPassed = true;
		indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
		GameObject failedX = GameObject.Instantiate(failedPrefab);
		failedX.transform.position = indicators[index].transform.position;
		Destroy(failedX, 1f);
		GameUIController.Instance.UpdateScoreAndComboText();
		GameUIController.Instance.UpdateFeedbackText(GlobalEnums.HitType.Miss);
	}

	private void IncrementCurrIndicatorIndex()
	{
		currIndicatorIndex++;

		if (currIndicatorIndex >= map.beats.Count)
		{
			songHasFinished = true;
		}
	}

	private void SpawnBeatIndicators()
	{
		print(Application.dataPath);
		GameObject indicatorsParent = GameObject.Find("Indicators");
		GameObject indicator;
		LineWalker lineWalker = walker.GetComponent<LineWalker>();
		print("map contains " + map.beats.Count + " beats");
		for (int i = 0; i < map.beats.Count; i++)
		{
			//we have non-normalized time already, add normalized time
			map.beats[i].normalizedTime = map.beats[i].time / songClip.length;
			switch (map.beats[i].beatType)
			{
				case GlobalEnums.BeatType.Single:
					indicator = GameObject.Instantiate(indicatorPrefab);
					break;

				case GlobalEnums.BeatType.TwoKey:
					indicator = GameObject.Instantiate(twoHitIndicatorPrefab);
					break;

				default:
					//default to plain indicator so we don't fail
					indicator = GameObject.Instantiate(indicatorPrefab);
					Debug.LogError("Unkown beat type: " + (int)map.beats[i].beatType);
					break;
			}
			indicator.transform.position = lineWalker.GetPoint(map.beats[i].normalizedTime);
			indicator.transform.parent = indicatorsParent.transform;
			indicators.Add(indicator);
		}
	}
}
