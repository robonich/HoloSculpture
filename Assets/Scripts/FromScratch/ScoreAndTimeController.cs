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
        private int finalScore = 0;
        private int initialScore = 0;
        [SyncVar]
        private int maxScore = 1;

        private int startTime;
        private int now;
        private int timeBonusDelta = 2;
        [SyncVar]
        private int leftTime = 120;
        private int initialLeftTime = 120;
        public Text timeText;
        public Text scoreText;
        public Image timerImage;
        public Text finalScoreText;
        

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
        /// サーバーにしか呼ばれていない
        /// </summary>
        public void Initialize(bool withoutLeftTime=false)
        {
            if (!withoutLeftTime)
            {
                leftTime = initialLeftTime;
                startTime = DateTime.Now.Hour * 60 * 60 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
            }
            // score 計算は server だけ
            if (isServer)
            {
                score = initialScore;
                maxScore = initialScore;
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
                            maxScore++;
                        }
                    }
                }

            }
        }

        public void CalcAndChangeScoreAt(Vector3Int position)
        {
            // score 計算は server だけ
            if (isServer)
            {
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

        public bool CheckIsGameEnd()
        {
            return ((leftTime <= 0) || (score == maxScore)); 
        }

        // Use this for initialization
        void Start()
        {
        }

        private void UpdateTimer()
        {
            now = DateTime.Now.Hour * 60 * 60 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
            leftTime -= now - startTime;
            startTime = now;
        }

        private void ShowScoreAndTime()
        {
            scoreText.text = "Score: " + score.ToString() + "/" + maxScore;
            timeText.text = leftTime.ToString();
        }

        private void CalcAndShowFinalScore()
        {
            var timeBonus = leftTime * timeBonusDelta;
            finalScore = score + timeBonus;
            finalScoreText.text = String.Format("{0} pt\n{1} x {2} pt = {3} pt\n\n{4} pt",
                score, leftTime, timeBonusDelta, timeBonus, finalScore);
        }

        // Update is called once per frame
        void Update()
        {
            // 得点表示
            switch (GameStateManager.Instance.gameState)
            {
                case GameState.PlayModeSelection:
                    break;
                case GameState.StageSelection:
                    break;
                case GameState.Playing:
                    UpdateTimer();
                    ShowScoreAndTime();

                    if (isServer)
                    {
                        if (CheckIsGameEnd())
                        {
                            print(CheckIsGameEnd());
                            SystemControllerInServer.Instance.EndPlaying();
                        }
                    }
                    break;
                case GameState.Result:
                    CalcAndShowFinalScore();
                    break;
            }
            
        }
    }
}