using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMode : IBuilderMode
{
    private GameObject _prefab;
    private GameObject _selectedObject = null;
    private GameObject _tempGameObject = null;
    private const float _DISTANCE_MULTIPLER = 25.0f;
    private Material _standardMaterial, _selectedMaterial, _tempMaterial;

    public CreateMode(GameObject prefab) : base(prefab)
    {
        _prefab = prefab;
        _standardMaterial = BuilderManager.standardMaterial;
        _selectedMaterial = BuilderManager.selectedMaterial;
        _tempMaterial = BuilderManager.tempMaterial;
    }

    public override void ClickLeftButton()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider.gameObject.CompareTag("Node"))
            {
                if(_selectedObject != null)
                {
                    _selectedObject.GetComponent<Renderer>().material = _standardMaterial;;
                    var lineObject = new GameObject("Line");
                    var renderer = lineObject.AddComponent<LineRenderer>();
                    var identity = lineObject.AddComponent<Identity>();
                    int from = _selectedObject.GetComponent<Identity>().component.GetIndex();
                    int to = hit.collider.gameObject.GetComponent<Identity>().component.GetIndex();
                    identity.component = new Line(from, to);
                    BuilderManager.graphManager.lineList.Add((Line)identity.component);
                    lineObject.tag = "Line";
                    renderer.SetPosition(0 ,_selectedObject.transform.position);
                    renderer.SetPosition(1, hit.transform.position);
                    _selectedObject = null;
                }
                else
                {
                    _selectedObject = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<Renderer>().material = _selectedMaterial;
                }
            }

        }
        else
        {
            var temp = ray.origin + _DISTANCE_MULTIPLER * ray.direction;
            var newObject = GameObject.Instantiate(_prefab, temp, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            var identity = newObject.GetComponent<Identity>();
            identity.component = new Node(BuilderManager.graphManager.currentIndex, newObject.transform.position);
            BuilderManager.graphManager.nodeList.Add((Node)identity.component);
            BuilderManager.graphManager.currentIndex++;
            if (_selectedObject != null)
            {
                _selectedObject.GetComponent<Renderer>().material = _standardMaterial;
            }
            _selectedObject = null;
            _tempGameObject = null;
        }
    }

    public override void ClickRightButton()
    {
        
    }

    public override void MouseOnThePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Node"))
            {
                if(_selectedObject != hit.collider.gameObject)
                {
                    hit.collider.gameObject.GetComponent<Renderer>().material = _tempMaterial;
                    _tempGameObject = hit.collider.gameObject;
                }
            }
        }
        else
        {
            if(_tempGameObject != null && _tempGameObject != _selectedObject)
            {
                _tempGameObject.GetComponent<Renderer>().material = _standardMaterial;
                _tempGameObject = null;
            }
        }
    }
}
