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

        public List<GameObject> objectsShouldBeVisibleAfterServerStart;

        // Use this for initialization
        void Start()
        {
            
        }

        public void EnableObjects()
        {
            foreach(var obj in objectsShouldBeVisibleAfterServerStart)
            {
                obj.SetActive(true);
            }
        }

        public void DisableObjects()
        {
            foreach(var obj in objectsShouldBeVisibleAfterServerStart)
            {
                obj.SetActive(false);
            }
        }
        
        // Update is called once per frame
        void Update()
        {

        }
    }
}