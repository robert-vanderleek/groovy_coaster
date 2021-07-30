using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;

public class Orchestrator : MonoBehaviour
{
    public GameObject walker;
    public GameObject UICountDown;
    public GameObject indicatorPrefab;
    public GameObject failedPrefab;
    public TextMeshProUGUI scoreAndComboText;

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

    private Scorer scorer;
    private bool songHasStarted = false;
    private float previousFrameTime;
    private float lastReportedPlayheadPosition;
    private float songTime;
    private int currIndicatorIndex = 0;

    private float okHit = .15f;
    private float goodHit = .1f;
    private float perfectHit = .05f;


    // Start is called before the first frame update
    void Start()
    {
        scorer = new Scorer();
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
        UpdateScoreAndComboText();
        StartCoroutine(BeginCountDownAndPlay());
    }

    private IEnumerator BeginCountDownAndPlay()
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
        SongStarted();
    }

    private void SongStarted()
    {
        UICountDown.SetActive(false);
        previousFrameTime = Time.time;
        lastReportedPlayheadPosition = 0;
        audioSource.Play();
        songHasStarted = true;
        walker.GetComponent<LineWalker>().SetReady();
    }

    // Update is called once per frame
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

        //update which indicator we are looking at
        UpdateCurrentIndicator();
        
        if (Input.GetKeyDown(KeyCode.F))
        {

            if (map.beats[currIndicatorIndex].time < (songTime + okHit) && map.beats[currIndicatorIndex].time > (songTime - okHit))
            {
                OnHitBeat(currIndicatorIndex);
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

                case GlobalEnums.BeatType.Held: //maybe check for anykey at the top and then keydown on the other two?
                    break;

                default:
                    Debug.LogError("Unrecognized beat type: " + currType);
                    break;
            }
        }
    }

    private void UpdateScoreAndComboText()
    {
        scoreAndComboText.text = "Score: " + scorer.GetScore() + "\nCombo: " + scorer.GetCombo();
    }

    private void UpdateCurrentIndicator()
    { 
        float timeOfCurrIndicator = map.beats[currIndicatorIndex].time;
        if (songTime > timeOfCurrIndicator + okHit)
        {
            currIndicatorIndex++;
        }

        if (currIndicatorIndex >= 1)
        {
            int tempIndex = currIndicatorIndex - 1;
            //print($"{tempIndex} has been hit or passed: {!map.beats[tempIndex].hasBeenPassed}, within lookback time: {map.beats[tempIndex].time > (songTime - lookBackTime)}");
            while (!map.beats[tempIndex].hasBeenPassed)
            {
                //print($"{tempIndex} has not been hit");
                OnMissBeat(tempIndex);
                tempIndex--;
            }
        }
    }

    private void OnHitBeat(int index)
    {
        int gainedScore = scorer.Hit(GlobalEnums.BeatType.Single);
        map.beats[index].hasBeenPassed = true;
        indicators[index].transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        indicators[index].transform.GetChild(1).GetComponent<ParticleSystem>().Play();
        //play good sound/particle effect
        indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
        UpdateScoreAndComboText();
    }

    public void OnMissBeat(int index)
    {
        scorer.Miss();
        //need to check if it's close enough to destroy or just ignore input?
        map.beats[index].hasBeenPassed = true;
        indicators[index].GetComponent<SpriteRenderer>().forceRenderingOff = true;
        GameObject failedX = GameObject.Instantiate(failedPrefab);
        failedX.transform.position = indicators[index].transform.position;
        Destroy(failedX, 1f);
        UpdateScoreAndComboText();
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
