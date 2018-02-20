using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA;
using HoloToolkit.Unity;
using System;

public class ObjectSettiongAnchor : MonoBehaviour, ISpeechHandler, IInputHandler {
    public GameObject cursorObject;
    public GameObject anchorPoint;
    private Vector3 startPos = new Vector3(0, 0, 0);
    private Vector3 endPos = new Vector3(0, 0, 0);
    private bool finishSettingAnchors = false;
    private bool finishCalibrating = false;
    private bool canSetAnchor = false;
    private GameObject startAnchorPoint;
    private GameObject endAnchorPoint;

    public void OnHoldCanceled(HoldEventData eventData)
    {
        print("Hold Canceled");
        finishSettingAnchors = false;
    }

    public void OnHoldCompleted(HoldEventData eventData)
    {
        print("End Calibration");
        endPos = cursorObject.transform.position;
        finishSettingAnchors = true;
    }

    public void OnHoldStarted(HoldEventData eventData)
    {
        print("Start Calibration");
        startPos = cursorObject.transform.position;
        finishSettingAnchors = false;
    }

    public void OnInputDown(InputEventData eventData)
    {
        if (!canSetAnchor) return;
        print("Start Calibration");
        startPos = cursorObject.transform.position;
        finishSettingAnchors = false;
        if (startAnchorPoint != null)
        {
            DestroyImmediate(startAnchorPoint);
        }
        startAnchorPoint = (GameObject)Instantiate(anchorPoint, startPos, transform.rotation);
        startAnchorPoint.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        print(startPos);
    }

    public void OnInputUp(InputEventData eventData)
    {
        if (!canSetAnchor) return;
        print("End Calibration");
        endPos = cursorObject.transform.position;
        finishSettingAnchors = true;
        if(endAnchorPoint != null)
        {
            DestroyImmediate(endAnchorPoint);
        }
        endAnchorPoint = (GameObject)Instantiate(anchorPoint, endPos, transform.rotation);
        endAnchorPoint.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        print(endPos);
    }

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        print(eventData.RecognizedText);
        switch (eventData.RecognizedText.ToLower())
        {
            case "set":
                print("Start Setting Anchor");
                if (!finishSettingAnchors)
                {
                    print("StartPos and EndPos initialization haven't done yet!!");
                }

                // Need to remove any anchor that is on the object before we can move the object.
                WorldAnchor worldAnchor = GetComponent<WorldAnchor>();

                if (worldAnchor != null)
                {
                    DestroyImmediate(worldAnchor);
                }

                // Move the object to the specified place
                // start と end の中心に置く
                transform.position = (startPos + endPos) / 2;
                Vector3 relativePos = endPos - startPos;
                Quaternion rotation = Quaternion.LookRotation(relativePos);
                transform.rotation = rotation;

                // Attach a new anchor
                worldAnchor = gameObject.AddComponent<WorldAnchor>();

                // Name the anchor
                string exportingAnchorName = Guid.NewGuid().ToString();
                Debug.Log("preparing " + exportingAnchorName);

                finishCalibrating = true;
                canSetAnchor = false;
                break;
            case "start":
                canSetAnchor = true;
                break;
        }
    }

    // Use this for initialization
    void Start () {
        // set this speech manager as global listener
        InputManager.Instance.AddGlobalListener(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
