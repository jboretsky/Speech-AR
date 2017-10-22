using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Networking;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Text;
using System;

[Serializable]
public class TextResponse {
	public string RecognitionStatus;
	public string DisplayText;
	public static TextResponse CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<TextResponse>(jsonString);
	}
}

public class Recognition : MonoBehaviour {

	AudioSource _audio;
	AudioSource _audio2;
	TextMesh _text;
	public BackgroundWorker backgroundWorker1;
	int i = 0;
	public Slider _slider;
	float timeLeft;
	int interval = 3;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
//		_text = GetComponent<TextMesh>();
		_text = GameObject.Find ("Text").GetComponent<TextMesh> ();
		_slider = GameObject.Find("Slider").GetComponent<Slider>();
		timeLeft = (float)0;

		backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
		backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Send);
		backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Receive);

		UpdateAudio();
	}

	void UpdateAudio() {
		_audio.Stop();
		_audio.clip = Microphone.Start("Built-in Microphone", false, interval, 44100);
		_audio.loop = true;

		if (Microphone.IsRecording("Built-in Microphone")) {
			while (!(Microphone.GetPosition (null) > 0)) {}
		}
	}

	// Update is called once per frame
	void Update () {
		timeLeft += Time.deltaTime;
		if (!(Microphone.IsRecording ("Built-in Microphone")) && !_audio.isPlaying) {
			try {
				++i;
				SavWav.Save("clip"+i, _audio.clip);
				Debug.Log ("Saved audio clip" + i);
				this.backgroundWorker1.RunWorkerAsync(Application.persistentDataPath+"/clip"+i+".wav");
				_audio.clip = Microphone.Start("Built-in Microphone", false, interval, 44100);
				timeLeft = (float)0;
			}
			catch(Exception e) {
				Debug.Log (e);
			}
		}
		_slider.value = timeLeft / interval;
	}

	void Receive(object sender, RunWorkerCompletedEventArgs e) {
		Debug.Log ("e result: " + e.Result);
		if (!(string.IsNullOrEmpty((string)e.Result))) {
			_text.text = (string)e.Result;
		}
	}

	void Send(object sender, DoWorkEventArgs e) {
		HttpWebRequest request = null;
		request = (HttpWebRequest)HttpWebRequest.Create ("https://speech.platform.bing.com/speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=simple");
		request.SendChunked = true;
		request.Accept = @"application/json;text/xml";
		request.Method = "POST";
		request.ProtocolVersion = HttpVersion.Version11;
		request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
		request.Headers ["Ocp-Apim-Subscription-Key"] = "eea31b8c6a2041a59af19b436dd372d7";

		// Send an audio file by 1024 byte chunks
		using (var fs = new FileStream ((string)e.Argument, FileMode.Open, FileAccess.Read)) {

			/*
    		* Open a request stream and write 1024 byte chunks in the stream one at a time.
    		*/
			byte[] buffer = null;
			int bytesRead = 0;
			using (Stream requestStream = request.GetRequestStream ()) {
				/*
        		* Read 1024 raw bytes from the input audio file.
        		*/
				buffer = new Byte[checked((uint)Math.Min (1024, (int)fs.Length))];
				while ((bytesRead = fs.Read (buffer, 0, buffer.Length)) != 0) {
					requestStream.Write (buffer, 0, bytesRead);
				}

				// Flush
				requestStream.Flush ();
			}
		}

		using (WebResponse response = request.GetResponse ()) {
			string responseString;
			Debug.Log (((HttpWebResponse)response).StatusCode);

			using (StreamReader sr = new StreamReader (response.GetResponseStream ())) {
				responseString = sr.ReadToEnd ();
			}
			Debug.Log (responseString);

			TextResponse ret = TextResponse.CreateFromJSON (responseString);

			if (ret.RecognitionStatus == "Success") {
				if (ret.DisplayText != "") {
					e.Result = WrapString (ret.DisplayText);
				} else {
					Debug.Log ("Response was empty");
				}
			}
		}
	}

	public string WrapString(string str) {
		int myLimit = 20;
		string[] words = str.Split(' ');

		StringBuilder newSentence = new StringBuilder();

		string line = "";
		foreach (string word in words)
		{
			if ((line + word).Length > myLimit)
			{
				newSentence.AppendLine(line);
				line = "";
			}

			line += string.Format("{0} ", word);
		}

		if (line.Length > 0)
			newSentence.AppendLine(line);

		return newSentence.ToString();
	}
}