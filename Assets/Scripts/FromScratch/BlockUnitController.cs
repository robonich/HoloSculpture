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

        [SyncVar]
        public Color color;
        [SyncVar]
        public Vector3 positionInMap;

        public void OnInputClicked(InputClickedEventData eventData)
        {
            // Instance が localplayer に限定されているので、DestroyObjectを一回かませるのは冗長なきがする?
            PlayerController.Instance.DestroyBlock(this.gameObject);
            //print("blockunit worldpos");
            //print(transform.position);
            //print("blockunit localpos");
            //print(transform.localPosition);
            //print("parent is");
            //print(transform.parent.name);
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
        }

        private void OnDestroy()
        {
            Instantiate(breakAudio, this.gameObject.transform.position, this.gameObject.transform.rotation);
        }
    }
}