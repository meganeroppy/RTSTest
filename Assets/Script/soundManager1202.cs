using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager1202 : MonoBehaviour {

	private AudioSource _audioSource;
	public AudioSource audioSource
	{
		get{
			if( _audioSource == null )
			{
				var obj = new GameObject("BGM");
				obj.transform.SetParent(transform);
				_audioSource = obj.AddComponent<AudioSource>();
			}
			return _audioSource;
		}

		set{
			_audioSource = value;
		}
	}

	public AudioClip[] clips;
	int currentIndex = 0;

	// Use this for initialization
	void Start () {
		Play();
	}
	
	// Update is called once per frame
	void Update () {
		CheckInput();
	}

	void CheckInput()
	{
		if( Input.GetKeyDown( KeyCode.P ) ) 
		{
			if( audioSource.isPlaying )
			{
				Stop();
			}
			else
			{
				Play();
			}
		}

		if( Input.GetKeyDown( KeyCode.O ) ) 
		{
			if( audioSource.isPlaying )
			{
				currentIndex = ( currentIndex + 1 ) % clips.Length;
				Play( clips[ currentIndex ] );
			}
		}

		if( Input.GetKeyDown( KeyCode.F ) ) 
		{
			if( audioSource.isPlaying )
			{
				currentIndex = ( currentIndex + 1 ) % clips.Length;
				StartCoroutine( CrossFade(clips[ currentIndex ], 3f));
			}
		}
	}

	void Play()
	{
		audioSource.clip = clips[ currentIndex ];
		audioSource.Play();
		audioSource.loop = true;
	}

	void Play( AudioClip clip )
	{
		audioSource.clip = clip;
		audioSource.Play();
		audioSource.loop = true;
	}

	void Stop()
	{
		audioSource.Stop();
	}

	IEnumerator CrossFade(AudioClip newClip,  float delay )
	{
		var obj = new GameObject("NewBGM");
		obj.transform.SetParent( transform );
		var newAudioSource = obj.AddComponent<AudioSource>();

		newAudioSource.clip = newClip;
		newAudioSource.loop = true;
		newAudioSource.Play();

		float progress = 0;
		while (progress < 1)
		{
			progress = Mathf.Clamp01( progress + Time.deltaTime / delay ); 
			newAudioSource.volume = progress;
			audioSource.volume = 1f - progress;
			yield return null;
		}

		obj.name = "BGM";
		Destroy( audioSource.gameObject );
		audioSource = newAudioSource;
	}
}
