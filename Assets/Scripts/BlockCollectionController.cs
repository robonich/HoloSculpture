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
/// BlockCollection の状態を管理する。Singleton として扱う
/// 1. 与えられたInitialStateJsonから配置する
/// 2. Undo/Redo 機能を保有（BlockHistoryManager）
/// </summary>

public class BlockCollectionController : Singleton<BlockCollectionController>, ISpeechHandler
{

    public string JsonFileName;
    public GameObject BlockUnit;
    public BlockHistoryManager blockHistoryManager;

    private BlockCollectionData data;
    public int[][][] blockCollectionMap;

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        print("Recognize text: ");
        print(eventData.RecognizedText);
        ChangeState(eventData.RecognizedText);
    }

    /// <summary>
    /// BlockUnit を配置する（ゲーム開始時や、リセットの時に呼ばれる）
    /// </summary>
    public void ArrangeBlocks()
    {
        print("Arrange Blocks");
        Vector3 initialScale = new Vector3(data.initialBlockSize[0], data.initialBlockSize[1], data.initialBlockSize[2]);
        Vector3 initialPos = new Vector3(data.initialPosition[0], data.initialPosition[1], data.initialPosition[2]) + this.transform.position - new Vector3(initialScale.x* data.blockArrangement[0][0].Length, 0, 0);
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

        for (int z = 0; z < blockCollectionMap.Length; z++)
        {
            current.y = initialPos.y;
            for (int y = 0; y < blockCollectionMap[0].Length; y++)
            {
                current.x = initialPos.x;
                for (int x = 0; x < blockCollectionMap[0][0].Length; x++)
                {

                    if (blockCollectionMap[z][y][x] > 0)
                    {
                        GameObject nextBlock = (GameObject)Instantiate(BlockUnit, current, transform.rotation, this.transform);
                        // NetworkServer が active になっていないと spawn されない
                        // Todo: あとでそのチェックをすべき
                        nextBlock.GetComponent<Renderer>().material.SetColor("_Color", BlockCollectionData.colorDic[blockCollectionMap[z][y][x]]);
                        // positionInMap を更新する
                        nextBlock.GetComponent<BlockUnitController>().positionInMap = new Vector3Int(z, y, x);
                        NetworkServer.Spawn(nextBlock);
                    }
                    current += deltaX;
                }
                current += deltaY;
            }
            current += deltaZ;
        }
    }

    public void ResetBlocks()
    {
        // blockHistory の状態を reset
        print("Reset BlockHistoryManager");
        blockHistoryManager.Reset();

        // BlockCollection の子オブジェクトをすべて消す
        print("Delete all blocks");
        var transforms = this.GetComponentsInChildren<Transform>();
        print(this.GetComponentsInChildren<Transform>().Count());
        for(int i = transform.childCount - 1; i>= 0; --i)
        {
            var g = transform.GetChild(i).gameObject;
            if (g.name.Contains("BlockUnit"))
            {
                DestroyImmediate(g);
            }
        }
        print(this.GetComponentsInChildren<Transform>().Count());

        // blockCollectionMap のリセット
        for (int i = 0; i < blockCollectionMap.Length; i++)
        {
            blockCollectionMap[i] = new int[data.blockArrangement[0].Length][];
            for (int j = 0; j < blockCollectionMap[0].Length; j++)
            {
                blockCollectionMap[i][j] = new int[data.blockArrangement[0][0].Length];
                for (int k = 0; k < blockCollectionMap[0][0].Length; k++)
                {
                    // ここで 0 のところは 1 に置き換える（バウンディングボックスを作るため）
                    blockCollectionMap[i][j][k] = data.blockArrangement[i][j][k] == 0 ? 1 : data.blockArrangement[i][j][k];
                }
            }
        }

        // Arrange する
        ArrangeBlocks();
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
            case "reset":
                ResetBlocks();
                break;
        }
    }

    // Use this for initialization
    void Start()
    {
        string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "Jsons");
        jsonFilePath = Path.Combine(jsonFilePath, JsonFileName);

        if (!File.Exists(jsonFilePath))
        {
            print(string.Format("Cannot Find Json File at {0}", jsonFilePath));
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        data = JsonConvert.DeserializeObject<BlockCollectionData>(json);

        // blockCollectionMap へのコピー
        blockCollectionMap = new int[data.blockArrangement.Length][][];
        for (int i = 0; i < blockCollectionMap.Length; i++)
        {
            blockCollectionMap[i] = new int[data.blockArrangement[0].Length][];
            for (int j = 0; j < blockCollectionMap[0].Length; j++)
            {
                blockCollectionMap[i][j] = new int[data.blockArrangement[0][0].Length];
                for (int k = 0; k < blockCollectionMap[0][0].Length; k++)
                {
                    // ここで 0 のところは 1 に置き換える（バウンディングボックスを作るため）
                    blockCollectionMap[i][j][k] = data.blockArrangement[i][j][k] == 0 ? 1 : data.blockArrangement[i][j][k];
                }
            }
        }

        // ここでつくることですべての場所で共通のManagerとなる
        blockHistoryManager = new BlockHistoryManager();

        // set this speech manager as global listener
        InputManager.Instance.AddGlobalListener(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
