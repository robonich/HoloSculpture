using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;

namespace FromScratch
{
    public class ObjectVisibleManager : NetworkBehaviour
    {
        private static ObjectVisibleManager _Instance;
        public static ObjectVisibleManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    ObjectVisibleManager[] objects = FindObjectsOfType<ObjectVisibleManager>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(ObjectVisibleManager).ToString(), objects.Length);
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

        public List<GameObject> objectsShouldBeVisibleAtPlayModeSelection;
        public List<GameObject> objectsShouldBeVisibleAtStageSelection;
        public List<GameObject> objectsShouldBeVisibleAtPlaying;
        public List<GameObject> objectsShouldBeVisibleAtResult;

        // Use this for initialization
        void Start()
        {
            
        }

        [ClientRpc]
        public void RpcSetActivenessOfObjects(GameState state)
        {
            print("In client");
            SetActivenessOfObjects(state);
        }

        public void SetActivenessOfObjects(GameState state)
        {
            switch (state)
            {
                case GameState.PlayModeSelection:
                    print("Enable PlayModeSelection Objects");
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlayModeSelection, true);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtStageSelection, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlaying, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtResult, false);
                    break;
                case GameState.StageSelection:
                    print("Enable StageSelection Objects");
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlayModeSelection, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtStageSelection, true);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlaying, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtResult, false);
                    break;
                case GameState.Playing:
                    print("Enable Playing Objects");
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlayModeSelection, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtStageSelection, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlaying, true);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtResult, false);
                    break;
                case GameState.Result:
                    print("Enable Result Objects");
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlayModeSelection, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtStageSelection, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtPlaying, false);
                    ChangeActivenessOfList(objectsShouldBeVisibleAtResult, true);
                    break;
            }
        }

        private void ChangeActivenessOfList(List<GameObject> objects, bool isActive)
        {
            foreach(var obj in objects)
            {
                obj.SetActive(isActive);
            }
        }
        
        // Update is called once per frame
        void Update()
        {
            
        }
    }
}