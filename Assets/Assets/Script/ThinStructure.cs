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
    public static List<Vector3> vertices;
    public static List<Edge> edges;
    public static List<Vector3> edgeVecs;
    public static List<Vector3> edgeCents;
    public static List<Vector3> splitNorms;
    public static List<GameObject> verticeGOs;
    public static List<GameObject> edgeGOs;
    public static int[][] neighborMap;
    public static List<HashSet<int>> linkInfo = new List<HashSet<int>>();
    public static int verticeNum;
    public static int edgeNum;
    float div = 20;
    float tubesize = 0.5f;
    // Use this for initialization
    void Start () {
        vertices = new List<Vector3>();
        edges = new List<Edge>();
        splitNorms = new List<Vector3>();
        edgeVecs = new List<Vector3>();
        edgeCents = new List<Vector3>();
        verticeGOs = new List<GameObject>();
        edgeGOs = new List<GameObject>();
        edgeGOs = new List<GameObject>();
    }

   
    public void restart() {
        GameObject collect = GameObject.Find("Collect");
        for (int i = 0; i < collect.transform.childCount; i++)
        {
            GameObject go = collect.transform.GetChild(i).gameObject;
            Destroy(go);
        }
        vertices.Clear();
        edges.Clear();
        splitNorms.Clear();
        edgeVecs.Clear();
        edgeCents.Clear();
        verticeGOs.Clear();
        edgeGOs.Clear();
    }
    
    void basicRead(int tar)
    {
        string line;
        string[] items;


        //read vert
        System.IO.StreamReader file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\thinstruct.txt");
        line = file.ReadLine(); items = line.Split(' ');
        verticeNum = int.Parse(items[0]);
        edgeNum = int.Parse(items[1]);
        //prepare data
        for (int i = 0; i < verticeNum; i++) linkInfo[i] = new HashSet<int>();
        neighborMap = new int[verticeNum][];
        for (int i = 0; i < neighborMap.Length; i++) neighborMap[i] = new int[verticeNum];
        for (int i = 0; i < verticeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            vertices.Add(new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2])));
        }
        for (int i = 0; i < edgeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            int v1 = int.Parse(items[0]);
            int v2 = int.Parse(items[1]);
            edges.Add(new Edge(v1, v2));
            neighborMap[v1][v2] = neighborMap[v2][v1] = i;
        }
        file.Close();

        //read linkInfo
        file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\splitinfo.txt");
        for (int i = 0; i < edgeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            splitNorms.Add(new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2])));
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
    }
    void basicPut()
    {
        for (int i = 0; i < verticeNum; i++)
        {
            Vector3 vertice = vertices[i];
            GameObject go = GameObject.Instantiate(Resources.Load("Sphere"), vertice / div, Quaternion.identity) as GameObject;
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

            //GameObject go = GameObject.Instantiate(Resources.Load("Column"), cent / div, fromto2) as GameObject;
            GameObject go = GameObject.Instantiate(Resources.Load("Cylinder"), cent / div, fromto2) as GameObject;
            go.transform.localScale = new Vector3(tubesize, (v1 - v2).magnitude / div / 2, tubesize);
            go.transform.parent = GameObject.Find("Collect").transform;
            edgeGOs.Add(go);
        }
    }
}
