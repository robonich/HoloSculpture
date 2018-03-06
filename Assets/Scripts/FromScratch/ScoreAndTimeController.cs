using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

namespace FromScratch
{
    public class ScoreAndTimeController : NetworkBehaviour
    {
        private SystemControllerInServer systemControllerInServer;
        [SyncVar]
        private int score = 0;
        private int initialScore = 0;

        private int startTime;
        private int now;
        [SyncVar]
        private int leftTime = 120;
        private int initialLeftTime = 120;
        public Text timeText;
        public Text scoreText;
        public Image timerImage;
        [SyncVar]
        public bool hasInitialized = false;

        private static ScoreAndTimeController _Instance;
        public static ScoreAndTimeController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    ScoreAndTimeController[] objects = FindObjectsOfType<ScoreAndTimeController>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(ScoreAndTimeController).ToString(), objects.Length);
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
        public void Initialize()
        {
            score = initialScore;
            leftTime = initialLeftTime;
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

                startTime = DateTime.Now.Hour * 60 * 60 + DateTime.Now.Minute * 60 + DateTime.Now.Second;

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
            timerImage.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {

            // 得点表示
            if (hasInitialized)
            {
                scoreText.text = "Score: " + score.ToString();
                now = DateTime.Now.Hour * 60 * 60 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
                leftTime -= now - startTime;
                startTime = now;
                timeText.text = leftTime.ToString();
                timerImage.enabled = true;
            }
        }
    }
}