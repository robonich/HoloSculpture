using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testInstantiate : MonoBehaviour {

    public BreakBlockAudioSourceController prehab;

	// Use this for initialization
	void Start () {
        Instantiate(prehab, transform.position, transform.rotation);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
