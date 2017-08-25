using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
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
    public void swap() {
        int t = idx1;
        idx1 = idx2;
        idx2 = t;
    }
    public Vector3 vec { get { return (ThinStructure.vertices[idx2] - ThinStructure.vertices[idx1]).normalized; } }
    public Vector3 cent { get { return (ThinStructure.vertices[idx2] + ThinStructure.vertices[idx1]) / 2; } }
    public float len {get{return (ThinStructure.vertices[idx2] - ThinStructure.vertices[idx1]).magnitude;}}
    public int idx1;
    public int idx2;
    public int group;
    public float minAngle1;
    public float minAngle2;
    public float fixDis1;
    public float fixDis2;
}

public class CompInfo {
    public CompInfo() {
        group_u = -1;
        group_d = -1;
        norm = Vector3.zero;
        angleArg = new AngleArg();
    }
    public int group_u;
    public int group_d;
    public Vector3 norm;
    public AngleArg angleArg;

}

public class ThinStructure : MonoBehaviour {
    public static Vector3[] vertices;
    public static Edge[] edges;
    public static Vector3[] splitNorms;
    public static bool[] splitReverse;
    public static AngleArg[] angleArgs;
    public static CompInfo[] compinfo_edge;
    public static CompInfo[] compinfo_vert;
    public static List<Vector3> edgeVecs = new List<Vector3>();
    public static List<Vector3> edgeCents = new List<Vector3>();
    public static GameObject[] verticeGOs;
    public static GameObject[] edgeGOs;
    public static int[][] neighborMap;
    public static int[][] edgeConnMap;
    public static HashSet<int>[] verticesvertices;
    public static HashSet<int>[] verticesedges;
    public static HashSet<int>[] linkInfo;
    public static int[] solvedinfo;
    public static int verticeNum;
    public static int edgeNum;
    public static float tuberadii = 15f;
    public static int curSet;
    public static float myscale = 1;
    // Use this for initialization
    void Start () {
        edgeVecs = new List<Vector3>();
        edgeCents = new List<Vector3>();
    }
    public static void prepareData() {
        //prepare data
        vertices = new Vector3[verticeNum];
        verticeGOs = new GameObject[verticeNum];
        edges = new Edge[edgeNum];
        edgeGOs = new GameObject[edgeNum];
        splitNorms = new Vector3[edgeNum];
        splitReverse = new bool[edgeNum];
        angleArgs = new AngleArg[edgeNum];
        compinfo_edge = new CompInfo[edgeNum];
        compinfo_vert = new CompInfo[verticeNum];
        neighborMap = new int[verticeNum][];
        edgeConnMap = new int[edgeNum][];
        linkInfo = new HashSet<int>[verticeNum];
        verticesvertices = new HashSet<int>[verticeNum];
        verticesedges = new HashSet<int>[verticeNum];
        for (int i = 0; i < edgeNum; i++) angleArgs[i] = new AngleArg();
        for (int i = 0; i < edgeNum; i++) compinfo_edge[i] = new CompInfo();
        for (int i = 0; i < verticeNum; i++) compinfo_vert[i] = new CompInfo();
        for (int i = 0; i < verticeNum; i++) linkInfo[i] = new HashSet<int>();
        for (int i = 0; i < verticeNum; i++) verticesvertices[i] = new HashSet<int>();
        for (int i = 0; i < verticeNum; i++) verticesedges[i] = new HashSet<int>();
        for (int i = 0; i < verticeNum; i++)
        {
            neighborMap[i] = new int[verticeNum];
            for (int j = 0; j < verticeNum; j++) neighborMap[i][j] = -1;
        }
        for (int i = 0; i < edgeNum; i++)
        {
            edgeConnMap[i] = new int[edgeNum];
            for (int j = 0; j < edgeNum; j++) edgeConnMap[i][j] = -1;
        }
    }

