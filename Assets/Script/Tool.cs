using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static Vector3 randomVector() {
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public static Vector3 calPerpend(Vector3 baseVec, Vector3 toVec) {
        return Vector3.Cross(Vector3.Cross(baseVec, toVec), baseVec).normalized;
    }

    public static GameObject DrawBall(Vector3 pos, float radii, Color color) {
        GameObject go = GameObject.Instantiate(Resources.Load("Sphere")) as GameObject;
        go.transform.localScale = new Vector3(radii*2, radii * 2, radii * 2);
        go.transform.position = pos;
        go.GetComponent<MeshRenderer>().material.color = color;
        go.GetComponent<SphereCollider>().enabled = false;
        return go;
    }
    public static GameObject DrawLine(Vector3 pos1, Vector3 pos2, float radii, Color color)
    {
        GameObject go = GameObject.Instantiate(Resources.Load("Cylinder")) as GameObject;
        Vector3 vec = (pos2 - pos1).normalized;
        float len = (pos2 - pos1).magnitude;
        Vector3 norm = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        norm = Vector3.Cross(Vector3.Cross(vec, norm), vec);
        go.transform.localScale = new Vector3(radii*2, len/2, radii*2);
        go.transform.rotation = Quaternion.LookRotation(norm, vec);
        go.transform.position = (pos2 + pos1) / 2;
        go.GetComponent<MeshRenderer>().material.color = color;
        go.GetComponent<CapsuleCollider>().enabled = false;
        return go;
    }

    public static void clearObj() {

        GameObject go;
        go = GameObject.Find("Collect");
        for (int i = 0; i <go.transform.childCount;i++) {
            Destroy(go.transform.GetChild(i).gameObject);
        }
        go = GameObject.Find("Assist");
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Destroy(go.transform.GetChild(i).gameObject);
        }
        go = GameObject.Find("Sample");
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Destroy(go.transform.GetChild(i).gameObject);
        }
    }
    public static bool isParallel(Vector3 vec1, Vector3 vec2) {
        if (Mathf.Abs(Vector3.Dot(vec1.normalized, vec2.normalized)) > 0.99) return true;
        return false;
    }

    public static void setColor(GameObject go, Color color) {
        go.GetComponent<MeshRenderer>().material.color = color;
    }
    public static void resetColor()
    {
        Transform collect = GameObject.Find("Collect").transform;
        foreach (GameObject go in ThinStructure.verticeGOs)
        {
            //go.SetActive(true);
            Tool.setColor(go, Color.white);
        }
    }


    public static GameObject mouseSelect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    public static GameObject[] mouseSelects()
    {
        if (Input.GetMouseButtonDown(0))
        {


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hit = Physics.RaycastAll(ray, 10000);
            GameObject[] gos = new GameObject[hit.Length];
            for (int i = 0; i < hit.Length; i++) {
                gos[i] = hit[i].collider.gameObject;
            }
            return gos;
        }
        return null;
    }
    public static Vector3 nodenorm(int idx) {
        if (ThinStructure.verticesedges[idx].Count == 2) {
            int ea = -1, eb = -1;
            foreach (int e in ThinStructure.verticesedges[idx]) {
                if (ea == -1) ea = e;
                else if (eb == -1) eb = e;
            }
            Edge edgea = ThinStructure.edges[ea];
            Edge edgeb = ThinStructure.edges[eb];
            Vector3 veca = edgea.vec;
            if (edgea.idx2 == idx) veca *= -1;
            Vector3 vecb = edgeb.vec;
            if (edgeb.idx2 == idx) vecb *= -1;
            if (Mathf.Abs(Vector3.Dot(veca, vecb)) > 0.99f) return Vector3.zero;
            return (veca + vecb).normalized;
        }
        return Vector3.zero;
    }
}
