﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectSwitcher : MonoBehaviour
{
    public List<Button> buttons = new List<Button>();
    public List<EventTrigger> eventTriggers = new List<EventTrigger>();

    public List<GameObject> objects = new List<GameObject>();
    public bool hideAtStart;
    public int defaultIndex = 0;
    public int currentIndex = 0;
    public int oldIndex = 0;

    public System.Action<int, int> OnSwitch; // (NewIndex,OldIndex)
    public bool autoBind = false;
    public bool tabMode = false;
    public Color tabModeColor;

    private void Start()
    {
        if (autoBind)
        {
            if (buttons.Count == 0)
                BindEvent<EventTrigger>(eventTriggers);
            else
                BindEvent<Button>(buttons);
        }

        if (hideAtStart)
            Switch(defaultIndex);

    }

    void BindEvent<T>(List<T> list)
    {
        if (list.GetType() == typeof(List<Button>))
        {
            var newList = list as List<Button>;
            int iter = 0;
            foreach (Button v in newList)
            {
                int temp = iter;
                v.onClick.AddListener(() => Switch(temp));
                iter++;
            }
        }
        else
        {
            var newList = list as List<EventTrigger>;
            int iter = 0;
            foreach (EventTrigger v in newList)
            {
                int temp = iter;
                JacDev.Utils.EventBinder.Bind(v, EventTriggerType.PointerClick, (e) => Switch(temp));
                iter++;
            }
        }
    }



    public void Switch(int index)
    {
        for (int i = 0; i < objects.Count; ++i)
        {
            if (objects[i] != null)
                objects[i].SetActive(i == index);
        }
        oldIndex = currentIndex;
        currentIndex = index;

        if (OnSwitch != null)
            OnSwitch.Invoke(currentIndex, oldIndex);

        if (tabMode)
        {
            int iter = 0;
            if (buttons.Count != 0)
            {
                foreach (var v in buttons)
                {
                    v.image.color = iter == index ? Color.white : tabModeColor;
                    iter++;
                }
            }
        }
    }

    public void Next()
    {
        if (currentIndex < objects.Count - 1)
            Switch(currentIndex + 1);
    }

    public void Back()
    {
        if (currentIndex > 0)
            Switch(currentIndex - 1);
    }
}
