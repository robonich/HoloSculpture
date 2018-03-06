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
            switch (actionName)
            {
                case "reset":
                    if (isServer)
                    {
                        SystemControllerInServer.Instance.ResetGame();
                    }
                    break;
                case "retry":
                    if (isServer)
                    {
                        SystemControllerInServer.Instance.RetryGame();
                    }
                    break;
                case "start":
                    if (isServer)
                    {
                        SystemControllerInServer.Instance.StartGame();
                    }
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