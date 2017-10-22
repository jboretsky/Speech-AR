using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class SetFocus : MonoBehaviour {

	// Use this for initialization
	void Start () {
		bool focusModeSet = CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
		if (!focusModeSet) 
		{
			Debug.Log ("Failed to set focus mode to continusauto (unsupported mode).");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
