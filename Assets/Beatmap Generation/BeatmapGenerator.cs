using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BeatmapGenerator : MonoBehaviour
{
	public AudioSource song;
	public string beatMapLocation;
	public string songName;

	public void CreateBeatmap(List<SpectralFluxInfo> info)
	{
		beatMapLocation = Application.dataPath + @"\Beatmaps";
		songName = song.clip.name;
		System.Random r = new System.Random();
		Beatmap map = new Beatmap
		{
			beats = new List<Beat>()
		};

		System.Array enumValues = System.Enum.GetValues(typeof(GlobalEnums.BeatType));
		foreach (SpectralFluxInfo peak in info)
		{
			if (peak.isPeak)
			{
				map.beats.Add(new Beat(peak.time, (GlobalEnums.BeatType)enumValues.GetValue(r.Next(0, enumValues.Length))));
			}
		}

		string jsonOfMap = JsonUtility.ToJson(map);
		string fileName = Path.Combine(beatMapLocation, songName + "map.json");
		if (File.Exists(fileName))
		{
			Debug.Log(fileName + " already exists.");
			return;
		}
		print("writing beatmap to file at path: " + fileName);
		var sr = File.CreateText(fileName);
		sr.Write(jsonOfMap);
		sr.Close();
	}
}
