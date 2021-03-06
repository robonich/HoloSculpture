﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SharingWithUNET;
using HoloToolkit.Unity.InputModule;
using System.Linq;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;


namespace FromScratch
{

    /// <summary>
    /// SculptureModel の状態を管理する。Singleton として扱う
    /// 1. 与えられたInitialStateJsonから配置する
    /// </summary>
    public class SculptureModelController : NetworkBehaviour
    {
        private WorldAnchor worldAnchor;
        private bool isFirstWorldAnchorLocated = false;

        [SyncVar]
        public Vector3 localPos;
        [SyncVar]
        public Quaternion localRot;

        private static SculptureModelController _Instance;
        public static SculptureModelController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    SculptureModelController[] objects = FindObjectsOfType<SculptureModelController>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(SculptureModelController).ToString(), objects.Length);
                    }
                }
                return _Instance;
            }
        }

        /// <summary>
        /// Called by Unity when destroying a MonoBehaviour. Scripts that extend
        /// SingleInstance should be sure to call base.OnDestroy() to ensure the
        /// underlying static _Instance reference is properly cleaned up.
        /// </summary>
        protected virtual void OnDestroy()
        {
            _Instance = null;
        }

        // Update is called once per frame
        void Update()
        {
            // worldAnchor がちゃんと定まるまで続ける
            if (!isFirstWorldAnchorLocated)
            {
                worldAnchor = SharedCollection.Instance.GetComponent<WorldAnchor>();
                if (worldAnchor == null)
                    return;

                if (worldAnchor.isLocated)
                {
                    print("SetUp SculptureModelController");
                    if (SharedCollection.Instance == null)
                    {
                        Debug.LogError("This script required a SharedCollection script attached to a gameobject in the scene");
                        Destroy(this);
                        return;
                    }
                    
                    //サーバがオブジェクト生成時にlocalPositionを初期位置に設定しているので
                    //localPositionを維持したまま、Parentを設定する。
                    transform.SetParent(SharedCollection.Instance.transform, false);
                    
                    transform.localPosition = localPos;
                    transform.localRotation = localRot;

                }
                isFirstWorldAnchorLocated = true;
            }
        }
    }
}