using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Edge {
    public Edge(int idx1, int idx2) {
        if (idx1 > idx2) {
            int temp = idx1;
            idx1 = idx2;
            idx2 = temp;
        }
        this.idx1 = idx1;
        this.idx2 = idx2;
    }
    public int idx1;
    public int idx2;
}
public class ThinStructure : MonoBehaviour {
    public static Vector3[] vertices;
    public static Edge[] edges;
    public static Vector3[] splitNorms;
    public static List<Vector3> edgeVecs = new List<Vector3>();
    public static List<Vector3> edgeCents = new List<Vector3>();
    public static List<GameObject> verticeGOs = new List<GameObject>();
    public static List<GameObject> edgeGOs = new List<GameObject>();
    public static int[][] neighborMap;
    public static HashSet<int>[] verticesvertices;
    public static HashSet<int>[] verticesedges;
    public static HashSet<int>[] linkInfo;
    public static int verticeNum;
    public static int edgeNum;
    static float tubesize = 30f;
    // Use this for initialization
    void Start () {
        edgeVecs = new List<Vector3>();
        edgeCents = new List<Vector3>();
        verticeGOs = new List<GameObject>();
        edgeGOs = new List<GameObject>();
        edgeGOs = new List<GameObject>();
    }
    public static void basicRead(int tar)
    {
        string line;
        string[] items;

        //read vert
        System.IO.StreamReader file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\thinstruct.txt");
        line = file.ReadLine(); items = line.Split(' ');
        verticeNum = int.Parse(items[0]);
        edgeNum = int.Parse(items[1]);
        //prepare data
        vertices = new Vector3[verticeNum];
        edges = new Edge[edgeNum];
        splitNorms = new Vector3[edgeNum];
        neighborMap = new int[verticeNum][];
        linkInfo = new HashSet<int>[verticeNum];
        verticesvertices = new HashSet<int>[verticeNum];
        verticesedges = new HashSet<int>[verticeNum];
        for (int i = 0; i < verticeNum; i++) linkInfo[i] = new HashSet<int>();
        for (int i = 0; i < verticeNum; i++) verticesvertices[i] = new HashSet<int>();
        for (int i = 0; i < verticeNum; i++) verticesedges[i] = new HashSet<int>();
        for (int i = 0; i < neighborMap.Length; i++) neighborMap[i] = new int[verticeNum];
        //keep reading
        for (int i = 0; i < verticeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            vertices[i] = new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
        }
        for (int i = 0; i < edgeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            int v1 = int.Parse(items[0]);
            int v2 = int.Parse(items[1]);
            edges[i]= new Edge(v1, v2);
            neighborMap[v1][v2] = neighborMap[v2][v1] = i;
            verticesvertices[v1].Add(v2);
            verticesvertices[v2].Add(v1);
            verticesedges[v1].Add(i);
            verticesedges[v2].Add(i);
        }
        file.Close();

        //read splitInfo
        file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\splitinfo.txt");
        for (int i = 0; i < edgeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            splitNorms[i] = new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
            Vector3 vec = vertices[edges[i].idx1] - vertices[edges[i].idx2];
            splitNorms[i] = Vector3.Cross(Vector3.Cross(vec, splitNorms[i]), vec).normalized;
        }
        file.Close();

        //read link info
        file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\linkinfo.txt");
        int dataanum = int.Parse(file.ReadLine());
        for (int i = 0; i < dataanum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            int vi = int.Parse(items[0]);
            int ei = int.Parse(items[1]);
            linkInfo[vi].Add(ei);
        }
        file.Close();
        //fix link info
        for (int i = 0; i < verticeNum; i++) {
            if (verticesedges[i].Count == 1) {
                foreach(int e in verticesedges[i])linkInfo[i].Add(e);
            }
        }
    }
    public static void basicPut()
    {
        for (int i = 0; i < verticeNum; i++)
        {
            Vector3 vertice = vertices[i];
            GameObject go = GameObject.Instantiate(Resources.Load("Sphere"), vertice, Quaternion.identity) as GameObject;
            go.transform.localScale = new Vector3(tubesize, tubesize, tubesize);
            go.transform.parent = GameObject.Find("Collect").transform;
            verticeGOs.Add(go);
        }
        for (int i = 0; i < edgeNum; i++)
        {
            Edge edge = edges[i];
            Vector3 v1 = vertices[edge.idx1];
            Vector3 v2 = vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized; edgeVecs.Add(vec);
            Vector3 cent = (v2 + v1) / 2; edgeCents.Add(cent);
            Vector3 norm = splitNorms[i].normalized;
            norm = Vector3.Cross(Vector3.Cross(vec, norm), vec);
            Quaternion fromto = Quaternion.FromToRotation(new Vector3(0, 1, 0), vec);
            Quaternion fromto2 = Quaternion.LookRotation(norm, vec);
            Quaternion fromto3 = Quaternion.LookRotation(vec, norm);

            GameObject go = GameObject.Instantiate(Resources.Load("Cylinder"), cent, fromto2) as GameObject;
            GameObject go2 = GameObject.Instantiate(Resources.Load("Plane"), cent, fromto2) as GameObject;
            go.transform.localScale = new Vector3(tubesize, (v1 - v2).magnitude / 2, tubesize);
            go.transform.parent = GameObject.Find("Collect").transform;
            go2.transform.localScale = new Vector3(tubesize, (v1 - v2).magnitude / 2, tubesize);
            go2.transform.parent = go.transform.parent;
            edgeGOs.Add(go);
        }
    }
}
