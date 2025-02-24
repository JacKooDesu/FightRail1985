﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JacDev.Level;

namespace JacDev.Entity
{
    public class TrainLine : MonoBehaviour
    {
        public TrainObject[] trains;

        public float speed; // should be static?
        public static float hasMove = 0;
        bool finished = false;

        KeyCode[] switchKey = {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
        };

        KeyCode firstPersonViewKey = KeyCode.Minus;
        public RenderTexture frontViewTexture;

        public Camera firstPersonViewCamera;
        Camera originCamera;


        private void Start()
        {
            if (GameHandler.Singleton.debugMode)
                return;

            originCamera = Camera.main;
            if (firstPersonViewCamera)
                firstPersonViewCamera.enabled = false;

            float lengthTemp = 0;

            for (int i = 0; i < trains.Length; ++i)
            {
                // print(lengthTemp);
                trains[i].transform.localPosition = new Vector3(0, 0, lengthTemp);
                lengthTemp -= trains[i].Length;

                // Setting RenderTexture
                if (i == 0 && frontViewTexture)
                {
                    trains[i].GetComponentInChildren<Camera>().targetTexture = frontViewTexture;
                }
            }

            // reset position
            transform.position = Vector3.zero;
            hasMove = 0;

            // EnemyObject.target = this;
            JacDev.UI.GameScene.GameSceneUIHandler.Singleton.trackingTrain = trains[0];
        }

        private void Update()
        {
            if (GameHandler.Singleton.debugMode)
                return;

            TestMove();
            SwitchFocusTrain();

            if (Input.GetKeyDown(firstPersonViewKey))
            {
                SwitchCamera();
            }
        }

        void TestMove()
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
            hasMove += Time.deltaTime * speed;

            if (hasMove >= LevelGenerator.Singleton.totalLength && !finished)
            {
                SwitchCamera(originCamera);
                Camera.main.GetComponent<OrbitCamera>().SetFocus(LevelGenerator.Singleton.dest);
                AsyncSceneLoader.Singleton.LoadScene("ShopScene", 2f);
                finished = true;

                // 2021.10.23 added (for test only, will remove in future)
                DataManager.Singleton.PlayerData.currentStation = DataManager.Singleton.PlayerData.nextStation;
                DataManager.Singleton.PlayerData.nextStation = null;
            }
        }

        void SwitchCamera()
        {
            originCamera.enabled = !originCamera.enabled;
            firstPersonViewCamera.enabled = !firstPersonViewCamera.enabled;
        }

        void SwitchCamera(Camera cam)
        {
            if (Camera.main == cam)
                return;
            else
                SwitchCamera();
        }

        void SwitchFocusTrain()
        {
            for (int i = 0; i < trains.Length; ++i)
            {
                if (Input.GetKeyDown(switchKey[i]))
                {
                    if (Camera.main.GetComponent<OrbitCamera>())
                        Camera.main.GetComponent<OrbitCamera>().SetFocus(trains[i].transform);

                    JacDev.UI.GameScene.GameSceneUIHandler.Singleton.trackingTrain = trains[i];
                }
            }
        }
    }
}

