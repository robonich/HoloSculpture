using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SharingWithUNET;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;


namespace FromScratch
{
    /// <summary>
    /// いろいろな集中管理をする場所
    /// </summary>
    public class SystemControllerInServer : NetworkBehaviour
    {
        public GameObject blockCollectionPrehab;
        public GameObject blockUnitPrehab;
        public GameObject sculptureModelPrehab;
        public GameObject sculptureBlockPrehab;
        public GameObject ObjectCollectionButtonsInServer;
        public GameObject ObjectCollectionButtonsOfPlayModes;
        public GameObject GoBackToStageSelectionButton;
        private GameObject worldAnchorObject;
        // これは一番最初の世界座標系における blockCollectionの初期座標
        // blockCollection 
        private Vector3 blockCollectionInitialPos = new Vector3(0.7f, -0.2f, 1.5f);
        private Vector3 sculptureModelInitialPos = new Vector3(-0.7f, -0.2f, 1.5f);

        private WorldAnchor worldAnchor;
        private bool isFirstWorldAnchorLocated = false;

        private string[] stageName = { "model1", "model2" };

        private string JsonFileName;
        public int[][][] blockCollectionMap;
        public int[][][] sculptureMap;
        private BlockCollectionData data;

        private PlayMode playMode;

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

        //public void OnSpeechKeywordRecognized(SpeechEventData eventData)
        //{
        //    switch (eventData.RecognizedText.ToLower())
        //    {
        //        case "start":
        //            StartGame();
        //            break;
        //        case "retry":
        //            RetryGame();
        //            break;
        //        case "reset":
        //            ResetGame();
        //            break;
        //    }
        //}

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

        private void SpawnBlocks(bool onlyBlockCollection = false)
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

