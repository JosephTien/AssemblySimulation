﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour {
    Transform ToDelete = null;
    // Use this for initialization
    void Start () {
        ToDelete = GameObject.Find("ToDelete").transform;
    }
    float deltaTime = 0;
    private void FixedUpdate()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime > 0.3 && ToDelete.childCount > 0) {
            Destroy(ToDelete.GetChild(0).gameObject);
        } 
    }
    // Update is called once per frame
    void Update () {
        
        
    }

    public void closeAllPanel() {
        GameObject canvas = GameObject.Find("Canvas");
        canvas.transform.Find("Panel_Lin").gameObject.SetActive(false);
        canvas.transform.Find("Panel_Edit").gameObject.SetActive(false);
        canvas.transform.Find("Panel_Generate").gameObject.SetActive(false);
        canvas.transform.Find("Panel_Simulate").gameObject.SetActive(false);
        canvas.transform.Find("Panel_Cube").gameObject.SetActive(false);
        Tool.clearObj();
    }
    public void openPanelPannel(int tar) {
        closeAllPanel();
        GameObject canvas = GameObject.Find("Canvas");
        if (tar == -1) canvas.transform.Find("Panel_Lin").gameObject.SetActive(true);
        if (tar == 0) canvas.transform.Find("Panel_Edit").gameObject.SetActive(true);
        if (tar == 1) canvas.transform.Find("Panel_Generate").gameObject.SetActive(true);
        if (tar == 2) canvas.transform.Find("Panel_Simulate").gameObject.SetActive(true);
        if (tar == 3) canvas.transform.Find("Panel_Cube").gameObject.SetActive(true);
    }

    public void restartScene() {
        Application.LoadLevel(Application.loadedLevel);
        Generator.forcebreak = true;
    }
}
