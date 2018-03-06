using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;

namespace FromScratch
{
    public class BlockUnitController : NetworkBehaviour, IInputClickHandler
    {
        public BreakBlockAudioSourceController breakAudio;
        public BlockHistoryManager historyManager;

        [SyncVar]
        public Color color;
        [SyncVar]
        public Vector3 positionInMap;
        [SyncVar]
        public bool isActive;

        private class BlockUnitCommand : ICommand
        {
            private GameObject gameObject;

            public BlockUnitCommand(GameObject gameObject)
            {
                this.gameObject = gameObject;
            }

            public void Do()
            {
                PlayerController.Instance.DisableBlock(this.gameObject);
            }

            public void Redo()
            {
                print("Redo cube(Destroy cube which you have redone)");
                this.Do();
            }

            public void Undo()
            {
                print("Undo destruction");
                PlayerController.Instance.EnableBlock(this.gameObject);
            }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            BlockUnitCommand command = new BlockUnitCommand(this.gameObject);

            historyManager.Do(command);
        }

        // Use this for initialization
        void Start()
        {
            if (BlockCollectionController.Instance == null)
            {
                Destroy(this);
                return;
            }

            //サーバがオブジェクト生成時にlocalPositionを初期位置に設定しているので
            //localPositionを維持したまま、Parentを設定する。
            transform.SetParent(BlockCollectionController.Instance.transform, false);
            GetComponent<Renderer>().material.SetColor("_Color", color);

            historyManager = BlockCollectionController.Instance.blockHistoryManager;

            isActive = gameObject.activeSelf;
        }



        private void OnDestroy()
        {
            Instantiate(breakAudio, this.gameObject.transform.position, this.gameObject.transform.rotation);
        }

        void Update()
        {
            if (isActive != gameObject.GetComponent<BoxCollider>().enabled)
            {
                gameObject.GetComponent<MeshRenderer>().enabled = isActive;
                gameObject.GetComponent<BoxCollider>().enabled = isActive;

                // isActive が false になったということは破壊されたということなので音を出す
                if(!isActive)
                {
                    Instantiate(breakAudio, this.gameObject.transform.position, this.gameObject.transform.rotation);
                }
            }
        }
    }
}