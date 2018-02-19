using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Linq;

/// <summary>
/// SculptureModel の状態を管理する。Singleton として扱う
/// 1. 与えられたInitialStateJsonから配置する
/// </summary>
public class SculptureModelController : Singleton<SculptureModelController>
{

    public string JsonFileName;
    public GameObject BlockUnit;

    private BlockCollectionData data;
    public int[][][] sculptureMap;

    /// <summary>
    /// BlockUnit を配置する（ゲーム開始時や、リセットの時に呼ばれる）
    /// </summary>
    public void ArrangeBlocks()
    {
        print("Arrange Blocks");
        Vector3 initialScale = new Vector3(data.initialBlockSize[0], data.initialBlockSize[1], data.initialBlockSize[2]);
        Vector3 initialPos = new Vector3(data.initialPosition[0], data.initialPosition[1], data.initialPosition[2]) + this.transform.position - new Vector3(initialScale.x * data.blockArrangement[0][0].Length, 0, 0);
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

        for (int z = 0; z < sculptureMap.Length; z++)
        {
            current.y = initialPos.y;
            for (int y = 0; y < sculptureMap[0].Length; y++)
            {
                current.x = initialPos.x;
                for (int x = 0; x < sculptureMap[0][0].Length; x++)
                {

                    if (sculptureMap[z][y][x] > 0)
                    {
                        GameObject nextBlock = (GameObject)Instantiate(BlockUnit, current, transform.rotation, this.transform);
                        // NetworkServer が active になっていないと spawn されない
                        // Todo: あとでそのチェックをすべき
                        nextBlock.GetComponent<Renderer>().material.SetColor("_Color", BlockCollectionData.colorDic[sculptureMap[z][y][x]]);
                        
                        NetworkServer.Spawn(nextBlock);
                    }
                    current += deltaX;
                }
                current += deltaY;
            }
            current += deltaZ;
        }
    }

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

        // sculptureMap へのコピー
        sculptureMap = new int[data.blockArrangement.Length][][];
        for(int i = 0; i < sculptureMap.Length; i++)
        {
            sculptureMap[i] = new int[data.blockArrangement[0].Length][];
            for(int j = 0; j < sculptureMap[0].Length; j++)
            {
                sculptureMap[i][j] = new int[data.blockArrangement[0][0].Length];
                for(int k = 0; k < sculptureMap[0][0].Length; k++)
                {
                    sculptureMap[i][j][k] = data.blockArrangement[i][j][k];
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
