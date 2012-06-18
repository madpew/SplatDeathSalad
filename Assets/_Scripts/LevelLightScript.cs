using UnityEngine;
using System.Collections;

public class LevelLightScript : MonoBehaviour {
	
	
	public bool litInPitchBlack = false;
	
	public bool disableifNotPitchBlack = false;
	
	// Use this for initialization
	void Start () {
		
		if (GameObject.Find("_SophieNet") != null){
			if (GameObject.Find("_SophieNet").GetComponent<SophieNetworkScript>().gameSettings.pitchBlack){
				if (!litInPitchBlack) light.enabled = false;
			}else{
				if (disableifNotPitchBlack) light.enabled = false;
			}
		}
	}
	
}
