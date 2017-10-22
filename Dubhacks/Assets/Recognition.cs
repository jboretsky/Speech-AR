using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recognition : MonoBehaviour {

	AudioSource _audio;
	AudioSource _audio2;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
//		_audio.mute = true;
//		while (!(Microphone.GetPosition(null) > 0)) {}
//		_audio.Play();
		UpdateAudio();
	}

	void UpdateAudio() {
		_audio.Stop();
		_audio.clip = Microphone.Start("Built-in Microphone", false, 5, 44100);
		_audio.loop = true;

		Debug.Log (Microphone.IsRecording ("Built-in Microphone").ToString ());

		if (Microphone.IsRecording("Built-in Microphone")) {
			while (!(Microphone.GetPosition (null) > 0)) {}
		}
	}

	// Update is called once per frame
	void Update () {
		if (!(Microphone.IsRecording ("Built-in Microphone")) && !_audio.isPlaying) {
			Debug.Log ("POOP");
			_audio.Play();
			SavWav.Save("clip", _audio.clip);
//			_audio.clip = Microphone.Start("Built-in Microphone", false, 5, 44100);
		}
	}

	float getAveragedVolume() {
		float[] data = new float[256];
		float a = 0;
		_audio.GetOutputData(data, 0);
		foreach(float s in data){
			a+=Mathf.Abs(s);
		}
		return a / 256;
	}
}
