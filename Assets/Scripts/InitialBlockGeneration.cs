using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using HoloToolkit.Unity;

/// <summary>
/// Block を json からの情報から配置する。ゲーム開始時やリセットのときに呼びたい
/// Singleton として扱う。ほかのとこから呼び出すときに複数も使う必要ないから
/// </summary>
public class InitialBlockGeneration : SingleInstance<InitialBlockGeneration>
{ 
    public string JsonFileName;
    public GameObject BlockUnit;
    public UndoRedoManager blockHistroyManager;

    private BlockCollectionData data;
    private Dictionary<int, Color> colorDic = new Dictionary<int, Color>() {
        {1, Color.gray },
        {2, Color.blue },
        {3, Color.yellow },
        {4, Color.red },
        {5, Color.green }
    };

	// Use this for initialization
	void Start () {
        string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "Jsons");
        jsonFilePath = Path.Combine(jsonFilePath, JsonFileName);

        if (!File.Exists(jsonFilePath))
        {
            print(string.Format("Cannot Find Json File at {0}", jsonFilePath));
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        data = JsonConvert.DeserializeObject<BlockCollectionData>(json);

        // ここでつくることですべての場所で共通のManagerとなる
        blockHistroyManager = new UndoRedoManager();
    }

    /// <summary>
    /// BlockUnit を配置する（ゲーム開始時や、リセットの時に呼ばれる）
    /// </summary>
    public void ArrangeBlocks()
    {
        Vector3 initialScale = new Vector3(data.initialBlockSize[0], data.initialBlockSize[1], data.initialBlockSize[2]);
        Vector3 initialPos = new Vector3(data.initialPosition[0], data.initialPosition[1], data.initialPosition[2]);
        // Quartenion だからこの方向の定義はいらんかも
        Vector3 initialRot = new Vector3(data.initialRotation[0], data.initialRotation[1], data.initialRotation[2]);

        float deltax = initialScale.x;
        float deltay = initialScale.y;
        float deltaz = initialScale.z;
        Vector3 deltaX = new Vector3(deltax, 0, 0);
        Vector3 deltaY = new Vector3(0, deltay, 0);
        Vector3 deltaZ = new Vector3(0, 0, deltaz);
        // startは (0,0,0)
        float currentX = initialPos.x;
        float currentY = initialPos.y;
        float currentZ = initialPos.z;
        Vector3 current = new Vector3(currentX, currentY, currentZ);

        for (int z = 0; z < data.blockArrangement.Length; z++)
        {
            current.y = 0;
            for (int y = 0; y < data.blockArrangement[0].Length; y++)
            {
                current.x = 0;
                for (int x = 0; x < data.blockArrangement[0][0].Length; x++)
                {

                    if (data.blockArrangement[z][y][x] > 0)
                    {
                        GameObject nextBlock = (GameObject)Instantiate(BlockUnit, current, transform.rotation, this.transform);
                        // NetworkServer が active になっていないと spawn されない
                        // Todo: あとでそのチェックをすべき
                        nextBlock.GetComponent<Renderer>().material.SetColor("_Color", colorDic[data.blockArrangement[z][y][x]]);
                        NetworkServer.Spawn(nextBlock);
                    }
                    current += deltaX;
                }
                current += deltaY;
            }
            current += deltaZ;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
