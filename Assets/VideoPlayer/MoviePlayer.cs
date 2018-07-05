#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class MoviePlayer : MonoBehaviour, IPointerClickHandler
{
	public RawImage targetImage;
	public bool enableLoop;
	public bool enableSkip;

	private VideoClip videoClipToPlay;
	private string fileURL;

	private VideoPlayer videoPlayer;	
	private AudioSource audioSource;
	private Action onEndMovie;

	private void Awake()
	{
		//Add VideoPlayer to the GameObject
		videoPlayer = gameObject.AddComponent<VideoPlayer>();

		//Add AudioSource
		audioSource = gameObject.AddComponent<AudioSource>();

		InitMoviePlayer();
	}

	private void InitMoviePlayer()
	{
		enableLoop = false;
		enableSkip = false;

		//Disable Play on Awake for both Video and Audio
		videoPlayer.playOnAwake = false;
		audioSource.playOnAwake = false;
		audioSource.Pause();

		videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
				
		videoPlayer.started += VideoPlayer_started;
		videoPlayer.loopPointReached += VideoPlayer_loopPointReached;		
	}

	#region Callback Events

	public void OnPointerClick(PointerEventData eventData)
	{
		if (enableSkip)
		{
			videoPlayer.Stop();
		}
	}

	private void VideoPlayer_started(VideoPlayer source)
	{
		Debug.Log("Start Movie");
	}

	private void VideoPlayer_loopPointReached(VideoPlayer source)
	{
		Debug.Log("On Reached Looping Point To Movie");
	}

	private void VideoPlayer_endMovie()
	{
		Debug.Log("End Movie");
		if (onEndMovie != null)
			onEndMovie();

		onEndMovie = null;
	}

	#endregion

	#region Play Movie

	public void PlayMovie(VideoClip _clip, bool _isSkip, bool _isLoop = false, Action _onEnded = null)
	{
		videoClipToPlay = _clip;

		enableLoop = _isLoop;
		enableSkip = _isSkip;

		videoPlayer.isLooping = enableLoop;

		StartCoroutine(CoPlayMovieFromVideoClip(videoClipToPlay, _onEnded));
	}

	private IEnumerator CoPlayMovieFromVideoClip(VideoClip clip, Action onEnded)
	{
		onEndMovie = onEnded;

		videoPlayer.source = VideoSource.VideoClip;
		videoPlayer.url = null;

		//Set Audio Output to AudioSource
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		//Assign the Audio from Video to AudioSource to be played
		videoPlayer.EnableAudioTrack(0, true);
		videoPlayer.SetTargetAudioSource(0, audioSource);

		//Set video To Play then prepare Audio to prevent Buffering
		videoPlayer.clip = videoClipToPlay;
		videoPlayer.Prepare();

		//Wait until video is prepared
		WaitForSeconds waitTime = new WaitForSeconds(1);
		while (!videoPlayer.isPrepared)
		{			
			//Prepare/Wait for 5 sceonds only
			yield return waitTime;
			//Break out of the while loop after 5 seconds wait
			break;
		}
		Debug.Log("Done Preparing Video");

		videoPlayer.controlledAudioTrackCount = videoPlayer.audioTrackCount;

		//Play Video
		videoPlayer.Play();

		//Play Sound
		audioSource.Play();

		while (videoPlayer.texture == null)
			yield return null;
		Debug.Log("Done Prepared display texture");

		//Assign the Texture from Video to RawImage to be displayed
		targetImage.texture = videoPlayer.texture;
		
		Debug.Log("Playing Video");

		while (videoPlayer.isPlaying)
			yield return null;

		Debug.Log("Done Playing Video");

		VideoPlayer_endMovie();
		
	}

	public void PlayMovie(string _filePath, bool _isSkip, bool _isLoop = false, Action _onEnded = null)
	{
		fileURL = string.Format("file://{0}", _filePath);
		enableLoop = _isLoop;
		enableSkip = _isSkip;

		videoPlayer.isLooping = enableLoop;		
		StartCoroutine(CoPlayMovieFromVideoFile(fileURL, _onEnded));
	}

	private IEnumerator CoPlayMovieFromVideoFile(string fileUrl, Action onEnded)
	{
		onEndMovie = onEnded;

		videoPlayer.source = VideoSource.Url;
		videoPlayer.url = fileUrl;
		videoPlayer.clip = null;

		//Set Audio Output to AudioSource
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		//Assign the Audio from Video to AudioSource to be played
		videoPlayer.EnableAudioTrack(0, true);
		videoPlayer.SetTargetAudioSource(0, audioSource);

		//Set video To Play then prepare Audio to prevent Buffering		
		videoPlayer.Prepare();

		//Wait until video is prepared
		WaitForSeconds waitTime = new WaitForSeconds(1);
		while (!videoPlayer.isPrepared)
		{
			//Prepare/Wait for 5 sceonds only
			yield return waitTime;
			//Break out of the while loop after 5 seconds wait
			break;
		}
		Debug.Log("Done Preparing Video");

		videoPlayer.controlledAudioTrackCount = videoPlayer.audioTrackCount;

		//Play Video
		videoPlayer.Play();

		//Play Sound
		audioSource.Play();

		while (videoPlayer.texture == null)
			yield return null;
		Debug.Log("Done Prepared display texture");

		//Assign the Texture from Video to RawImage to be displayed
		targetImage.texture = videoPlayer.texture;
		
		Debug.Log("Playing Video");

		while (videoPlayer.isPlaying)
			yield return null;

		Debug.Log("Done Playing Video");

		VideoPlayer_endMovie();
	}
	
	#endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(MoviePlayer))]
public class MoviePlayerEditor : Editor
{
	string sampleFilePath = "";
	VideoClip sampleVideoClip;
	bool isSkip = false;
	bool isLoop = false;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("[Debug Test]");
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical();
		{
			sampleFilePath = EditorGUILayout.TextField("URL", sampleFilePath);
			sampleVideoClip = (VideoClip)EditorGUILayout.ObjectField("Clip", (VideoClip)sampleVideoClip, typeof(VideoClip), false);
			isSkip = EditorGUILayout.Toggle("Skip", isSkip);
			isLoop = EditorGUILayout.Toggle("Loop", isLoop);
			if (GUILayout.Button("PlayFromFile"))
			{
				MoviePlayer player = target as MoviePlayer;
				if (player != null)
				{
					player.PlayMovie(sampleFilePath, isSkip, isLoop);					
				}
			}
			if (GUILayout.Button("PlayFromClip"))
			{
				MoviePlayer player = target as MoviePlayer;
				if (player != null)
				{
					player.PlayMovie(sampleVideoClip, isSkip, isLoop);
				}
			}
		}
		EditorGUILayout.EndVertical();

	}
}

#endif