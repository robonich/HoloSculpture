﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.Networking;

public class BlockUnitController : MonoBehaviour, IInputClickHandler {
    public BreakBlockAudioSourceController breakAudio;
    private UndoRedoManager blockHistoryManager;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        UnitBlockDestructionCommand destructionCommand = new UnitBlockDestructionCommand(
            transform.position,
            transform.rotation,
            GetComponent<Renderer>().material.color,
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

        public UnitBlockDestructionCommand(Vector3 positionAtDestruction, Quaternion rotationAtDestruction, Color colorAtDestruction, GameObject gameObject, BreakBlockAudioSourceController breakAudio)
        {
            this.positionAtDestruction = positionAtDestruction;
            this.rotationAtDestruction = rotationAtDestruction;
            this.colorAtDestruction = colorAtDestruction;
            this.gameObject = gameObject;
            this.breakAudio = breakAudio;
            // BlockUnit を BlockUnitController に持たせて、ここに渡すと、BlockUnit が Destroy されると同時に BlockUnit のアドレスも消されてしまって Null参照になってしまったので、そこを解決するために BlockCollectionController という永遠に死なないやつから持ってくることにした
            this.BlockUnit = BlockCollectionController.Instance.BlockUnit;
        }

        public void Do()
        {
            print("Destroy cube");
            Instantiate(breakAudio, this.gameObject.transform.position, this.gameObject.transform.rotation);
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
            NetworkServer.Spawn(block);
            // 新しく生成した block に破壊対象を変更する
            this.gameObject = block;
        }
    }

    // Use this for initialization
    void Start () {
        blockHistoryManager = BlockCollectionController.Instance.blockHistoryManager;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}