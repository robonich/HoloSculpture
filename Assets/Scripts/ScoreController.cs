using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;
using UnityEngine.UI;


public class ScoreController : Singleton<ScoreController> {

    private BlockCollectionController blockCollection;
    private SculptureModelController sculptureModel;
    private int score = 0;
    
    public Text scoreText;

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
        print("There are scoreText");
    }

    public void CalcAndChangeScoreAt(Vector3Int position)
    {
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
        blockCollection = BlockCollectionController.Instance;
        sculptureModel = SculptureModelController.Instance;


        // set this speech manager as global listener
        InputManager.Instance.AddGlobalListener(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
