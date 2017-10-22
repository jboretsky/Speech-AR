using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;

public static class SendSpeech {
	public static void Send(string filePath) {
		HttpWebRequest request = null;
		request = (HttpWebRequest)HttpWebRequest.Create("https://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?language=en-US&format=detailed");
		request.SendChunked = true;
		request.Accept = @"application/json;text/xml";
		request.Method = "POST";
		request.ProtocolVersion = HttpVersion.Version11;
		request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
		request.Headers["Ocp-Apim-Subscription-Key"] = "API_KEY";

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

		/*
		* Get the response from the service.
		*/
		Debug.Log("Response:");
		using (WebResponse response = request.GetResponse())
		{
			string responseString;
			Debug.Log(((HttpWebResponse)response).StatusCode);

			using (StreamReader sr = new StreamReader(response.GetResponseStream()))
			{
				responseString = sr.ReadToEnd();
			}

			Debug.Log(responseString);
//			Console.ReadLine();
		}
	}
}