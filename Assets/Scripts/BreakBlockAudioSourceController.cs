using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakBlockAudioSourceController : MonoBehaviour {

    private AudioSource breakBlockSound;

	// Use this for initialization
	void Start () {
        breakBlockSound = GetComponent<AudioSource>();
        Destroy(this.gameObject, breakBlockSound.clip.length);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
