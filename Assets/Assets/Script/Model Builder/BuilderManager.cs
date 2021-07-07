using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderManager : MonoBehaviour {

    private IBuilderMode _mode = null;
    public static GraphManager graphManager;

    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    public static Material selectedMaterial;

    [SerializeField]
    public static Material standardMaterial;

    [SerializeField]
    public static Material tempMaterial;

    public Material m1, m2, m3;
	void Start () {
        selectedMaterial = m1;
        standardMaterial = m2;
        tempMaterial = m3;
        BuilderModeSetup();
        graphManager = new GraphManager();
        StartCoroutine(CheckMouseEvent());
	}
	
	void Update () {
		
	}

    void BuilderModeSetup()
    {
        _mode = new CreateMode(_prefab);
    }

    IEnumerator CheckMouseEvent()
    {
        while(true)
        {
            _mode.MouseOnThePosition();

            if (Input.GetMouseButtonDown(0))
            {
                _mode.ClickLeftButton();
            }
            if (Input.GetMouseButtonDown(1))
            {
                _mode.ClickRightButton();
            }

            yield return null;
        }
    }

    public void Output()
    {
        Debug.Log(graphManager.Output());
    }
}
