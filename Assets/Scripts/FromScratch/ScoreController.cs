using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace FromScratch
{
    public class ScoreController : NetworkBehaviour
    {
        private SystemControllerInServer systemControllerInServer;
        [SyncVar]
        private int score = 0;
        public Text scoreText;
        [SyncVar]
        public bool hasInitialized = false;

        private static ScoreController _Instance;
        public static ScoreController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    ScoreController[] objects = FindObjectsOfType<ScoreController>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(ScoreController).ToString(), objects.Length);
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

        /// <summary>
        /// スコア初期化の
        /// </summary>
        public void InitializeScore()
        {
            // score 計算は server だけ
            if (isServer)
            {
                systemControllerInServer = SystemControllerInServer.Instance;
                print("Initialize score");
                score = 0;
                for (int i = 0; i < systemControllerInServer.sculptureMap.Length; i++)
                {
                    for (int j = 0; j < systemControllerInServer.sculptureMap[0].Length; j++)
                    {
                        for (int k = 0; k < systemControllerInServer.sculptureMap[0][0].Length; k++)
                        {
                            if (systemControllerInServer.sculptureMap[i][j][k] == systemControllerInServer.blockCollectionMap[i][j][k])
                            {
                                score++;
                            }
                        }
                    }
                }

                hasInitialized = true;
            }
        }

        public void CalcAndChangeScoreAt(Vector3Int position)
        {
            // score 計算は server だけ
            if (isServer)
            {
                // もしまだスコアが初期化されていなかったら計算しない
                if (!hasInitialized)
                {
                    print("Score has not been initialized");
                    return;
                }
                if (systemControllerInServer.sculptureMap[position[0]][position[1]][position[2]] == systemControllerInServer.blockCollectionMap[position[0]][position[1]][position[2]])
                {
                    score++;
                }
                else
                {
                    score--;
                }
            }
            
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
            // 得点表示
            if (hasInitialized)
                scoreText.text = "Score: " + score.ToString();
            

            
        }
    }
}