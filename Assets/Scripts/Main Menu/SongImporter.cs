using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DSPLib;
using System.Numerics;
using System;
using UnityEngine.Networking;

public class SongImporter : MonoBehaviour
{
	public MainMenuHandler mainMenuHandler;
	private string musicFilesPath;
	private string beatMapLocation;
	private string songName;
	private int numChannels;
	private int numTotalSamples;
	private int sampleRate;
	private float[] multiChannelSamples;
	private SpectralFluxAnalyzer preProcessedSpectralFluxAnalyzer;
	private AudioSource audioSource;

	private void Start()
	{
		beatMapLocation = Application.dataPath + @"\Beatmaps";
		musicFilesPath = Application.dataPath + @"\Music";
		audioSource = GetComponent<AudioSource>();
		preProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer();
	}

	public void GetClipInfo()
	{
		// Need all audio samples.  If in stereo, samples will return with left and right channels interweaved
		// [L,R,L,R,L,R]
		multiChannelSamples = new float[audioSource.clip.samples * audioSource.clip.channels];
		numChannels = audioSource.clip.channels;
		numTotalSamples = audioSource.clip.samples;

		// We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
		this.sampleRate = audioSource.clip.frequency;

		audioSource.clip.GetData(multiChannelSamples, 0);
	}

	public void ImportSong()
	{
		StartCoroutine(ShowSongSelectDialog());
	}

	private IEnumerator ShowSongSelectDialog()
	{
		FileBrowser.SetDefaultFilter(".mp3");
		FileBrowser.AddQuickLink("User", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select Song File", "Select");
		if (!FileBrowser.Success)
		{
			print("please select a song file");
			yield break;
		}
		//copy to music folder so we have it for future reference
		string originalFile = FileBrowser.Result[0];
		string songName = Path.GetFileNameWithoutExtension(originalFile);
		string newFile = Path.Combine(musicFilesPath, songName + ".mp3");
		try
		{
			File.Copy(originalFile, newFile);
		}
		catch (IOException e)
		{
			print("failed to copy. exception details: " + e.ToString());
			yield break;
		}
		catch (Exception e)
		{
			print("unkown error. exception details: " + e.ToString());
			yield break;
		}
		//load song w/ web request into our audio source
		print("attempting to load copied file at: " + newFile);
		using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(newFile, AudioType.MPEG))
		{
			yield return req.SendWebRequest();
			audioSource.clip = DownloadHandlerAudioClip.GetContent(req);
			audioSource.clip.name = songName;
		}
		GetClipInfo();
		getFullSpectrumThreaded();
		print("Song successfully added");
		mainMenuHandler.AddSong(audioSource.clip);
	}

	public void getFullSpectrumThreaded()
	{
		try
		{
			// We only need to retain the samples for combined channels over the time domain
			float[] preProcessedSamples = new float[this.numTotalSamples];

			int numProcessed = 0;
			float combinedChannelAverage = 0f;
			for (int i = 0; i < multiChannelSamples.Length; i++)
			{
				combinedChannelAverage += multiChannelSamples[i];

				// Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
				if ((i + 1) % this.numChannels == 0)
				{
					preProcessedSamples[numProcessed] = combinedChannelAverage / this.numChannels;
					numProcessed++;
					combinedChannelAverage = 0f;
				}
			}

			Debug.Log("Combine Channels done");

			// Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
			int spectrumSampleSize = 1024;
			int iterations = preProcessedSamples.Length / spectrumSampleSize;

			FFT fft = new FFT();
			fft.Initialize((UInt32)spectrumSampleSize);

			Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
			double[] sampleChunk = new double[spectrumSampleSize];
			List<float> times = new List<float>();
			for (int i = 0; i < iterations; i++)
			{
				// Grab the current 1024 chunk of audio sample data
				Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

				// Apply our chosen FFT Window
				double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
				double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
				double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

				// Perform the FFT and convert output (complex numbers) to Magnitude
				Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
				double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
				scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

				// These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
				float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

				// Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
				times.Add(curSongTime);
				preProcessedSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 1.6f);
			}
			if (preProcessedSpectralFluxAnalyzer.spectralFluxSamples == null)
			{
				print("null samples!");
				return;
			}
			CreateBeatmap(preProcessedSpectralFluxAnalyzer.spectralFluxSamples);
			Debug.Log("Spectrum Analysis done");
		}
		catch (Exception e)
		{
			// Catch exceptions here since the background thread won't always surface the exception to the main thread
			Debug.Log(e.ToString());
		}
	}

	public void CreateBeatmap(List<SpectralFluxInfo> info)
	{
		songName = audioSource.clip.name;
		System.Random r = new System.Random();
		Beatmap map = new Beatmap
		{
			beats = new List<Beat>()
		};

		Array enumValues = Enum.GetValues(typeof(GlobalEnums.BeatType));
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

	public float getTimeFromIndex(int index)
	{
		return ((1f / (float)this.sampleRate) * index);
	}
}