                        // BlockCollection だけを配置させたいときは、sculptureMap の配置を飛ばす
                        if (!onlyBlockCollection) {
                            if (sculptureMap[z][y][x] > 0)
                            {
                                // 親は SculptureModel
                                GameObject nextSculpBlock = (GameObject)Instantiate(sculptureBlockPrehab, current, sculpParent.transform.rotation);
                                var sculptureBlockController = nextSculpBlock.GetComponent<SculptureBlockController>();
                                sculptureBlockController.color = BlockCollectionData.colorDic[sculptureMap[z][y][x]];

                                NetworkServer.Spawn(nextSculpBlock);
                            }
                         }
                        current += deltaX;
                    }
                    current += deltaY;
                }
                current += deltaZ;
            }
        }

        //public void StartGame()
        //{
        //    if (!isServer)
        //    {
        //        print("Client has no authority to start a game");
        //        return;
        //    }
        //    print("Start Game");
        //    if (BlockCollectionController.Instance != null)
        //    {
        //        print("You have already spawned a BlockCollection.");
        //        return;
        //    }

        //    // GameStateの変更
        //    GameStateManager.Instance.gameState = GameState.Playing;

        //    // Playing のオブジェクトの有効化
        //    RpcSetActivenessOfObjects(GameStateManager.Instance.gameState);

        //    //　次はここでステージ選択のことをする
        //    ReadJsonAndGenerateMap();
        //    SpawnBlockCollection();
        //    ScoreAndTimeController.Instance.Initialize();
        //}

        public void RetryGame()
        {
            if (!isServer)
            {
                print("Client has no authority to retry the game");
                return;
            }

            print("Retry Game");

            // BlockCollection の子オブジェクトを すべて enable する
            // これは server でだけ行われる
            var blockCollectionChildrenTransforms = BlockCollectionController.Instance.GetComponentsInChildren<Transform>();
            for (int i = BlockCollectionController.Instance.transform.childCount - 1; i >= 0; --i)
            {
                var g = BlockCollectionController.Instance.transform.GetChild(i).gameObject;
                if (g.name.Contains("BlockUnit"))
                {
                    g.GetComponent<BlockUnitController>().isActive = true;
                }
            }
            
            RpcResetHistoryManager();

            // map の reset
            for (int i = 0; i < blockCollectionMap.Length; i++)
            {
                for (int j = 0; j < blockCollectionMap[0].Length; j++)
                {
                    for (int k = 0; k < blockCollectionMap[0][0].Length; k++)
                    {
                        blockCollectionMap[i][j][k] = data.blockArrangement[i][j][k] == 0 ? 1 : data.blockArrangement[i][j][k];
                    }
                }
            }

            // retry のときは時間をリセットしない
            ScoreAndTimeController.Instance.Initialize(withoutLeftTime: true);
        }

        [ClientRpc]
        private void RpcSetActivenessOfObjects(GameState state)
        {
            print("Set activeness of objects");
            print(state);
            ObjectVisibleManager.Instance.SetActivenessOfObjects(state);
        }

        [ClientRpc]
        private void RpcResetHistoryManager()
        {
            print("Reset History Manager");
            BlockCollectionController.Instance.blockHistoryManager.Reset();
        }

        public void ResetGame()
        {
            if (!isServer)
            {
                print("Client has no authority to reset the game");
                return;
            }
            print("Reset Game");

            // BlockCollection, SculptureModel を破壊する
            Destroy(BlockCollectionController.Instance.gameObject);
            Destroy(SculptureModelController.Instance.gameObject);

            print("BlockCollectionController Instance is ");
            print(BlockCollectionController.Instance);

            // StageSelection を開始する
            ObjectCollectionButtonsInServer.SetActive(false);
            StartStageSelection();
        }

        // PlayModeSelection 関連スタート
        private void StartPlayModeSelection()
        {
            GameStateManager.Instance.gameState = GameState.PlayModeSelection;
            // これはサーバーだけなのでここで有効化
            ObjectCollectionButtonsOfPlayModes.SetActive(true);
            RpcSetActivenessOfObjects(GameStateManager.Instance.gameState);
        }

        public void SetSinglePlayMode()
        {
            playMode = PlayMode.Single;
            EndPlayModeSelection();
        }

        public void SetMultiPlayMode()
        {
            playMode = PlayMode.Multi;
            EndPlayModeSelection();
        }

        private void EndPlayModeSelection()
        {
            // これはサーバーだけなのでここで無効化
            ObjectCollectionButtonsOfPlayModes.SetActive(false);
            // 次に stageSelection を走らせる
            StartStageSelection();
        }
        // PlayModeSelection 関連終わり

        // StageSelection 関連スタート
        private void StartStageSelection()
        {
            GameStateManager.Instance.gameState = GameState.StageSelection;
            // StageSelection のオブジェクトを有効化
            RpcSetActivenessOfObjects(GameStateManager.Instance.gameState);
        }

        public void PlayerSelectStage(string stageName, string playerName)
        {
            // ここに stageName ごとに dictionary を持たせて、その値として playerName の配列を持たせる
            // もしいずれかの stageName の値が single なら 1, multi なら 2 以上の長さとなっていれば、ゲームを開始させる

            // ここにその判定の条件文を書く
            if (true)
            {
                JsonFileName = stageName.ToLower() + ".json";
                EndStageSelection();
            }
        }

        private void EndStageSelection()
        {
            StartPlaying();
        }
        // StageSelection 関連終わり

        // Playing 関連スタート
        public void StartPlaying()
        {
            print("Start Game");
            if (BlockCollectionController.Instance != null)
            {
                print("You have already spawned a BlockCollection.");
                return;
            }

            // GameStateの変更
            GameStateManager.Instance.gameState = GameState.Playing;

            // Playing のオブジェクトの有効化
            RpcSetActivenessOfObjects(GameStateManager.Instance.gameState);
            ObjectCollectionButtonsInServer.SetActive(true);

            //　次はここでステージ選択をする
            ReadJsonAndGenerateMap();
            SpawnBlockCollection();
            ScoreAndTimeController.Instance.Initialize();
        }

        public void EndPlaying()
        {
            ObjectCollectionButtonsInServer.SetActive(false);
            StartResult();
        }
        // Playing 関連終わり

        // Result 関連スタート
        private void StartResult()
        {
            GameStateManager.Instance.gameState = GameState.Result;
            RpcSetActivenessOfObjects(GameStateManager.Instance.gameState);
            GoBackToStageSelectionButton.SetActive(true);
        }

        public void EndResult()
        {
            GoBackToStageSelectionButton.SetActive(false);
            ResetGame();
        }
        // Result 関連終わり

        private void Start()
        {
            // set this speech manager as global listener
            InputManager.Instance.AddGlobalListener(gameObject);
            StartPlayModeSelection();
        }
        
    }
}