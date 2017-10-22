using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recognition : MonoBehaviour {

	AudioSource _audio;
	AudioSource _audio2;
	int i = 0;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
		UpdateAudio();
	}

	void UpdateAudio() {
		_audio.Stop();
		_audio.clip = Microphone.Start("Built-in Microphone", false, 3, 44100);
		_audio.loop = true;

//		Debug.Log (Microphone.IsRecording ("Built-in Microphone").ToString ());
		if (Microphone.IsRecording("Built-in Microphone")) {
			while (!(Microphone.GetPosition (null) > 0)) {}
		}
	}

	// Update is called once per frame
	void Update () {
		if (!(Microphone.IsRecording ("Built-in Microphone")) && !_audio.isPlaying) {
//			_audio.Play();
			++i;
			SavWav.Save("clip"+i, _audio.clip);
			Debug.Log ("Saved audio clip" + i);
			SendSpeech.Send(Application.dataPath+"/clip"+i+".wav");
			_audio.clip = Microphone.Start("Built-in Microphone", false, 5, 44100);
		}
	}
}
