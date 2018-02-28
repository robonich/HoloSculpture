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

public class BlockCollectionController : NetworkBehaviour, ISpeechHandler
{

    public string JsonFileName;
    public GameObject BlockUnit;
    public BlockHistoryManager blockHistoryManager;

    private BlockCollectionData data;
    public int[][][] blockCollectionMap;

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
        print("Recognize text: ");
        print(eventData.RecognizedText);
        ChangeState(eventData.RecognizedText);
    }

    /// <summary>
    /// BlockUnit を配置する（ゲーム開始時や、リセットの時に呼ばれる）
    /// これはサーバーだけが呼べることに注意。
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

    /// <summary>
    /// reset はサーバーだけが呼べる？？
    /// 多分そうだよな。 reset をすると
    /// </summary>
    public void ResetBlocks()
    {
        if(!isServer)
        {
            print("Client has no authority to reset the game");
            return;
        }

        // すべてのデバイスで実行するブロックリセット前の動作
        CmdResetInAllDevice();
        
        // BlockCollection の子オブジェクトをすべて消す
        // これは server でだけ行われる
        print("Delete all blocks");
        var transforms = this.GetComponentsInChildren<Transform>();
        print(this.GetComponentsInChildren<Transform>().Count());
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            var g = transform.GetChild(i).gameObject;
            if (g.name.Contains("BlockUnit"))
            {
                DestroyImmediate(g);
            }
        }
        print(this.GetComponentsInChildren<Transform>().Count());

        // Arrange する
        ArrangeBlocks();

        // すべてのデバイスで実行するブロックリセット後の動作
        CmdInitializeScoreInAllDevice();
    }

    [Command]
    void CmdResetInAllDevice()
    {
        print("Reset in client");
        ScoreController.Instance.haveInitialized = false;
        // blockHistory の状態を reset
        print("Reset BlockHistoryManager");
        blockHistoryManager.Reset();

        // blockCollectionMap のリセット
        print("Reset BlockCollectionMap");
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
    }

    [Command]
    void CmdInitializeScoreInAllDevice()
    {
        print("Initialize Score in Client");
        ScoreController.Instance.InitializeScore();
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
    // Awake に書くことで、 Instantiate が呼ばれたらほかの処理が走る前に実行される
    void Awake()
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
