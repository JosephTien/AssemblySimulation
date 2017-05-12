using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ctrl : MonoBehaviour {
    Vector3 mouseStart;
    bool mouseDown = false;
    float moveRate = 0.25f;
    Vector3 xaxis = new Vector3(0, 1, 0);
    Vector3 yaxis = new Vector3(1, 0, 0);
    Vector3 move = new Vector3(0, 0, 0);
    Vector3 realpos = new Vector3(0, 0, 0);
    public static bool valid = true;
    void Start () {
        realpos = Camera.main.gameObject.transform.position;
    }
	
	void Update () {
		
	}
    void FixedUpdate()
    {

        if (Input.GetMouseButton(0))
        {
            mouseDown = true;
        }
        else
        {
            mouseDown = false;
        }
        if (mouseDown&&valid)
        {
            Vector3 mouseDelta = Input.mousePosition - mouseStart;
            //Vector3 campos = Camera.main.gameObject.transform.position;
            Vector3 campos = realpos;
            Quaternion camrot = Camera.main.gameObject.transform.rotation;
            Quaternion rotx = Quaternion.AngleAxis(mouseDelta.x * moveRate, xaxis);
            Quaternion roty = Quaternion.AngleAxis(-mouseDelta.y * moveRate, yaxis);
            Quaternion rot = rotx * roty;
            realpos = rot * campos;
            Camera.main.gameObject.transform.rotation = rot * camrot;
            Camera.main.gameObject.transform.position = realpos + Camera.main.transform.rotation * move;
            xaxis = rot * xaxis;
            yaxis = rot * yaxis;
        }
        mouseStart = Input.mousePosition;
        if (!valid) return;
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
        {
            //Camera.main.transform.position *= 1.25f;
            realpos /= 1.25f;
            Camera.main.transform.position = realpos;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            //Camera.main.transform.position *= 0.8f;
            realpos *= 1.25f;
            Camera.main.transform.position = realpos;
        }
        /***********************************************/
        if (GameObject.Find("Canvas/Slider"))
            GameObject.Find("Main Camera/Directional Light").GetComponent<Light>().intensity = GameObject.Find("Canvas/Slider").GetComponent<UnityEngine.UI.Slider>().value;
    }
}
