using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;
using HoloToolkit.Unity.InputModule;
using System.Linq;

using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;

namespace FromScratch
{

    /// <summary>
    /// BlockCollection の状態を管理する。Singleton として扱う
    /// 1. 与えられたInitialStateJsonから配置する
    /// 2. Undo/Redo 機能を保有（BlockHistoryManager）
    /// </summary>

    public class BlockCollectionController : NetworkBehaviour, ISpeechHandler
    {
        public BlockHistoryManager blockHistoryManager = new BlockHistoryManager();

        private WorldAnchor worldAnchor;
        private bool isFirstWorldAnchorLocated = false;

        [SyncVar]
        public Vector3 localPos;
        [SyncVar]
        public Quaternion localRot;

        private static BlockCollectionController _Instance;
        public static BlockCollectionController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    BlockCollectionController[] objects = FindObjectsOfType<BlockCollectionController>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(BlockCollectionController).ToString(), objects.Length);
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

        
        public void OnSpeechKeywordRecognized(SpeechEventData eventData)
        {
            ChangeState(eventData.RecognizedText);
        }

        private void ChangeState(string command)
        {
            switch (command.ToLower())
            {
                case "undo":
                    blockHistoryManager.Undo();
                    break;
                case "redo":
                    blockHistoryManager.Redo();
                    break;
            }
        }

        // Use this for initialization
        // Awake に書くことで、 Instantiate が呼ばれたらほかの処理が走る前に実行される
        void Awake()
        {
            
        }

        void Start()
        {
            // set this speech manager as global listener
            InputManager.Instance.AddGlobalListener(gameObject);

            if (SharedCollection.Instance == null)
            {
                Debug.LogError("This script required a SharedCollection script attached to a gameobject in the scene");
                Destroy(this);
                return;
            }

            transform.SetParent(SharedCollection.Instance.transform, false);
            transform.localPosition = localPos;
            transform.localRotation = localRot;
        }

        // Update is called once per frame
        void Update()
        {
            //if(isServer)
            //{
            //    localPos = this.transform.localPosition;
            //    localRot = this.transform.localRotation;
            //} else
            //{
            //    this.transform.localPosition = localPos;
            //    this.transform.localRotation = localRot;
            //}

            // worldAnchor がちゃんと定まるまで続ける
            //if (!isFirstWorldAnchorLocated)
            //{
                //worldAnchor = SharedCollection.Instance.GetComponent<WorldAnchor>();
                //if (worldAnchor == null)
                //    return;

                //if (worldAnchor.isLocated)
                //{
                //    print("SetUp BlockCollectionController");
                //    if (SharedCollection.Instance == null)
                //    {
                //        Debug.LogError("This script required a SharedCollection script attached to a gameobject in the scene");
                //        Destroy(this);
                //        return;
                //    }

                //    print("WorldAnchorPos");
                //    print(worldAnchor.transform.position);

                //    print("BlockCollectionPos");
                //    //サーバがオブジェクト生成時にlocalPositionを初期位置に設定しているので
                //    //localPositionを維持したまま、Parentを設定する。
                //    print(transform.position);
                //    print(transform.localPosition);
                //    print("AfterSetParent");
                //    transform.SetParent(SharedCollection.Instance.transform, false);
                //    print(transform.position);
                //    print(transform.localPosition);
                //    print("Set local pos manually");
                //    transform.localPosition = localPos;
                //    transform.localRotation = localRot;
                //    print(transform.position);
                //    print(transform.localPosition);

                //}
                //isFirstWorldAnchorLocated = true;
            //}
        }

    }
}