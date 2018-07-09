using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;
using HoloToolkit.Sharing;

namespace FromScratch
{

    public class StageSelectionManager : NetworkBehaviour
    {
        // これらのテキストには NetworkIdentity はつけない
        public TextMesh StageSelectionStatusText;
        public TextMesh Stage1Text;
        public TextMesh Stage2Text;

        private string localPlayerName;

        public Dictionary<string, List<string>> playersSelection = new Dictionary<string, List<string>>();
        public StageSelectionStatus status = StageSelectionStatus.Selection;

        private static StageSelectionManager _Instance;
        public static StageSelectionManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    StageSelectionManager[] objects = FindObjectsOfType<StageSelectionManager>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(StageSelectionManager).ToString(), objects.Length);
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

        // Use this for initialization
        void Start()
        {
            playersSelection.Add("stage1", new List<string>());
            playersSelection.Add("stage2", new List<string>());

            ResetStageSelection();
        }

        // Update is called once per frame
        void Update()
        {
        }

        [ClientRpc]
        public void RpcAddPlayerToStage(string stage, string playerName)
        {
            // もしじぶんのplayerと同じ名前のやつがきたら you に変換する
            print(localPlayerName);
            if(playerName == localPlayerName)
            {
                playerName = "You";
            }

            // まずは重複を消す
            print(playersSelection["stage1"].Remove(playerName));
            print(playersSelection["stage2"].Remove(playerName));
            // 新たに追加する
            playersSelection[stage].Add(playerName);
            // 更新する
            UpdateStageText();
            // ステータス更新
            UpdateStatus();
        }

        public string MostPopularStage()
        {
            var stage1Num = playersSelection["stage1"].Count;
            var stage2Num = playersSelection["stage2"].Count;

            return stage1Num >= stage2Num ? "stage1" : "stage2";
        }

        private void UpdateStatus()
        {
            if(playersSelection["stage1"].Contains("You") || playersSelection["stage2"].Contains("You"))
            {
                var stageSelectedUsersNum = playersSelection["stage1"].Count + playersSelection["stage2"].Count;
                // host のときは　single と multi の選択があるからそれを反映させないといけない
                if (isServer) {
                    print("current users num");
                    print(NetworkServer.connections.Count);

                    var playModeThreshNum = SystemControllerInServer.Instance.playMode == PlayMode.Multi ? 2 : 1;
                    if ((stageSelectedUsersNum == NetworkServer.connections.Count) &&
                        stageSelectedUsersNum >= playModeThreshNum)
                    {
                        status = StageSelectionStatus.StartStage;
                    }
                    else
                    {
                        status = StageSelectionStatus.WaitForOthers;
                    }
                } else
                // client のときはふつうでよい
                {
                    status = StageSelectionStatus.WaitForOthers;
                }
            } else
            {
                status = StageSelectionStatus.Selection;
            }

            print("current status is " + status.ToString());
            UpdateStatusText();
        }

        // これはすべてのプレイヤーが stage を更新するたびに呼び出したいもの
        private void UpdateStageText()
        {
            Stage1Text.text = "";
            Stage2Text.text = "";

            foreach(var name in playersSelection["stage1"])
            {
                Stage1Text.text += name + "\n";
            }

            foreach(var name in playersSelection["stage2"])
            {
                Stage2Text.text += name + "\n";
            }
        }
        
        private void UpdateStatusText()
        {
            switch(status)
            {
                case StageSelectionStatus.Selection:
                    StageSelectionStatusText.text = "Please select stage";
                    break;
                case StageSelectionStatus.WaitForOthers:
                    if (isServer)
                    {
                        StageSelectionStatusText.text = "Waiting for client selection";
                        break;
                    }

                    StageSelectionStatusText.text = "Waiting for host selection";
                    break;
                case StageSelectionStatus.StartStage:

                    if (isServer)
                    {
                        StageSelectionStatusText.text = "Please start game";
                        break;
                    }

                    StageSelectionStatusText.text = "Waiting for host to start game";
                    
                    break;
            }
        }

        public void ResetStageSelection()
        {
            StageSelectionStatusText.text = "";
            Stage1Text.text = "";
            Stage2Text.text = "";
            playersSelection["stage1"].Clear();
            playersSelection["stage2"].Clear();
            status = StageSelectionStatus.Selection;
            UpdateStatusText();
            UpdateStageText();
        }

        [ClientRpc]
        public void RpcResetStageSelection()
        {
            ResetStageSelection();
        }

        public void SetLocalPlayerName(string name)
        {
            localPlayerName = name;
        }
    }
    

    public enum StageSelectionStatus
    {
        Selection,
        WaitForOthers,
        StartStage
    }
}