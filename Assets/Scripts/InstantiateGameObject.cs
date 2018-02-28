using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;
using HoloToolkit.Unity.InputModule;


/// <summary>
/// ゲームを開始したとき（サーバーを建てたとき）にブロックを初期化する
/// spawn させたいのでここでやる
/// </summary>
public class InstantiateGameObject : NetworkBehaviour {

    public GameObject blockCollectionPrehab;
    public GameObject sculptureModelPrehab;
    public GameObject blockPositionControllerPrehab;
    private GameObject worldAnchorObject;
    private Vector3 blockCollectionInitialPos = new Vector3(0.0f, 0.2f, 1.5f);
    private Vector3 sculptureModelInitialPos = new Vector3(-1.3f, 0.2f, 1.5f);
    private Vector3 blockPositionControllerInitialPos = new Vector3(0.5f, 0.2f, 1.5f);

	// Use this for initialization
	void Start () {
        worldAnchorObject = SharedCollection.Instance.gameObject;
	}

    public override void OnStartServer()
    {
        var blockCollection = Instantiate(blockCollectionPrehab, blockCollectionInitialPos, worldAnchorObject.transform.rotation, worldAnchorObject.transform);
        var sculptureModel = Instantiate(sculptureModelPrehab, sculptureModelInitialPos, worldAnchorObject.transform.rotation, worldAnchorObject.transform);
        // この操作はサーバーでしかしてなさそうな気がするけど、クライアント側でもちゃんとHologramCollectionの子オブジェクトになっててほしい

        if (blockCollection.GetComponentInParent<SharedCollection>() == null)
        {
            print("Parent has not been set in Server");
        } else
        {
            print("Set parent as HologramCollection in Server");
        }
        NetworkServer.Spawn(blockCollection);
        NetworkServer.Spawn(sculptureModel);

        var blockPositionController = Instantiate(blockPositionControllerPrehab, blockPositionControllerInitialPos, worldAnchorObject.transform.rotation, worldAnchorObject.transform);
        blockPositionController.GetComponent<HandDraggable>().HostTransform = blockCollection.transform;

        // ちゃんと初期化されてそうだったら
        // 多分うまくいく（Awake されてるはずだから）
        if (BlockCollectionController.Instance.blockCollectionMap != null)
        {
            // ブロックをspawnさせる
            // BlockCollectionController の Awake がこの時点ではもう呼ばれているはずなので、 ArrangeBlocks を呼んでも問題なさそう
            // サーバー側で spawn させたいものだから、ここで ArrangeBlocks を呼ぶ
            BlockCollectionController.Instance.ArrangeBlocks();
            SculptureModelController.Instance.ArrangeBlocks();
        }
    }

    public override void OnStartClient()
    {
        // もし親が設定されていなかったら、各クライアントが始まる段階で設定する 
        if (BlockCollectionController.Instance.gameObject.GetComponentInParent<SharedCollection>() == null)
        {
            BlockCollectionController.Instance.transform.SetParent(worldAnchorObject.gameObject.transform);
            SculptureModelController.Instance.transform.SetParent(worldAnchorObject.gameObject.transform);
            print("Set parent as HologramCollection in client");
        }
        else
        {
            print("Parent has already been set in client");
        }

        // BlockPositionController が server側で作られていなかったら、クライアントで作る
        // これはどっちかというと、 host（server と client）の server と client が同じ世界を共有しているのかを確かめるためにある
        // もし存在していれば世界を共有していて、存在していなかったら世界を共有していない
        if (GameObject.Find("BlockPositionController") == null)
        {
            print("There is no BlockPositionController. Make a new one");
            var blockPositionController = Instantiate(blockPositionControllerPrehab, blockPositionControllerInitialPos, worldAnchorObject.transform.rotation, worldAnchorObject.transform);
        } else
        {
            print("BlockPositioncontroller already exists");
        }
    }
}
