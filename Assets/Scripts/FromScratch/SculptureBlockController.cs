using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;


namespace FromScratch
{
    public class SculptureBlockController : NetworkBehaviour
    {
        [SyncVar]
        public Color color;

        // Use this for initialization
        void Start()
        {
            if (SculptureModelController.Instance == null)
            {
                Destroy(this);
                return;
            }

            //サーバがオブジェクト生成時にlocalPositionを初期位置に設定しているので
            //localPositionを維持したまま、Parentを設定する。
            transform.SetParent(SculptureModelController.Instance.transform, false);
            GetComponent<Renderer>().material.SetColor("_Color", color);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}