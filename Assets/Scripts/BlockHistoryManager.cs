using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BlockCollection の状態を管理するためのクラス
/// UndoRedoManager を継承し、それに Reset 関数を加えた
/// </summary>
public class BlockHistoryManager : UndoRedoManager {

    /// <summary>
    /// あくまで BlockHistory 内の状態を reset するもの。 Blockの消去とかは BlockCollectionController にやってもらう
    /// </summary>
	public void Reset()
    {
        // すべての redo undo を消す
        this.undo.Clear();
        this.redo.Clear();
        this.CanUndo = this.undo.Count > 0;
        this.CanRedo = this.redo.Count > 0;
    }
}
