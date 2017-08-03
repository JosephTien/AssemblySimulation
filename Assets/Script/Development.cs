using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Development : MonoBehaviour {
    GameObject plane1, plane2, seam;
    float angle = 0;
    bool auto = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (seam && auto) {
            angle += 0.51f;
            angle %= 180;
            float tangle = angle > 90?180-angle:angle;
            tangle += -90;

            Vector3 vec1 = Quaternion.AngleAxis(45, new Vector3(0, 0, 1)) * new Vector3(0, 1, 0);
            Vector3 vec2 = Quaternion.AngleAxis(-45, new Vector3(0, 0, 1)) * new Vector3(0, 1, 0);
            Vector3 norm = Quaternion.AngleAxis(tangle, new Vector3(1, 0, 0)) * new Vector3(0, 1, 0);
            Vector3 norm1 = Tool.calPerpend(vec1, norm);
            Vector3 norm2 = Tool.calPerpend(vec2, norm);
            Vector3 axis = rotatePlane(norm1, vec1, norm2, vec2);
        }
	}

    void showAngle(Vector3 axis, Vector3 vec1, Vector3 vec2) {
        float angle1 = Vector3.Angle(axis, vec1);
        float angle2 = Vector3.Angle(axis, vec2);
        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = (int)angle1 + " : " + (int)angle2;
    }

    public Vector3 rotatePlane(Vector3 norm1, Vector3 vec1, Vector3 norm2, Vector3 vec2)
    {
        plane1.transform.rotation = Quaternion.LookRotation(vec1, norm1);
        plane2.transform.rotation = Quaternion.LookRotation(vec2, norm2);
        Vector3 axis = Vector3.Cross(norm1, norm2).normalized;
        Vector3 norm = Tool.calPerpend(axis, Tool.randomVector());
        seam.transform.rotation = Quaternion.LookRotation(norm, axis);
        if (Vector3.Dot(vec1, axis) < 0) axis *= -1;
        if (Vector3.Dot(vec2, axis) < 0) axis *= -1;
        showAngle(axis, vec1, vec2);
        return axis;
    }

    public void genPlaneTry() {
        Vector3 vec1 = Quaternion.AngleAxis(Random.Range(0,90), new Vector3(0, 0, 1)) * new Vector3(0, 1, 0);
        Vector3 vec2 = Quaternion.AngleAxis(Random.Range(-90, 0), new Vector3(0, 0, 1)) * new Vector3(0, 1, 0);
        Vector3 norm = Quaternion.AngleAxis(Random.Range(0, 180), new Vector3(1, 0, 0)) * new Vector3(0, 1, 0);
        Vector3 norm1 = Tool.calPerpend(vec1, norm);
        Vector3 norm2 = Tool.calPerpend(vec2, norm);
        genPlane();
        rotatePlane(norm1, vec1, norm2, vec2);
    }

    public void genPlane() {
        if (plane1) Destroy(plane1);
        if (plane2) Destroy(plane2);
        if (seam) Destroy(seam);
        plane1 = GameObject.Instantiate(Resources.Load("2D") as GameObject);
        plane2 = GameObject.Instantiate(Resources.Load("2D") as GameObject);
        plane1.transform.parent = GameObject.Find("Collect").transform;
        plane2.transform.parent = GameObject.Find("Collect").transform;
        plane1.transform.localScale = new Vector3(10, 10, 10);
        plane2.transform.localScale = new Vector3(10, 10, 10);
        Vector3 axis = new Vector3(0, 1, 0);
        seam = Tool.DrawLine(axis * 50, -axis * 50, 1, Color.gray);
        seam.transform.parent = GameObject.Find("Collect").transform;
    }
}
