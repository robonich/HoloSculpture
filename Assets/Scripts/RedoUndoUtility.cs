using System.Collections;
using System.Collections.Generic;
using System;
using HoloToolkit.Unity;

/// <summary>
/// 実行、元に戻す(undo)、やり直す(redo)の各動作を定義するインターフェース
/// </summary>
public interface ICommand
{
    /// <summary>
    /// 操作を実行するメソッド
    /// </summary>
    void Do();

    /// <summary>
    /// 操作を元に戻すメソッド
    /// </summary>
    void Undo();

    /// <summary>
    /// 操作をやり直すメソッド
    /// </summary>
    void Redo();
}

/// <summary>
/// 行なった操作の履歴を蓄積することでUndo,Redoの機能を提供するクラス
/// </summary>
public class UndoRedoManager
{
    private Stack<ICommand> undo = new Stack<ICommand>();
    private Stack<ICommand> redo = new Stack<ICommand>();
    private bool canUndo = false;
    private bool canRedo = false;

    /// <summary>
    /// 操作を実行し、かつその内容を履歴に追加します。
    /// </summary>
    /// <param name="command">ICommandインターフェースを実装し、行う操作を定義したオブジェクト</param>
    public void Do(ICommand command)
    {
        this.undo.Push(command);
        this.CanUndo = this.undo.Count > 0;

        command.Do();

        this.redo.Clear();
        this.CanRedo = this.redo.Count > 0;
    }

    /// <summary>
    /// 操作を実行し、かつその内容を履歴に追加します。
    /// </summary>
    /// <param name="doMethod">操作を行なうメソッド</param>
    /// <param name="doParamater">doMethodに必要な引数</param>
    /// <param name="undoMethod">操作を行なう前の状態に戻すためのメソッド</param>
    /// <param name="undoParamater">undoMethodに必要な引数</param>
    public void Do(Delegate doMethod, object[] doParamater, Delegate undoMethod, object[] undoParamater)
    {
        Command command = new Command(doMethod, doParamater, undoMethod, undoParamater);

        this.Do(command);
    }

    /// <summary>
    /// 行なった操作を取り消してひとつ前の状態に戻します。
    /// </summary>
    public void Undo()
    {
        if (this.undo.Count >= 1)
        {
            ICommand command = this.undo.Pop();
            this.CanUndo = this.undo.Count > 0;

            command.Undo();

            this.redo.Push(command);
            this.CanRedo = this.redo.Count > 0;
        }
    }

    /// <summary>
    /// 取り消した操作をやり直します。
    /// </summary>
    public void Redo()
    {
        if (this.redo.Count >= 1)
        {
            ICommand command = this.redo.Pop();
            this.CanRedo = this.redo.Count > 0;

            command.Redo();

            this.undo.Push(command);
            this.CanUndo = this.undo.Count > 0;
        }
    }

    /// <summary>
    /// Undo出来るかどうかを返します。
    /// </summary>
    public bool CanUndo
    {
        private set
        {
            if (this.canUndo != value)
            {
                this.canUndo = value;

                if (this.CanUndoChange != null)
                {
                    this.CanUndoChange(this, EventArgs.Empty);
                }
            }
        }
        get
        {
            return this.canUndo;
        }
    }

    /// <summary>
    /// Redo出来るかどうかを返します。
    /// </summary>
    public bool CanRedo
    {
        private set
        {
            if (this.canRedo != value)
            {
                this.canRedo = value;

                if (this.CanRedoChange != null)
                {
                    this.CanRedoChange(this, EventArgs.Empty);
                }
            }
        }
        get
        {
            return this.canRedo;
        }
    }

    /// <summary>
    /// Undo出来るかどうかの状態が変化すると発生します。
    /// </summary>
    public event EventHandler CanUndoChange;

    /// <summary>
    /// Redo出来るかどうかの状態が変化すると発生します。
    /// </summary>
    public event EventHandler CanRedoChange;

    private class Command : ICommand
    {
        private Delegate doMethod;
        private Delegate undoMethod;
        private object[] doParamater;
        private object[] undoParamater;

        public Command(Delegate doMethod, object[] doParamater, Delegate undoMethod, object[] undoParamater)
        {
            this.doMethod = doMethod;
            this.doParamater = doParamater;
            this.undoMethod = undoMethod;
            this.undoParamater = undoParamater;
        }

        #region ICommand メンバ

        public void Do()
        {
            doMethod.DynamicInvoke(doParamater);
        }

        public void Undo()
        {
            undoMethod.DynamicInvoke(undoParamater);
        }

        public void Redo()
        {
            doMethod.DynamicInvoke(doParamater);
        }

        #endregion
    }
}
