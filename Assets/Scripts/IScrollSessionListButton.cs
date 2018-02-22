// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SharingWithUNET;

/// <summary>
/// Attached to the 'up' and 'down' arrows in the scroll control
/// </summary>
public class IScrollSessionListButton : MonoBehaviour, IInputClickHandler
{

    /// <summary>
    /// Whether we are scrolling up (-1) in the list or down (1) in the list
    /// </summary>
    public int Direction;

    /// <summary>
    /// Called when the user clicks the control
    /// </summary>
    /// <param name="eventData">information about the click</param>
    public void OnInputClicked(InputClickedEventData eventData)
    {
        IScrollingSessionListUIController.Instance.ScrollSessions(Direction);
        eventData.Use();
    }
}
