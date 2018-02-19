using UnityEngine;
using System;
using System.Collections.Generic;

public class BlockCollectionData {
    public float[] initialPosition;
    public float[] initialRotation;
    public float[] initialBlockSize;
    public int[][][] blockArrangement;

    static public Dictionary<int, Color> colorDic = new Dictionary<int, Color>() {
        {1, Color.gray },
        {2, Color.blue },
        {3, Color.yellow },
        {4, Color.red },
        {5, Color.green }
    };

    static public Dictionary<Color, int> colorToInt = new Dictionary<Color, int>()
    {
        {Color.gray, 1 },
        {Color.blue, 2 },
        {Color.yellow, 3 },
        {Color.red, 4 },
        {Color.green, 5 }
    };
}