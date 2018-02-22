﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SharingWithUNET;

/// <summary>
/// Starts a session when the user taps the control this script is attached to.
/// </summary>
public class IStartSessionButton : MonoBehaviour, IInputClickHandler
{
    /// <summary>
    /// Script which controls hosting and discovering sessions.
    /// </summary>
    private INetworkDiscovery networkDiscovery;

    private BlockCollectionController blockCollection;
    private SculptureModelController sculptureModel;

    private void Start()
    {
        networkDiscovery = INetworkDiscovery.Instance;

        if (false)
        {
            blockCollection = BlockCollectionController.Instance;
            sculptureModel = SculptureModelController.Instance;
        }
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque && !Application.isEditor)
        {
            Debug.Log("Only HoloLens can host for now");
            Destroy(gameObject);
        }
#else
        if (Application.isEditor)
        {
            Debug.Log("Only HoloLens can host for now");
            Destroy(gameObject);
        }
#endif
    }

    /// <summary>
    /// Called when a click event is detected
    /// </summary>
    /// <param name="eventData">Information about the click.</param>
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (networkDiscovery.running)
        {
            // Only let HoloLens host
            // We are also allowing the editor to host for testing purposes, but shared anchors
            // will currently not work in this mode.

            if (
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
                !UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque ||
#endif
                Application.isEditor)
            {
                if (Application.isEditor)
                {
                    Debug.Log("Unity editor can host, but World Anchors will not be shared");
                }

                networkDiscovery.StartHosting("DefaultName");
                eventData.Use();

                if (false)
                {
                    // Session が開始した直後に、ブロックをspawnさせる
                    blockCollection.ArrangeBlocks();
                    sculptureModel.ArrangeBlocks();
                    // score を計算する
                    ScoreController.Instance.InitializeScore();
                }
            }
        }
    }
}
