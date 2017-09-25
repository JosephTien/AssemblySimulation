using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class Tool : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public static Transform ToDelete;
    public static void deleteObj(GameObject go) {
        if (ToDelete == null) ToDelete = GameObject.Find("ToDelete").transform;
        go.transform.parent = ToDelete;
    }

    public static Vector3 randomVector() {
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public static Vector3 calPerpend(Vector3 baseVec, Vector3 toVec) {
        return Vector3.Cross(Vector3.Cross(baseVec, toVec), baseVec).normalized;
    }
    public static Color RandomColor() {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }
    public static Color RandomBrightColor()
    {
        return new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
    }
    public static Mesh getMesh(GameObject go) {
        return go.GetComponent<MeshFilter>().mesh;
    }
    public static Vector3 getCenter(GameObject go)
    {
        Vector3 c = go.GetComponent<MeshFilter>().mesh.bounds.center;
        Vector3 s = go.transform.localScale;
        return new Vector3(c.x * s.x, c.y * s.y, c.z * s.z);
    }

    public static float area(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = (v1 - v2).magnitude;
        float b = (v2 - v3).magnitude;
        float c = (v1 - v3).magnitude;
        float s = (a + b + c) / 2;
        return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c)) / 2;
    }

    public static Vector3 getExCenter(GameObject go)
    {
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;
        Vector3 c = Vector3.zero;
        for (int i = 0; i < mesh.vertices.Length; i += 3)
        {
            c += mesh.vertices[i];
        }
        c /= mesh.vertices.Length;
        /*
        float sum = 0;
        for (int i = 0; i < mesh.triangles.Length; i+=3) {
            Vector3 v1 = mesh.vertices[mesh.triangles[i]];
            Vector3 v2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 v3 = mesh.vertices[mesh.triangles[i + 2]];
            float a = area(v1, v2, v3);
            c += (v1 + v2 + v3) / 3 * a;
            sum += a;
        }
        c = c / sum;
        */
        Vector3 s = go.transform.localScale;
        return new Vector3(c.x * s.x, c.y * s.y, c.z * s.z);
    }
    public static GameObject DrawCube(Vector3 pmax, Vector3 pmin, Color color)
    {
        GameObject go = GameObject.Instantiate(Resources.Load("Cube")) as GameObject;
        Vector3 cent = (pmax + pmin) / 2;
        Vector3 up = (new Vector3(cent.x, cent.y, pmax.z) - cent).normalized;
        Vector3 forw = (new Vector3(cent.x, pmax.y, cent.z) - cent).normalized;
        go.transform.localScale = new Vector3(Mathf.Abs(pmax.x - pmin.x), Mathf.Abs(pmax.y - pmin.y), Mathf.Abs(pmax.z - pmin.z));
        go.transform.position = cent;
        //go.transform.rotation = Quaternion.LookRotation(forw, up);
        go.GetComponent<MeshRenderer>().material.color = color;
        return go;
    }
    public static GameObject DrawCubeMesh(Vector3 pmax, Vector3 pmin, Color color)
    {
        GameObject go = GameObject.Instantiate(Resources.Load("cubemesh")) as GameObject;
        Vector3 cent = (pmax + pmin) / 2;
        Vector3 up = (new Vector3(cent.x, cent.y, pmax.z) - cent).normalized;
        Vector3 forw = (new Vector3(cent.x, pmax.y, cent.z) - cent).normalized;
        go.transform.localScale = new Vector3(Mathf.Abs(pmax.x - pmin.x), Mathf.Abs(pmax.y - pmin.y), Mathf.Abs(pmax.z - pmin.z));
        go.transform.position = cent;
        //go.transform.rotation = Quaternion.LookRotation(forw, up);
        go.GetComponent<MeshRenderer>().material.color = color;
        return go;
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
    public static void clearObj_immed()
    {
        GameObject go;
        Transform[] trs;
        Transform toDelete = GameObject.Find("ToDelete").transform;

        go = GameObject.Find("Collect");
        trs = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) if (tr.gameObject != go) Destroy(tr.gameObject);

        go = GameObject.Find("Assist");
        trs = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) if (tr.gameObject != go) Destroy(tr.gameObject);

        go = GameObject.Find("Sample");
        trs = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) if (tr.gameObject != go) Destroy(tr.gameObject);
    }
    public static void clearObj() {
        /*
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
        */
        GameObject go;
        Transform[] trs;
        Transform toDelete = GameObject.Find("ToDelete").transform;

        go = GameObject.Find("Collect");
        trs = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) if (tr.gameObject != go) tr.parent = toDelete;

        go = GameObject.Find("Assist");
        trs = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) if (tr.gameObject != go) tr.parent = toDelete;
        
        go = GameObject.Find("Sample");
        trs = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) if (tr.gameObject != go) tr.parent = toDelete;
    }
    public static IEnumerator clearTodelete() {
        yield return new WaitForSeconds(0.5f);
        GameObject go;
        go = GameObject.Find("ToDelete");
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Destroy(go.transform.GetChild(i).gameObject);
        }
        yield return 0;
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
    static public float calVolume(GameObject instance)
    {
        Mesh mesh = instance.GetComponent<MeshFilter>().mesh;
        Vector3 s = instance.transform.localScale;
        float volume = 0;
        /*
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 v = mesh.vertices[i];
            mesh.vertices[i] = new Vector3(v.x * s.x, v.y * s.y, v.z * s.z) ;
        }
        instance.GetComponent<MeshFilter>().mesh = mesh;
        instance.transform.localScale = new Vector3(1,1,1);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        */
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            volume += SignedVolumeOfTriangle(mesh.vertices[mesh.triangles[i]] * s.x, mesh.vertices[mesh.triangles[i + 1]] * s.y, mesh.vertices[mesh.triangles[i + 2]] * s.z);
        }
        volume = Mathf.Abs(volume);
        return volume;
    }
    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var v321 = p3.x * p2.y * p1.z;
        var v231 = p2.x * p3.y * p1.z;
        var v312 = p3.x * p1.y * p2.z;
        var v132 = p1.x * p3.y * p2.z;
        var v213 = p2.x * p1.y * p3.z;
        var v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
    public static void inputMesh(GameObject obj, string file)
    {
        Mesh holderMesh = new Mesh();
        ObjImporter newMesh = new ObjImporter();
        holderMesh = newMesh.ImportFile(file);
        if (obj.GetComponent<MeshRenderer>() == null) obj.AddComponent<MeshRenderer>();
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (obj.GetComponent<MeshFilter>() == null) obj.AddComponent<MeshFilter>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        holderMesh.RecalculateNormals();
        filter.mesh = holderMesh;
        obj.transform.localScale = Vector3.one;
    }
    public static void indMesh(GameObject obj)
    {
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < filter.mesh.triangles.Length; i += 3) {
            vertices.Add(filter.mesh.vertices[filter.mesh.triangles[i]]);
            vertices.Add(filter.mesh.vertices[filter.mesh.triangles[i + 1]]);
            vertices.Add(filter.mesh.vertices[filter.mesh.triangles[i + 2]]);
            triangles.Add(i);
            triangles.Add(i+1);
            triangles.Add(i+2);
        }
        filter.mesh.vertices = vertices.ToArray();
        filter.mesh.triangles = triangles.ToArray();
        filter.mesh.RecalculateNormals();
        filter.mesh.RecalculateBounds();
    }

    public static void cmpVert(Vector3 vert, ref Vector3 min, ref Vector3 max) {
        min.x = vert.x < min.x ? vert.x : min.x;
        min.y = vert.y < min.y ? vert.y : min.y;
        min.z = vert.z < min.z ? vert.z : min.z;
        max.x = vert.x > max.x ? vert.x : max.x;
        max.y = vert.y > max.y ? vert.y : max.y;
        max.z = vert.z > max.z ? vert.z : max.z;
    }
}
