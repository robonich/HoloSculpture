using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace FromScratch
{
    public class GameStateManager : NetworkBehaviour
    {
        private static GameStateManager _Instance;
        public static GameStateManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    GameStateManager[] objects = FindObjectsOfType<GameStateManager>();
                    if (objects.Length == 1)
                    {
                        _Instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}", typeof(GameStateManager).ToString(), objects.Length);
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

        [SyncVar]
        public GameState gameState = GameState.PlayModeSelection;

        //public event System.Action<GameState> StateChangedEvent;

        public void MoveTo(GameState state)
        {
            gameState = state;
            //StateChangedEvent(state);
        }

        // Use this for initialization
        void Start()
        {
            print("Start GameStateManager");
            print("Current GameState is " + gameState.ToString());
            
            ObjectVisibleManager.Instance.SetActivenessOfObjects(gameState);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public enum GameState
    {
        PlayModeSelection,
        StageSelection,
        Playing,
        Result
    }

    public enum PlayMode
    {
        Single,
        Multi
    }
}