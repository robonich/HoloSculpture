using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;
using UnityEngine.UI;
using UnityEngine.Networking;


public class ScoreController : MonoBehaviour {

    private BlockCollectionController blockCollection;
    private SculptureModelController sculptureModel;
    private int score = 0;
    public bool haveInitialized = false;
    public Text scoreText;

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
        blockCollection = BlockCollectionController.Instance;
        sculptureModel = SculptureModelController.Instance;
        print("there are ScoreController instance");
        score = 0;
        for (int i = 0; i < sculptureModel.sculptureMap.Length; i++)
        {
            for (int j = 0; j < sculptureModel.sculptureMap[0].Length; j++)
            {
                for (int k = 0; k < sculptureModel.sculptureMap[0][0].Length; k++)
                {
                    if (sculptureModel.sculptureMap[i][j][k] == blockCollection.blockCollectionMap[i][j][k])
                    {
                        score++;
                    }
                }
            }
        }
        scoreText.text = "Score: " + score.ToString();
        haveInitialized = true;
    }

    public void CalcAndChangeScoreAt(Vector3Int position)
    {
        // もしまだスコアが初期化されていなかったら計算しない
        if (!haveInitialized)
        {
            print("Score has not initialized");
            return;
        }
        if (sculptureModel.sculptureMap[position[0]][position[1]][position[2]] == blockCollection.blockCollectionMap[position[0]][position[1]][position[2]])
        {
            score++;
        } else
        {
            score--;
        }
        scoreText.text = "Score: " + score.ToString();
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        // ScoreController のほうが先に生成されるので、 blockCollection や sculptureModel の Instance が作られるまで更新する
		if (blockCollection == null || sculptureModel == null)
        {
            blockCollection = BlockCollectionController.Instance;
            sculptureModel = SculptureModelController.Instance;

            if (blockCollection == null || sculptureModel == null) return;

            // クライアント側でそれぞれ走らせるものだから、ここで実行させる
            InitializeScore();
        }
	}
}