    public static int groupnum;
    public static HashSet<int>[] vertgroup;
    public static HashSet<int> importantVert;
    public static void readLin(int tar) {
        Tool.clearObj();
        List<Vector3> vertList = new List<Vector3>();
        List<Edge> edgeList = new List<Edge>();
        System.IO.StreamReader file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\thinstruct.lin");
        string line;
        string[] items;
        int curgroup = 0;
        while (!file.EndOfStream) {
            line = file.ReadLine();
            items = line.Split(' ');
            if (items.Length <= 1) { continue; }
            else if (items[0] == "v")
            {
                Vector3 v = new Vector3(float.Parse(items[1]), float.Parse(items[2]), float.Parse(items[3]));
                vertList.Add(v);
                verticeNum = curgroup;
            }
            else if (items[0] == "l")
            {
                int v1 = int.Parse(items[1]);
                int v2 = int.Parse(items[2]);
                Edge e = new Edge(v1 - 1, v2 - 1);
                e.group = curgroup;
                edgeList.Add(e);
            }
            else if (items[0] == "g")
            {
                curgroup = int.Parse(items[1].Substring(("polyline").Length));
                groupnum = groupnum > curgroup ? groupnum : curgroup;
            }
        }
        file.Close();
        //////////////////////////////
        verticeNum = vertList.Count;
        edgeNum = edgeList.Count;
        prepareData();
        vertgroup = new HashSet<int>[verticeNum];
        int i = 0;
        foreach (Vector3 v in vertList)
        {
            vertices[i] = v;
            vertices[i] *= myscale;
            vertgroup[i] = new HashSet<int>();
            i++;
        }
        //////////////////////////////
        i = 0;
        foreach (Edge e in edgeList)
        {
            int v1 = e.idx1;
            int v2 = e.idx2;
            vertgroup[v1].Add(e.group);
            vertgroup[v2].Add(e.group);
            edges[i++] = e;
        }
        reGenDetail();
        importantVert = new HashSet<int>();
    }
    public static void genVertGroup() {
        vertgroup = new HashSet<int>[verticeNum];
        for(int i=0;i< verticeNum;i++)
        {
            vertgroup[i] = new HashSet<int>();
        }
        foreach (Edge e in edges)
        {
            vertgroup[e.idx1].Add(e.group);
            vertgroup[e.idx2].Add(e.group);
        }
    }
    
    public static void readFromImpoSet(int tar)
    {
        curSet = tar;
        string line;
        string[] items;
        int secNum = 0;
        List<Edge> impoSet = new List<Edge>();
        //read vert
        System.IO.StreamReader file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\thinstruct_.txt");
        line = file.ReadLine(); items = line.Split(' ');
        verticeNum = int.Parse(items[0]);
        secNum = int.Parse(items[1]);

        Vector3[] vertices_temp = new Vector3[verticeNum];
        for (int i = 0; i < verticeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            vertices_temp[i] = new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
            vertices_temp[i] *= myscale;
        }
        for (int i = 0; i < secNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            int v1 = int.Parse(items[0]);
            int v2 = int.Parse(items[1]);
            impoSet.Add(new Edge(v1, v2));
        }
        file.Close();
        edgeNum = 0;
        List<Edge> edges_temp = new List<Edge>();
        importantVert = new HashSet<int>();
        groupnum = 0;
        foreach (Edge e in impoSet)
        {
            int a = e.idx1;
            int b = e.idx2;
            importantVert.Add(a);
            importantVert.Add(b);
            edgeNum += b - a;
            for (int i = a; i < b; i++) {
                Edge ee = new Edge(i-1, i);
                ee.group = groupnum;
                edges_temp.Add(ee);
            }
            groupnum++;
        } 
        prepareData();
        vertices = vertices_temp;
        edges = edges_temp.ToArray();
        reGenDetail();
    }
   
