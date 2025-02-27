﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JacDev.Entity;
using JacDev.Data;
using JacDev.Audio;

public class GameHandler : MonoBehaviour
{
    static GameHandler singleton = null;

    public static GameHandler Singleton
    {
        get
        {
            if (singleton != null)
                return singleton;

            singleton = FindObjectOfType(typeof(GameHandler)) as GameHandler;

            if (singleton == null)
            {
                GameObject g = new GameObject("GameHandler");
                singleton = g.AddComponent<GameHandler>();
            }

            return singleton;
        }
    }

    public bool debugMode;

    [Header("遊戲中資料")]
    public int credit;   // 後續是否整合進PlayerData?
    public int initCredit;

    static PlayerData playerData;
    public PlayerData PlayerData { get { return playerData; } }
    // WIP 音訊控制類別
    // public AudioHandler audioHandler;
    GameData gameData;

    public List<EntityObject> entities = new List<EntityObject>();

    [Header("設定")]
    public TowerList towerList;
    public EnemyList enemyList;

    [SerializeField] JacDev.Map.MapSetting mapSetting;
    public static JacDev.Map.MapSetting MapSetting
    {
        get => MapSetting;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(gameObject);
        }

        if (!hasAddSceneLoadAction)
        {
            hasAddSceneLoadAction = true;
            SceneManager.sceneLoaded += OnSceneLoad;
        }
    }

    private void Start()
    {
        credit = initCredit;
    }

    static bool hasAddSceneLoadAction;
    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        print($"載入 {scene.name}");
        switch (scene.name)
        {
            case "Title_v2":
                AudioHandler.Singleton.PlayBgm("OP");
                break;

            case "AsyncLoadingScene":
                //     Pause();
                //     break;

                // case "LoadGameScene":
                // if (FileManager.Load("/GameDatas", "/PlayerData") != null)
                // {
                //     playerData = FileManager.Load("/GameDatas", "/PlayerData");
                // }
                // else
                // {
                //     playerData = new PlayerData();
                //     FileManager.Save("/PlayerData", playerData, "/GameDatas");
                // }
                break;

            case "ShopScene":
                // if (FileManager.Load("/PlayerData", "/PlayerData") != null)
                // {
                //     playerData = FileManager.Load("/PlayerData", "/PlayerData");
                // }
                // else
                // {
                //     playerData = new PlayerData();
                //     FileManager.Save("/PlayerData", playerData, "/GameDatas");
                // }
                AudioHandler.Singleton.PlayBgm("Moring News");
                DataManager.Singleton.SavePlayerData();
                JacDev.Tutorial.TutorialManager.Singleton.Tutorial(0);  // 2022.3.22 新增
                break;

            case "GameScene":

                break;

            // for test
            case "GenerateTest new UI":
                // 設定並儲存Player Data
                DataManager dm = DataManager.Singleton;
                if (dm.GetMapData(false).FindPath(dm.PlayerData.currentPath) != null)
                {
                    JacDev.Level.LevelGenerator.Singleton.levelSetting = dm.GetMapData(false).FindPath(dm.PlayerData.currentPath).levelSetting;
                }
                dm.SavePlayerData();

                // 場景建置
                JacDev.Level.LevelGenerator.Singleton.BuildMap();
                AudioHandler.Singleton.RandPlayBgm();

                JacDev.Tutorial.TutorialManager.Singleton.Tutorial(21);  // 2022.3.22 新增

                break;
        }

        if (singleton == null)
            singleton = this;
        if (singleton != null && singleton != this)
            Destroy(gameObject);
    }

    public void Pause(bool b)
    {
        Time.timeScale = b ? 0 : 1;
    }

    private void Update()
    {

    }

    IEnumerator GameUpdate()
    {
        while (true)
        {
            for (int i = 0; i < entities.Count; ++i)
            {
                if (!entities[i].GameUpdate())
                {
                    entities.RemoveAt(i);
                }
            }
            yield return null;
        }
    }

    public void NewGame(int bloodIndex, bool bug = false)
    {
        ItemPriceData iData = new ItemPriceData();
        iData.Init();
        DataManager.Singleton.ItemPriceData = iData;

        PlayerData pData = new PlayerData();

        // pData.Init("New Player", bloodIndex, 10000f, DataManager.Singleton.GetMapData(true).commonStations[0]);
        if (bug)
        {
            pData.Init("封閉者", bloodIndex, 9999999f, DataManager.Singleton.GetMapData(true).stations1[0]);
            // iData
        }
        else
        {
            pData.Init("New Player", bloodIndex, 10000f, DataManager.Singleton.GetMapData(true).stations1[0]);
        }

        DataManager.Singleton.PlayerData = pData;

        AsyncSceneLoader.Singleton.LoadScene("ShopScene");
    }
}
