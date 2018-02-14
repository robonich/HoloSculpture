using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.Networking;

public class BlockUnitController : MonoBehaviour, IInputClickHandler {
    public BreakBlockAudioSourceController breakAudio;
    public GameObject BlockUnit;
    private UndoRedoManager blockHistoryManager;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        UnitBlockDestructionCommand destructionCommand = new UnitBlockDestructionCommand(
            transform.localPosition,
            transform.localRotation,
            GetComponent<Renderer>().material.color,
            BlockUnit,
            this.gameObject,
            breakAudio);

        blockHistoryManager.Do(destructionCommand);
    }

    private class UnitBlockDestructionCommand : ICommand
    {
        /// <summary>
        /// 壊されたときの Local position
        /// </summary>
        private Vector3 positionAtDestruction;
        /// <summary>
        /// 壊されたときの Local position
        /// </summary>
        private Quaternion rotationAtDestruction;
        /// <summary>
        /// 壊されたときの Color
        /// </summary>
        private Color colorAtDestruction;
        /// <summary>
        /// Instantiate したい prehab
        /// </summary>
        private GameObject BlockUnit;
        /// <summary>
        /// Destroy したい gameObject
        /// </summary>
        private GameObject gameObject;
        private BreakBlockAudioSourceController breakAudio;

        public UnitBlockDestructionCommand(Vector3 positionAtDestruction, Quaternion rotationAtDestruction, Color colorAtDestruction, GameObject BlockUnit, GameObject gameObject, BreakBlockAudioSourceController breakAudio)
        {
            this.positionAtDestruction = positionAtDestruction;
            this.rotationAtDestruction = rotationAtDestruction;
            this.colorAtDestruction = colorAtDestruction;
            this.BlockUnit = BlockUnit;
            this.gameObject = gameObject;
            this.breakAudio = breakAudio;
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
            GameObject block = (GameObject)Instantiate(BlockUnit, positionAtDestruction, rotationAtDestruction, InitialBlockGeneration.Instance.transform);
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
        blockHistoryManager = InitialBlockGeneration.Instance.blockHistroyManager;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