    public static void basicRead(int tar)
    {
        curSet = tar;
        string line;
        string[] items;
        //read vert
        System.IO.StreamReader file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\thinstruct.txt");
        line = file.ReadLine(); items = line.Split(' ');
        verticeNum = int.Parse(items[0]);
        edgeNum = int.Parse(items[1]);
        //prepare data
        prepareData();
        //keep reading
        for (int i = 0; i < verticeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            vertices[i] = new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
            vertices[i] *= myscale;
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
        //set edgeConnMap
        for (int k = 0; k < verticeNum; k++) {
            foreach (int i in verticesedges[k]) {
                foreach (int j in verticesedges[k])
                {
                    edgeConnMap[i][j] = edgeConnMap[j][i] = k;
                }
            }
        }

        //fix angle distance
        fixAngleDistance();

        //read splitInfo
        if (File.Exists(".\\inputSet\\" + tar + "\\input\\splitinfo.txt")) {
            file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\splitinfo.txt");
            for (int i = 0; i < edgeNum; i++)
            {
                line = file.ReadLine(); items = line.Split(' ');
                if (items.Length <= 1) { i--; continue; }
                splitNorms[i] = new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
                Vector3 vec = vertices[edges[i].idx1] - vertices[edges[i].idx2];
                splitNorms[i] = Vector3.Cross(Vector3.Cross(vec, splitNorms[i]), vec).normalized;
                if (items.Length == 7) {
                    angleArgs[i].angle2 = float.Parse(items[3]);
                    angleArgs[i].angle3 = float.Parse(items[4]);
                    angleArgs[i].angle2_2 = float.Parse(items[5]);
                    angleArgs[i].angle3_2 = float.Parse(items[6]);
                }
                if (items.Length == 9)
                {
                    angleArgs[i].dir1 = new Vector3(float.Parse(items[3]), float.Parse(items[4]), float.Parse(items[5]));
                    angleArgs[i].dir2 = new Vector3(float.Parse(items[6]), float.Parse(items[7]), float.Parse(items[8]));
                }
            }
            file.Close();
        }
        
        //read link info
        if (File.Exists(".\\inputSet\\" + tar + "\\input\\linkinfo.txt"))
        {
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
        //fix link info
        fixLinkinfo();

        //read solved info
        if (File.Exists(".\\inputSet\\" + tar + "\\input\\solvedinfo.txt"))
        {
            file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\solvedinfo.txt");
            int num = verticeNum + edgeNum;
            solvedinfo = new int[num];
            for (int i = 0; i < num; i++)
            {
                line = file.ReadLine();
                solvedinfo[i]=int.Parse(line);
            }
            file.Close();
        }
        
        //read compinfo(group)
        if (File.Exists(".\\inputSet\\" + tar + "\\input\\groupinfo.txt"))
        {
            file = new System.IO.StreamReader(".\\inputSet\\" + tar + "\\input\\groupinfo.txt");
            for (int i = 0; i < verticeNum; i++)
            {
                line = file.ReadLine();
                items = line.Split(' ');
                compinfo_vert[i].group_u = int.Parse(items[0]);
                compinfo_vert[i].group_d = int.Parse(items[1]);
            }
            for (int i = 0; i < edgeNum; i++)
            {
                line = file.ReadLine();
                items = line.Split(' ');
                compinfo_edge[i].group_u = int.Parse(items[0]);
                compinfo_edge[i].group_d = int.Parse(items[1]);
            }
            file.Close();
        }

    }

    public static void addEdge(int vi1, int vi2) {
        int newgroup = -1;
        foreach (int g1 in vertgroup[vi1])
        {
            foreach (int g2 in vertgroup[vi2])
            {
                if (g1 == g2) {//should be impossable
                    newgroup = g1;
                }
            }
        }
        if (newgroup == -1) {
            newgroup = groupnum++;
        }
        List<Edge> newedges = new List<Edge>(edges);
        Edge e = new Edge(vi1, vi2);
        e.group = newgroup;
        newedges.Add(e);
        reGenDetail(vertices, newedges.ToArray());
    }
    public static void delEdge(int vi1, int vi2)
    {
        int ei = neighborMap[vi1][vi2];
        List<Edge> newedges = new List<Edge>(edges);
        newedges.RemoveAt(ei);
        reGenDetail(vertices, newedges.ToArray());
    }


    public static void reGenDetail(Vector3[] vertices, Edge[] edges)
    {
        verticeNum = vertices.Length;
        edgeNum = edges.Length;
        prepareData();
        ThinStructure.vertices = vertices;
        ThinStructure.edges = edges;
        reGenDetail();
    }
    public static void reGenDetail() {
        for (int i = 0; i < edgeNum; i++)
        {
            int v1 = edges[i].idx1;
            int v2 = edges[i].idx2;
            neighborMap[v1][v2] = neighborMap[v2][v1] = i;
            verticesvertices[v1].Add(v2);
            verticesvertices[v2].Add(v1);
            verticesedges[v1].Add(i);
            verticesedges[v2].Add(i);
        }
        for (int k = 0; k < verticeNum; k++)
        {
            foreach (int i in verticesedges[k])
            {
                foreach (int j in verticesedges[k])
                {
                    edgeConnMap[i][j] = edgeConnMap[j][i] = k;
                }
            }
        }
    }

    public static void fixLinkinfo() {
        for (int i = 0; i < verticeNum; i++)
        {
            if (verticesedges[i].Count == 1)
            {
                foreach (int e in verticesedges[i]) linkInfo[i].Add(e);
            }
        }
    }

    public static void outputThin(int setnum) {
        //write linkinfo
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Format("{0} {1}\n", verticeNum, edgeNum));

        for (int i = 0; i < verticeNum; i++)
        {
            Vector3 v = vertices[i] / myscale;
            sb.Append(string.Format("{0} {1} {2}\n", v.x, v.y, v.z));
        }
        for (int i = 0; i < edgeNum; i++)
        {
            sb.Append(string.Format("{0} {1}\n", edges[i].idx1, edges[i].idx2));
        }
        string filename = "inputSet\\" + setnum + "\\input\\thinstruct.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }

    public static void outputlinkInfo(int setnum)
    {
        //write linkinfo
        StringBuilder sb = new StringBuilder();
        int cnt = 0;
        for (int i = 0; i < linkInfo.Length; i++) {
            cnt += linkInfo[i].Count;
        }
        sb.Append(string.Format("{0}\n", cnt));

        for (int i = 0; i < linkInfo.Length; i++)
        {
            foreach (int j in linkInfo[i]) {
                sb.Append(string.Format("{0} {1}\n", i, j));
            }
        }
        string filename = "inputSet\\" + setnum + "\\input\\linkinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }
    public static void reverseAllSplitNorm() {
        for (int i = 0; i < splitReverse.Length; i++) {
            splitReverse[i] = true;
        }
    }
    public static void applyReverseSplitNorm()
    {
        for (int i = 0; i < splitReverse.Length; i++) {
            if (splitReverse[i]) splitNorms[i] *= -1;
        }
    }
    public static void outputsplitNorms(int setnum) {
        //write splitInfo
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < edgeNum; i++)
        {
            Vector3 splitNorm = splitNorms[i];
            if (splitReverse[i]) splitNorm *= -1;
            if (angleArgs[i].hasDirDefine())
            {
                sb.Append(string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}\n", splitNorm.x, splitNorm.y, splitNorm.z,
                                                                    angleArgs[i].dir1.x, angleArgs[i].dir1.y, angleArgs[i].dir1.z,
                                                                    angleArgs[i].dir2.x, angleArgs[i].dir2.y, angleArgs[i].dir2.z));
            }
            else {
                sb.Append(string.Format("{0} {1} {2} {3} {4} {5} {6}\n", splitNorm.x, splitNorm.y, splitNorm.z,
                                                                    angleArgs[i].angle2, angleArgs[i].angle2_2, angleArgs[i].angle3, angleArgs[i].angle3_2));
            }
        }
        string filename = "inputSet\\" + setnum + "\\input\\splitinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }
    public static void fixAngleDistance() {
        for (int i = 0; i < edgeNum; i++)
        {
            float minAngle1 = 180;
            float minAngle2 = 180;
            int idx1 = edges[i].idx1;
            int idx2 = edges[i].idx2;
            foreach (int idxTo in verticesvertices[idx1])
            {
                if (idxTo == idx2) continue;
                Vector3 vecTo = vertices[idxTo] - vertices[idx1];
                Vector3 vecFrom = vertices[idx2] - vertices[idx1];
                minAngle1 = Mathf.Min(Vector3.Angle(vecFrom, vecTo), minAngle1);
            }
            foreach (int idxTo in verticesvertices[idx2])
            {
                if (idxTo == idx1) continue;
                Vector3 vecTo = vertices[idxTo] - vertices[idx2];
                Vector3 vecFrom = vertices[idx1] - vertices[idx2];
                minAngle2 = Mathf.Min(Vector3.Angle(vecFrom, vecTo), minAngle2);
            }
            edges[i].minAngle1 = minAngle1;
            edges[i].minAngle2 = minAngle2;
            edges[i].fixDis1 = Algorithm.angleFix(i, 0, tuberadii);
            edges[i].fixDis2 = Algorithm.angleFix(i, 1, tuberadii);
        }
    }

    public static void basicPut()
    {
        for (int i = 0; i < verticeNum; i++)
        {
            Vector3 vertice = vertices[i];
            GameObject go = GameObject.Instantiate(Resources.Load("Sphere"), vertice, Quaternion.identity) as GameObject;
            go.transform.localScale = new Vector3(tuberadii*2, tuberadii * 2, tuberadii * 2);
            go.transform.parent = GameObject.Find("Collect").transform;
            verticeGOs[i]=go;
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
            Quaternion fromto = Quaternion.LookRotation(norm, vec);
            //Quaternion fromto2 = Quaternion.FromToRotation(new Vector3(0, 1, 0), vec);
            //Quaternion fromto3 = Quaternion.LookRotation(vec, norm);

            GameObject go = GameObject.Instantiate(Resources.Load("Cylinder"), cent, fromto) as GameObject;
            go.transform.localScale = new Vector3(tuberadii * 2, (v1 - v2).magnitude / 2, tuberadii * 2);
            go.transform.parent = GameObject.Find("Collect").transform;
            edgeGOs[i] = go;
        }
    }
}
