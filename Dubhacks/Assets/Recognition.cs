using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Networking;
using System.Net;
using System.IO;
using System;

[Serializable]
public class TextResponse {
	public string RecognitionStatus;
	public N_Best[] NBest;
	public static TextResponse CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<TextResponse>(jsonString);
	}
}

[Serializable]
public class N_Best {
	public string Display;
	public string Confidence;
	public static N_Best CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<N_Best>(jsonString);
	}
}

public class Recognition : MonoBehaviour {

	AudioSource _audio;
	AudioSource _audio2;
	TextMesh _text;
	int i = 0;

	// Use this for initialization
	void Start () {
		_audio = GetComponent<AudioSource>();
		_text = GetComponent<TextMesh>();
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
			try {
				++i;
				SavWav.Save("clip"+i, _audio.clip);
				Debug.Log ("Saved audio clip" + i);
				Send(Application.dataPath+"/clip"+i+".wav");
				_audio.clip = Microphone.Start("Built-in Microphone", false, 3, 44100);
			}
			catch(Exception e) {
				Debug.Log (e);
			}
		}
	}

	void Send(string filePath) {
		HttpWebRequest request = null;
		request = (HttpWebRequest)HttpWebRequest.Create("https://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?language=en-US&format=detailed");
		request.SendChunked = true;
		request.Accept = @"application/json;text/xml";
		request.Method = "POST";
		request.ProtocolVersion = HttpVersion.Version11;
		request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
		request.Headers["Ocp-Apim-Subscription-Key"] = "eea31b8c6a2041a59af19b436dd372d7";

		// Send an audio file by 1024 byte chunks
		using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{

			/*
    		* Open a request stream and write 1024 byte chunks in the stream one at a time.
    		*/
			byte[] buffer = null;
			int bytesRead = 0;
			using (Stream requestStream = request.GetRequestStream())
			{
				/*
        		* Read 1024 raw bytes from the input audio file.
        		*/
				buffer = new Byte[checked((uint)Math.Min(1024, (int)fs.Length))];
				while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
				{
					requestStream.Write(buffer, 0, bytesRead);
				}

				// Flush
				requestStream.Flush();
			}
		}

		using (WebResponse response = request.GetResponse()) {
			string responseString;
			Debug.Log(((HttpWebResponse)response).StatusCode);

			using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
				responseString = sr.ReadToEnd();
			}
			responseString = responseString.Replace("N-Best", "NBest");

			TextResponse ret = TextResponse.CreateFromJSON(responseString);
			if (ret.RecognitionStatus == "Success") {
				if (ret.NBest.Length > 0) {
					N_Best final = ret.NBest [0];
					_text.text = final.Display;
				} else {
					Debug.Log ("No response");
				}
			}
		}

//		// init your request...then:
//		DoWithResponse(request, (response) => {
//			string responseString;
//			using (StreamReader sr = new StreamReader(response.GetResponseStream()))
//			{
//				responseString = sr.ReadToEnd();
//	//			_text.text = sr.ReadToEnd();
//			}
//			Debug.Log(responseString);
//		});
	}
//
//	void DoWithResponse(HttpWebRequest request, Action<HttpWebResponse> responseAction) {
//		Action wrapperAction = () =>
//		{
//			request.BeginGetResponse(new AsyncCallback((iar) =>
//				{
//					var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
//					responseAction(response);
//				}), request);
//		};
//		wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
//			{
//				var action = (Action)iar.AsyncState;
//				action.EndInvoke(iar);
//			}), wrapperAction);
//	}
}

//public static class JsonHelper {
//
//	public static T[] FromJson<T>(string json) {
//		Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
//		return wrapper.Items;
//	}
//
//	[Serializable]
//	private class Wrapper<T> {
//		public T[] T;
//	}
//}