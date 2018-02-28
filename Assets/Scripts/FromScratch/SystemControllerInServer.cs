using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;


namespace FromScratch
{
    /// <summary>
    /// ゲームを開始したとき（サーバーを建てたとき）にブロックを初期化する
    /// spawn させたいのでここでやる
    /// </summary>
    public class SystemControllerInServer : NetworkBehaviour, ISpeechHandler
    {
        public GameObject blockCollectionPrehab;
        public GameObject blockUnitPrehab;
        public GameObject sculptureModelPrehab;
        public GameObject sculptureBlockPrehab;
        private GameObject worldAnchorObject;
        // これは一番最初の世界座標系における blockCollectionの初期座標
        // blockCollection 
        private Vector3 blockCollectionInitialPos = new Vector3(0.7f, 0.5f, 1.5f);
        private Vector3 sculptureModelInitialPos = new Vector3(-0.7f, 0.5f, 1.5f);
        private WorldAnchor worldAnchor;
        private bool isFirstWorldAnchorLocated = false;

        public string JsonFileName;
        public int[][][] blockCollectionMap;
        public int[][][] sculptureMap;
        private BlockCollectionData data;
        public BlockHistoryManager historyManager;

        private static SystemControllerInServer _Instance;
        public static SystemControllerInServer Instance
        {
            get
            {
                if (_Instance == null)
                {
                    SystemControllerInServer[] objects = FindObjectsOfType<SystemControllerInServer>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(SystemControllerInServer).ToString(), objects.Length);
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
            if (eventData.RecognizedText.ToLower() == "spawn")
            {
                if(!isServer)
                {
                    print("You have no authority to spawn block collection");
                    return;
                }
                //　次はここでステージ選択のことをする
                ReadJsonAndGenerateMap();
                SpawnBlockCollection();
                ScoreController.Instance.InitializeScore();
            }
        }

        private void ReadJsonAndGenerateMap()
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
            sculptureMap = new int[data.blockArrangement.Length][][];
            for (int i = 0; i < blockCollectionMap.Length; i++)
            {
                blockCollectionMap[i] = new int[data.blockArrangement[0].Length][];
                sculptureMap[i] = new int[data.blockArrangement[0].Length][];
                for (int j = 0; j < blockCollectionMap[0].Length; j++)
                {
                    blockCollectionMap[i][j] = new int[data.blockArrangement[0][0].Length];
                    sculptureMap[i][j] = new int[data.blockArrangement[0][0].Length];
                    for (int k = 0; k < blockCollectionMap[0][0].Length; k++)
                    {
                        // ここで 0 のところは 1 に置き換える（バウンディングボックスを作るため）
                        blockCollectionMap[i][j][k] = data.blockArrangement[i][j][k] == 0 ? 1 : data.blockArrangement[i][j][k];
                        // sculptureMap はそのまま
                        sculptureMap[i][j][k] = data.blockArrangement[i][j][k];
                    }
                }
            }
        }

        /// <summary>
        /// サーバーにしか呼ばれないBlockCollectionをspawnするための関数
        /// </summary>
        private void SpawnBlockCollection()
        {
            print("Spawn BlockCollection");
            worldAnchor = SharedCollection.Instance.GetComponent<WorldAnchor>();
            if (worldAnchor == null)
            {
                print("There are still no WorldAnchor");
                return;
            }

            if (!worldAnchor.isLocated)
            {
                print("WorldAnchor has not been located yet");
            }

            worldAnchorObject = SharedCollection.Instance.gameObject;
            print("Anchor pos is");
            print(worldAnchorObject.transform.position);

            var blockCollectionLocalPos = worldAnchorObject.transform.InverseTransformPoint(blockCollectionInitialPos);
            print("localPos is");
            print(blockCollectionLocalPos);

            var sculptureModelLocalPos = worldAnchorObject.transform.InverseTransformPoint(sculptureModelInitialPos);
            print("localPos is");
            print(blockCollectionLocalPos);

            var localDir = worldAnchorObject.transform.InverseTransformVector(-transform.forward);
            print("localDir is");
            print(localDir);

            var blockCollection = (GameObject)Instantiate(blockCollectionPrehab, blockCollectionLocalPos, Quaternion.Euler(localDir));
            blockCollection.GetComponent<BlockCollectionController>().localPos = blockCollectionLocalPos;
            blockCollection.GetComponent<BlockCollectionController>().localRot = Quaternion.Euler(localDir);

            var sculptureModel = (GameObject)Instantiate(sculptureModelPrehab, sculptureModelLocalPos, Quaternion.Euler(localDir));
            sculptureModel.GetComponent<SculptureModelController>().localPos = sculptureModelLocalPos;
            sculptureModel.GetComponent<SculptureModelController>().localRot = Quaternion.Euler(localDir);

            NetworkServer.Spawn(blockCollection);
            NetworkServer.Spawn(sculptureModel);

            SpawnBlocks();
        }

        private void SpawnBlocks()
        {
            GameObject blockParent = BlockCollectionController.Instance.gameObject;
            GameObject sculpParent = SculptureModelController.Instance.gameObject;

            print("Spawn BlockUnits");
            Vector3 initialScale = new Vector3(data.initialBlockSize[0], data.initialBlockSize[1], data.initialBlockSize[2]);
            Vector3 initialPos = new Vector3(data.initialPosition[0], data.initialPosition[1], data.initialPosition[2]) - new Vector3(initialScale.x * data.blockArrangement[0][0].Length, 0, 0);
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
                            //親は BlockCollection
                            GameObject nextBlockUnit = Instantiate(blockUnitPrehab, current, blockParent.transform.rotation);

                            // localplayer 側で色などの情報は持たせておく
                            var blockUnitController = nextBlockUnit.GetComponent<BlockUnitController>();
                            blockUnitController.color = BlockCollectionData.colorDic[blockCollectionMap[z][y][x]];
                            blockUnitController.positionInMap = new Vector3(z, y, x);
                            
                            NetworkServer.Spawn(nextBlockUnit);
                        }

                        if (sculptureMap[z][y][x] > 0)
                        {
                            // 親は SculptureModel
                            GameObject nextSculpBlock = (GameObject)Instantiate(sculptureBlockPrehab, current, sculpParent.transform.rotation);
                            var sculptureBlockController = nextSculpBlock.GetComponent<SculptureBlockController>();
                            sculptureBlockController.color = BlockCollectionData.colorDic[sculptureMap[z][y][x]];

                            NetworkServer.Spawn(nextSculpBlock);
                        }
                        current += deltaX;
                    }
                    current += deltaY;
                }
                current += deltaZ;
            }
        }
        private void Start()
        {
            // set this speech manager as global listener
            InputManager.Instance.AddGlobalListener(gameObject);
        }
    }
}