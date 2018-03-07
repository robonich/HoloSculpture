using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Networking;

namespace FromScratch {

    public class ButtonPressedActionInClient : MonoBehaviour, IInputClickHandler {

        public string actionName;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            switch (actionName)
            {
                case "undo":
                    BlockCollectionController.Instance.blockHistoryManager.Undo();
                    break;
                case "redo":
                    BlockCollectionController.Instance.blockHistoryManager.Redo();
                    break;
                case "stage1":
                    PlayerController.Instance.SelectStage("stage1");
                    break;
                case "stage2":
                    PlayerController.Instance.SelectStage("stage2");
                    break;
            }
        }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            
        }
    }
}