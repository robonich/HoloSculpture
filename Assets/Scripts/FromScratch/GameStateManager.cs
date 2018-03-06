using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace FromScratch
{
    public class GameStateManager : NetworkBehaviour
    {

        [SyncVar]
        public GameState gameState = GameState.HostClientSelection;

        // Use this for initialization
        void Start()
        {
            print("Start GameStateManager");
            print("Current GameState is " + gameState.ToString());
        }

        public override void OnStartClient()
        {
            print("Client Started!");
        }

        public override void OnStartServer()
        {
            print("Server Started!");
        }
        

        // Update is called once per frame
        void Update()
        {

        }
    }

    public enum GameState
    {
        HostClientSelection,
        PlayModeSelection,
        StageSelection,
        Playing,
        Result
    }
}