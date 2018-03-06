using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;

namespace FromScratch
{
    public class ObjectVisibleManager : NetworkBehaviour
    {

        public List<GameObject> objectsShouldBeVisibleAfterServerStart;

        // Use this for initialization
        void Start()
        {
            foreach (var obj in objectsShouldBeVisibleAfterServerStart)
            {
                obj.SetActive(false);
            }
        }

        public override void OnStartClient()
        {
            foreach (var obj in objectsShouldBeVisibleAfterServerStart)
            {
                obj.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}