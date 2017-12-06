using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ctrl : MonoBehaviour {
    Vector3 mouseStart;
    bool mouseDown = false;
    float moveRate = 0.25f;
    static Vector3 xaxis = new Vector3(0, 1, 0);
    static Vector3 yaxis = new Vector3(1, 0, 0);
    static Vector3 move = new Vector3(0, 0, 0);
    static Vector3 realpos = new Vector3(0, 0, 0);
    public static bool valid = true;
    float scrollVal = 30;
    bool reverse = false;
    static int state = 0;
    static int statenum = 8;
    void Start () {
        realpos = Camera.main.gameObject.transform.position;
    }
	
	void Update () {
		
	}
    public static void rotateView(float rx, float ry) {
        //Vector3 campos = Camera.main.gameObject.transform.position;
        Vector3 campos = realpos;
        Quaternion camrot = Camera.main.gameObject.transform.rotation;
        Quaternion rotx = Quaternion.AngleAxis(rx, xaxis);
        Quaternion roty = Quaternion.AngleAxis(ry, yaxis);
        Quaternion rot = rotx * roty;
        realpos = rot * campos;
        Camera.main.gameObject.transform.rotation = rot * camrot;
        Camera.main.gameObject.transform.position = realpos + Camera.main.transform.rotation * move;
        xaxis = rot * xaxis;
        yaxis = rot * yaxis;
    }
    public static void setView(int statetar) {
        state = statetar - 1;
        defaultView();
    }
    public static void defaultView() {
        state = (state + 1) % statenum;
        xaxis = new Vector3(0, 1, 0);
        yaxis = new Vector3(1, 0, 0);
        move = new Vector3(0, 0, 0);
        realpos = new Vector3(0, 0, -250);
        Quaternion camrot = Quaternion.identity;
        if (state == 0) ;
        else if (state == 1) camrot = Quaternion.Euler(90, 0, 0);
        else if (state == 2) camrot = Quaternion.Euler(0, 90, 0);
        else if (state == 3) camrot = Quaternion.Euler(0, 0, 90);
        else if (state == 4) camrot = Quaternion.Euler(-90, 0, 0);
        else if (state == 5) camrot = Quaternion.Euler(0, -90, 0);
        else if (state == 6) camrot = Quaternion.Euler(0, 0, -90);
        else if (state == 7) camrot = Quaternion.Euler(0, -180, 0);
        Camera.main.gameObject.transform.rotation = camrot;
        realpos = camrot* realpos;
        Camera.main.gameObject.transform.position = realpos;
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
            float fixedScrollVal = scrollVal * realpos.magnitude / 250;
            if (realpos.magnitude < 100) fixedScrollVal = scrollVal;
             Vector3 realposmove = realpos.normalized * fixedScrollVal;
            if (reverse) realposmove *= -1;
            if (!reverse && realpos.magnitude < scrollVal) reverse = true;
            realpos = realpos - realposmove;
            //realpos = (realpos.magnitude - scrollVal) / realpos.magnitude * realpos;
            Camera.main.transform.position = realpos;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            //Camera.main.transform.position *= 0.8f;
            float fixedScrollVal = scrollVal * realpos.magnitude / 250;
            if (realpos.magnitude < 100) fixedScrollVal = scrollVal;
            Vector3 realposmove = realpos.normalized * fixedScrollVal;
            if (reverse) realposmove *= -1;
            if (reverse && realpos.magnitude < scrollVal) reverse = false;
            realpos = realpos + realposmove;
            //realpos = (realpos.magnitude + scrollVal) / realpos.magnitude * realpos;
            Camera.main.transform.position = realpos;
        }
        /***********************************************/
        if (GameObject.Find("Canvas/Slider"))
            GameObject.Find("Main Camera/Directional Light").GetComponent<Light>().intensity = GameObject.Find("Canvas/Slider").GetComponent<UnityEngine.UI.Slider>().value;
    }
}
