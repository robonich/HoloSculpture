using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.Networking;

public class BlockUnitController : MonoBehaviour, IInputClickHandler {
    public BreakBlockAudioSourceController breakAudio;
    private UndoRedoManager blockHistoryManager;
    public Vector3Int positionInMap = new Vector3Int( 0, 0, 0 );

    public void OnInputClicked(InputClickedEventData eventData)
    {
        print(positionInMap);
        UnitBlockDestructionCommand destructionCommand = new UnitBlockDestructionCommand(
            transform.position,
            transform.rotation,
            GetComponent<Renderer>().material.color,
            positionInMap,
            this.gameObject,
            breakAudio);

        blockHistoryManager.Do(destructionCommand);
    }

    private class UnitBlockDestructionCommand : ICommand
    {
        /// <summary>
        /// 壊されたときの position
        /// </summary>
        private Vector3 positionAtDestruction;
        /// <summary>
        /// 壊されたときの position
        /// </summary>
        private Quaternion rotationAtDestruction;
        /// <summary>
        /// 壊されたときの Color
        /// </summary>
        private Color colorAtDestruction;
        /// <summary>
        /// Destroy したい gameObject
        /// </summary>
        private GameObject gameObject;
        private BreakBlockAudioSourceController breakAudio;

        /// <summary>
        /// Instantiate したい prehab
        /// </summary>
        private GameObject BlockUnit;
        /// <summary>
        /// blockCollectionMap の中での座標
        /// </summary>
        private Vector3Int positionInMap;
        private BlockCollectionController blockCollectionController;

        public UnitBlockDestructionCommand(Vector3 positionAtDestruction, Quaternion rotationAtDestruction, Color colorAtDestruction, Vector3Int positionInMap, GameObject gameObject, BreakBlockAudioSourceController breakAudio)
        {
            this.positionAtDestruction = positionAtDestruction;
            this.rotationAtDestruction = rotationAtDestruction;
            this.colorAtDestruction = colorAtDestruction;
            this.positionInMap = positionInMap;
            this.gameObject = gameObject;
            this.breakAudio = breakAudio;
            // BlockUnit を BlockUnitController に持たせて、ここに渡すと、BlockUnit が Destroy されると同時に BlockUnit のアドレスも消されてしまって Null参照になってしまったので、そこを解決するために BlockCollectionController という永遠に死なないやつから持ってくることにした
            this.BlockUnit = BlockCollectionController.Instance.BlockUnit;
            blockCollectionController = BlockCollectionController.Instance;
        }

        public void Do()
        {
            Destroy(this.gameObject);
        }

        public void Redo()
        {
            print("Redo cube(Destroy cube which you have redone)");
            this.Do();
        }

        public void Undo()
        {
            print("Undo destruction");
            GameObject block = (GameObject)Instantiate(BlockUnit, positionAtDestruction, rotationAtDestruction, BlockCollectionController.Instance.transform);
            // NetworkServer が active になっていないと spawn されない
            // Todo: あとでそのチェックをすべき
            block.GetComponent<Renderer>().material.SetColor("_Color", colorAtDestruction);
            // positionInMap の更新をする
            block.GetComponent<BlockUnitController>().positionInMap = positionInMap;
            NetworkServer.Spawn(block);
            // 新しく生成した block に破壊対象を変更する
            this.gameObject = block;
        }
    }

    // Use this for initialization
    void Start () {
        blockHistoryManager = BlockCollectionController.Instance.blockHistoryManager;
        // map を更新する
        // start の前には色がもう変わっているはずなので直接参照する
        // もし変わっていなかったら default が入るので 1 になるかエラーになる
        BlockCollectionController.Instance.blockCollectionMap[positionInMap[0]][positionInMap[1]][positionInMap[2]] = BlockCollectionData.colorToInt[GetComponent<Renderer>().material.color];
        // もしまだスコアが初期化されていなかったら特に計算されない
        ScoreController.Instance.CalcAndChangeScoreAt(positionInMap);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDestroy()
    {
        print("Destroy cube");
        // 破壊時の音をだす
        Instantiate(breakAudio, this.gameObject.transform.position, this.gameObject.transform.rotation);
        // map を更新する
        BlockCollectionController.Instance.blockCollectionMap[positionInMap[0]][positionInMap[1]][positionInMap[2]] = 0;
        ScoreController.Instance.CalcAndChangeScoreAt(positionInMap);
    }
}
