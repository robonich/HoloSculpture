using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Networking;

namespace FromScratch
{

    public class ButtonPressedActionInServer : NetworkBehaviour, IInputClickHandler
    {

        public string actionName;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            print("Pressed Button");

            switch (actionName)
            {
                case "reset":
                    print("reset");
                    SystemControllerInServer.Instance.ResetGame();
                    break;
                case "retry":
                    print("retry");
                    SystemControllerInServer.Instance.RetryGame();
                    break;
                case "backToStageSelection":
                    SystemControllerInServer.Instance.EndResult();
                    break;
                case "startStage":
                    SystemControllerInServer.Instance.SelectStartStageButton();
                    break;
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}