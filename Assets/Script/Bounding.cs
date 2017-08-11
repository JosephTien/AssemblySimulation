using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
public class HoleInfo {
    public HoleInfo(int idx, Vector3 dir) {
        this.idx = idx;
        this.dir = dir;
    }
    public int idx;
    public Vector3 dir;
}
public class Bounding : MonoBehaviour {
    public static BoundInfo[] boundInfo;
    public static bool valid = false;
    public static bool debug = false;
    int tarSet;
    /**/
    int curEdge;
    int compTar;
    public static bool groupMode = false;
    float anglePlus = 10;
    float checkerdis_lim = BoundInfo.checkerdis_lim;
    int selectedGroup = -1;
    List<int> selectedEdge = new List<int>();
    List<int> selectedVert = new List<int>();
    List<HashSet<int>> mergeComp = new List<HashSet<int>>();
    List<HashSet<int>> mergeComp_vert = new List<HashSet<int>>();
    public static int[] compIdx;
    public static int[] compIdx_vert;
    DataEditor dataEditor;
    public static GroupInfo[] groupInfo;
    public static List<HoleInfo> holeInfos = new List<HoleInfo>();
    GameObject holeTube;
    /**/
    void addSelectVert(int vert) {
        selectedVert.Remove(vert);
        selectedVert.Add(vert);
    }
    void removeSelectVert(int vert)
    {
        selectedVert.Remove(vert);
    }
    void addSelectEdge(int edge)
    {
        selectedEdge.Remove(edge);
        selectedEdge.Add(edge);
    }
    void removeSelectEdge(int edge)
    {
        selectedEdge.Remove(edge);
    }

    void putWithTunning()
    {
        groupMode = false;
        Vector3 fix = new Vector3(0.001f, 0.001f, 0.001f);
        //float arrowradii = 3;
        float tuberadii = ThinStructure.tuberadii;
        boundInfo = new BoundInfo[ThinStructure.edgeNum+ThinStructure.verticeNum];
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            Edge edge = ThinStructure.edges[i];
            Vector3 v1 = ThinStructure.vertices[edge.idx1];
            Vector3 v2 = ThinStructure.vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized;
            float len = (v2 - v1).magnitude;
            float f1 = ThinStructure.edges[i].fixDis1;
            float f2 = ThinStructure.edges[i].fixDis2;
            if (f1 + f2 > len) {
                f1 = f2 = len / 2;
                continue;
            }
            v1 += vec * f1;
            v2 -= vec * f2;
            Vector3 cent = (v2 + v1) / 2;
            Vector3 norm = ThinStructure.splitNorms[i].normalized;
            if (norm == Vector3.zero) norm = ThinStructure.splitNorms[i] = new Vector3(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            norm = Vector3.Cross(Vector3.Cross(vec, norm), vec);
            Quaternion fromto = Quaternion.LookRotation(norm, vec);
            GameObject go = GameObject.Instantiate(Resources.Load("Cylinder"), cent, fromto) as GameObject;
            go.transform.localScale = new Vector3(tuberadii * 2, (v1 - v2).magnitude / 2, tuberadii * 2);
            go.transform.parent = GameObject.Find("Collect").transform;
            go.name = "Edge_" + i;
            /**********************************************************/
            go.AddComponent<BoundInfo>();
            boundInfo[i] = go.GetComponent<BoundInfo>();
            boundInfo[i].initNorm = norm;
            boundInfo[i].axis = vec;
            boundInfo[i].len = 20;
            boundInfo[i].idx = i;
            boundInfo[i].minAngle1 = ThinStructure.edges[i].minAngle1;
            boundInfo[i].minAngle2 = ThinStructure.edges[i].minAngle2;
            boundInfo[i].fixDis1 = ThinStructure.edges[i].fixDis1;
            boundInfo[i].fixDis2 = ThinStructure.edges[i].fixDis2;
            /**********************************************************/
            GameObject plane = GameObject.Instantiate(Resources.Load("Plane2"), cent, fromto) as GameObject;
            plane.transform.parent = GameObject.Find("Assist").transform;
            plane.transform.localScale = new Vector3(tuberadii * 2, (v1 - v2).magnitude / 2, tuberadii * 2);
            plane.name = "AssistPlane";
            boundInfo[i].assistPlane = plane;
            /**********************************************************/
            /*
            Quaternion fromto2 = Quaternion.LookRotation(vec, norm);
            GameObject arrow = GameObject.Instantiate(Resources.Load("Arrow"), cent, fromto2) as GameObject;
            arrow.transform.localScale = new Vector3(arrowradii, boundInfo[i].len * 2, arrowradii);
            arrow.transform.parent = bi.transform;
            arrow.name = "arrow";
            ////////////////////////////////////////////////////////////
            GameObject arrowhead1 = GameObject.Instantiate(Resources.Load("ArrowHead"), cent + norm * boundInfo[i].len, fromto2) as GameObject;
            arrowhead1.transform.localScale = new Vector3(10, 10, 10);
            arrowhead1.transform.parent = bi.transform;
            arrowhead1.name = "arrowhead1";
            arrowhead1.GetComponentInChildren<MeshFilter>().sharedMesh.RecalculateNormals();
            ////////////////////////////////////////////////////////////
            Quaternion fromto3 = Quaternion.LookRotation(vec, -norm);
            GameObject arrowhead2 = GameObject.Instantiate(Resources.Load("ArrowHead"), cent - norm * boundInfo[i].len, fromto3) as GameObject;
            arrowhead2.transform.localScale = new Vector3(10, 10, 10);
            arrowhead2.transform.parent = bi.transform;
            arrowhead2.name = "arrowhead2";
            arrowhead2.GetComponentInChildren<MeshFilter>().sharedMesh.RecalculateNormals();
            ////////////////////////////////////////////////////////////
            */
        }
        int en = ThinStructure.edgeNum;
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            Vector3 v = ThinStructure.vertices[i];
            /**********************************************************/
            GameObject gov = GameObject.Instantiate(Resources.Load("Sphere"), v, Quaternion.identity) as GameObject;
            gov.transform.localScale = new Vector3(tuberadii * 2, tuberadii * 2, tuberadii * 2);
            gov.transform.parent = GameObject.Find("Collect").transform;
            gov.name = "Vert_" + i;
            /**********************************************************/
            gov.AddComponent<BoundInfo>();
            int j = i + en;
            boundInfo[j] = gov.GetComponent<BoundInfo>();
            boundInfo[j].idx = i;
            boundInfo[j].isvert = true;
            /**********************************************************/
            foreach (int edge in ThinStructure.verticesedges[i])
            {
                Vector3 vec = ThinStructure.edges[edge].vec;
                float dis;
                if (ThinStructure.edges[edge].idx1 == i)
                {
                    dis = ThinStructure.edges[edge].fixDis1;
                }
                else
                {
                    vec *= -1;
                    dis = ThinStructure.edges[edge].fixDis2;
                }
                Tool.DrawLine(v, v + vec * dis, tuberadii, Color.white).transform.parent = gov.transform;
            }
        }
        //StartCoroutine(IcalAllBound());
    }

    public void collectToGroup() {
        HashSet<int>[] edgeGroups = mergeComp.ToArray();
        HashSet<int>[] vertGroups = mergeComp_vert.ToArray();
        GameObject Collect = GameObject.Find("Collect");
        int cnt;
        for (cnt = 0; cnt < edgeGroups.Length; cnt++) {
            GameObject go = GameObject.Instantiate(Resources.Load("Group") as GameObject);
            go.name = "Group_" + cnt;
            go.transform.parent = Collect.transform;
            HashSet<int> edgeGroup = edgeGroups[cnt];
            HashSet<int> vertGroup = vertGroups[cnt];
            int en = ThinStructure.edgeNum;
            foreach (int e in edgeGroup) {
                if(boundInfo[e]) boundInfo[e].gameObject.transform.parent = go.transform;
            }
            foreach (int v in vertGroup) 
            {
                if(boundInfo[v+en]) boundInfo[v + en].gameObject.transform.parent = go.transform;
            }
        }
        groupInfo = GameObject.Find("Collect").GetComponentsInChildren<GroupInfo>();
        for (int i = 0; i < groupInfo.Length; i++) {
            groupInfo[i].edge = edgeGroups[i];
            groupInfo[i].vert = vertGroups[i];
        }
        /**************************************/
        List<GroupInfo> newGroupInfo = new List<GroupInfo>(groupInfo);
        foreach (BoundInfo bi in boundInfo) {
            if (bi == null) continue;
            if (bi.transform.parent == Collect.transform) {
                GameObject go = GameObject.Instantiate(Resources.Load("Group") as GameObject);
                go.name = "Group_" + cnt;
                go.transform.parent = Collect.transform;
                bi.transform.parent = go.transform;
                HashSet<int> edgeGroup = new HashSet<int>();
                HashSet<int> vertGroup = new HashSet<int>();
                if (!bi.isvert)
                {
                    edgeGroup.Add(bi.idx);
                    compIdx[bi.idx] = cnt;
                }
                else
                {
                    vertGroup.Add(bi.idx);
                    compIdx_vert[bi.idx] = cnt;
                }
                GroupInfo gi = go.GetComponent<GroupInfo>();
                gi.edge = edgeGroup;
                gi.vert = vertGroup;
                mergeComp.Add(edgeGroup);
                mergeComp_vert.Add(vertGroup);
                newGroupInfo.Add(gi);
                cnt++;
            }
        }
        groupInfo = newGroupInfo.ToArray();
    }
    public void disbandAllGroup() {

    }

    public void autoMerge() {
        mergeComp = new List<HashSet<int>>();
        mergeComp_vert = new List<HashSet<int>>();
        int groupcnt = 0;
        foreach (DataEditor.TS ts in dataEditor.GTSs)
        {
            HashSet<int> compVerts = new HashSet<int>();
            HashSet<int> compEdges = new HashSet<int>();
            List<int> secVerts = dataEditor.getSecVerts(ts);
            int from = -1;
            int to = -1;
            foreach (int v in secVerts)
            {
                if (from == -1)
                {
                    from = v;
                    continue;
                }
                if (to == -1)
                {
                    to = v;
                    compEdges.Add(ThinStructure.neighborMap[from][to]);
                    continue;
                }
                compEdges.Add(ThinStructure.neighborMap[to][v]);
                compVerts.Add(to);
                from = to;
                to = v;
            }
            //foreach (int i in compEdges) print(ThinStructure.edges[i].idx1+" "+ ThinStructure.edges[i].idx2);
            if (compEdges.Count >= 1) {
                foreach (int e in compEdges) {
                    compIdx[e] = groupcnt;
                }
                foreach (int v in compVerts)
                {
                    compIdx_vert[v] = groupcnt;
                }
                mergeComp.Add(compEdges);
                mergeComp_vert.Add(compVerts);
                groupcnt++;
            }
            /******************************************/
            if (compEdges.Count >= 1)
            {
                List<Vector3> secPos = new List<Vector3>();
                foreach (int i in secVerts) { secPos.Add(ThinStructure.vertices[i]); }
                Vector3 norm = Algorithm.calFittingPlane(secPos);
                if (compEdges.Count == 1) {
                    foreach (int e in compEdges) {
                        Edge edge = ThinStructure.edges[e];
                        norm = ThinStructure.splitNorms[ThinStructure.neighborMap[edge.idx1][edge.idx2]];
                    }
                }
                
                Vector3 prev = Vector3.zero;
                bool isrot = Random.Range(0,0.9f)>0.5f?false:true;
                foreach (int e in compEdges)
                {
                    if (prev != Vector3.zero) {
                        if (Vector3.Dot(prev, ThinStructure.edges[e].vec) < 0) ThinStructure.edges[e].swap();
                    }
                    prev = ThinStructure.edges[e].vec;
                    if (boundInfo[e])
                    {
                        Vector3 vec = ThinStructure.edges[e].vec;
                        boundInfo[e].initNorm = Vector3.Cross(vec, Vector3.Cross(norm, vec));
                        boundInfo[e].axis = vec;
                        boundInfo[e].rotateTo(boundInfo[e].initNorm);
                        //if (isrot) boundInfo[e].rotateTo(90);
                        boundInfo[e].rotateAssisPlane();
                    }
                }
            }
        }
    }

    public void sectionGroupMode() {
        if (groupMode) return;
        groupMode = true;
        autoMerge();
        collectToGroup();
        foreach (GroupInfo gi in groupInfo)
        {
            gi.init();
        }
    }
    public void calAllBound() {
        StartCoroutine(IcalAllBound());
    }

    IEnumerator IcalAllBound()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            if (!boundInfo[i]) continue;
            if (debug) print("cal edge" + i + "'s bound");
            boundInfo[i].checkAllBound();
            //boundInfo[i].findBestBound();
            if (compIdx[i] == -1)
                boundInfo[i].solved = 2;
            else
                boundInfo[i].solved = boundInfo[i].isSoled();
            ThinStructure.splitNorms[i] = boundInfo[i].curNorm;
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = (i + 1) + "/" + ThinStructure.edgeNum;
            yield return new WaitForSeconds(0.01f);
        }
        for (int i = ThinStructure.edgeNum; i < boundInfo.Length; i++)
        {
            if (!boundInfo[i]) continue;
            boundInfo[i].solved = boundInfo[i].isSoled();
        }
        GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
        /***************************************************************************/
        if (groupMode) {
            foreach (GroupInfo gi in groupInfo)
            {
                gi.calBoundingLen();
            }
        }
    }

    public void assignSplitInfo() {

        for (int i = 0;i < ThinStructure.edgeNum; i++){
            ThinStructure.splitNorms[i] = boundInfo[i].curNorm;
        }
        foreach (GroupInfo gi in groupInfo) {
            foreach (int e in gi.edge)
            {
                //ThinStructure.angleArgs[e] = new AngleArg(gi.curAngle2, gi.curAngle3, gi.curAngle2_2, gi.curAngle3_2);

                //float angle2, angle3, angle2_2, angle3_2;
                //boundInfo[e].vec2anglearg(boundInfo[e].curNorm, gi.curDir, out angle2, out angle3);
                //boundInfo[e].vec2anglearg(boundInfo[e].curNorm, -gi.curDir2, out angle2_2, out angle3_2);//注意使用方式，需翻轉到norm所在平面，或翻轉norm
                //ThinStructure.angleArgs[e] = new AngleArg(angle2, angle3, angle2_2, angle3_2);
                ThinStructure.angleArgs[e].dir1 = gi.curDir;
                ThinStructure.angleArgs[e].dir2 = gi.curDir2;
            }
        }
    }

    public void writeGroupInfo() {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < ThinStructure.verticeNum; i++) {
            GroupInfo gi = groupInfo[compIdx_vert[i]];
            if (gi.isChild && gi.nodeidx!=-1)
            {
                int g = int.Parse(gi.transform.parent.name.Split('_')[1]);
                sb.Append(string.Format("{0} {1}\n", g * 2, g * 2 + 1));
            }
            else {
                sb.Append(string.Format("-1 -1\n"));
            }
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            GroupInfo gi = groupInfo[compIdx[i]];
            if (gi.isChild)
            {
                Vector3 mainNorm = gi.transform.parent.GetComponent<GroupInfo>().mainNorm;
                int g = int.Parse(gi.transform.parent.name.Split('_')[1]);
                BoundInfo bi = boundInfo[i];
                if (Vector3.Dot(mainNorm, bi.curNorm) >= 0)
                    sb.Append(string.Format("{0} {1}\n", g * 2, g * 2 + 1));
                else
                    sb.Append(string.Format("{0} {1}\n", g * 2 + 1, g * 2));            
            }
            else
            {
                sb.Append(string.Format("-1 -1\n"));
            }
        }
        string filename = "inputSet\\" + ThinStructure.curSet + "\\input\\groupinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }

    public void writeholeinfo(){
        StringBuilder sb = new StringBuilder();
        
        sb.Append(holeInfos.Count + "\n");
        foreach(HoleInfo hi in holeInfos)
        {
            sb.Append(string.Format("{0} {1} {2} {3}\n",hi.idx, hi.dir.x, hi.dir.y, hi.dir.z));
        }
        string filename = "inputSet\\" + ThinStructure.curSet + "\\input\\holeinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }
    public static void readholeinfo()
    {
        string filename = "inputSet\\" + ThinStructure.curSet + "\\input\\holeinfo.txt";
        System.IO.StreamReader file = new System.IO.StreamReader(filename);
        string line;
        string[] items;
        int cnt;
        line = file.ReadLine();
        cnt = int.Parse(line);
        holeInfos = new List<HoleInfo>();
        while (!file.EndOfStream)
        {
            line = file.ReadLine();
            items = line.Split(' ');
            if (items.Length <= 1) { continue; }
            holeInfos.Add(new HoleInfo(int.Parse(items[0]), new Vector3(float.Parse(items[1]), float.Parse(items[2]), float.Parse(items[3]))));
        }
        file.Close();
    }

    public void writeInfo()
    {
        ThinStructure.outputsplitNorms(ThinStructure.curSet);
        ThinStructure.linkInfo = new HashSet<int>[ThinStructure.verticeNum];
        for (int i = 0; i < ThinStructure.verticeNum; i++) { ThinStructure.linkInfo[i] = new HashSet<int>(); }

        foreach (HashSet<int> hs_vert in mergeComp_vert)
        {
            HashSet<int> hs = mergeComp[0];
            foreach (int i_vert in hs_vert)
            {
                foreach (int i in hs)
                {
                    if (ThinStructure.verticesedges[i_vert].Contains(i))
                    {
                        ThinStructure.linkInfo[i_vert].Add(i);
                    }
                }
            }
            mergeComp.RemoveAt(0);
        }
        mergeComp.Clear();
        mergeComp_vert.Clear();
        ThinStructure.fixLinkinfo();
        ThinStructure.outputlinkInfo(ThinStructure.curSet);
        /*************************************************/
        //write solvedinfo
        StringBuilder sb = new StringBuilder();
        for (int i = ThinStructure.edgeNum; i < boundInfo.Length; i++)
        {
            sb.Append(boundInfo[i].solved + "\n");
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            sb.Append(boundInfo[i].solved + "\n");
        }
        string filename = "inputSet\\" + ThinStructure.curSet + "\\input\\solvedinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
        /*************************************************/
        collectToGroup();
        writeGroupInfo();
        writeholeinfo();
    }

    public void writeInfoFromGroup() {
        assignSplitInfo();
        ThinStructure.outputsplitNorms(ThinStructure.curSet);
        ThinStructure.linkInfo = new HashSet<int>[ThinStructure.verticeNum];
        for (int i = 0; i < ThinStructure.verticeNum; i++) { ThinStructure.linkInfo[i] = new HashSet<int>(); }
        foreach (GroupInfo gi in groupInfo)
        {
            foreach (int i_vert in gi.vert)
            {
                foreach (int i in gi.edge)
                {
                    if (ThinStructure.verticesedges[i_vert].Contains(i))
                    {
                        ThinStructure.linkInfo[i_vert].Add(i);
                    }
                }
            }
        }
        ThinStructure.fixLinkinfo();
        ThinStructure.outputlinkInfo(ThinStructure.curSet);
        /*************************************************/
        //write solvedinfo
        StringBuilder sb = new StringBuilder();
        for (int i = ThinStructure.edgeNum; i < boundInfo.Length; i++)
        {
            sb.Append(boundInfo[i].solved +"\n");
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            sb.Append(boundInfo[i].solved + "\n");
        }
        string filename = "inputSet\\" + ThinStructure.curSet + "\\input\\solvedinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
        /*************************************************/
        writeGroupInfo();
        writeholeinfo();
    }

    // Use this for initialization
    void Start () {
        
    }
    public void start() {
        GameObject f = GameObject.Find("Assist");
        for (int i = f.transform.childCount - 1; i >= 0; i--) {
            Destroy(f.transform.GetChild(i).gameObject);
        }
        GameObject collect = GameObject.Find("Collect");
        for (int i = 0; i < collect.transform.childCount; i++)
        {
            GameObject go = collect.transform.GetChild(i).gameObject;
            Destroy(go);
        }
        /************************************************************/
        try
        {
            main();
        }
        catch
        {
            print("bad number");
            return;
        }
        putWithTunning();
        init();
    }

    void init() {
        dataEditor = GameObject.Find("DataManager").GetComponent<DataEditor>();
        ThinStructure.importantVert = new HashSet<int>();
        dataEditor.addEndPointToImpo();
        dataEditor.addJuncPointToInpo();
        dataEditor.addSharpPointToImpo();
        dataEditor.GTSs = dataEditor.genSecInfo();
    }
    void fixDir()
    {
        foreach (DataEditor.TS ts in dataEditor.GTSs)
        {
            HashSet<int> compEdges = new HashSet<int>();
            List<int> secVerts = dataEditor.getSecVerts(ts);
            int from = -1;
            int to = -1;
            foreach (int v in secVerts)
            {
                if (from == -1)
                {
                    from = v;
                    continue;
                }
                if (to == -1)
                {
                    to = v;
                    compEdges.Add(ThinStructure.neighborMap[from][to]);
                    continue;
                }
                compEdges.Add(ThinStructure.neighborMap[to][v]);
                from = to;
                to = v;
            }
            if (compEdges.Count > 1)
            {
                List<Vector3> secPos = new List<Vector3>();
                foreach (int i in secVerts) { secPos.Add(ThinStructure.vertices[i]); }
                Vector3 prev = Vector3.zero;
                foreach (int e in compEdges)
                {
                    if (prev != Vector3.zero)
                    {
                        if (Vector3.Dot(prev, ThinStructure.edges[e].vec) < 0) ThinStructure.edges[e].swap();
                    }
                    prev = ThinStructure.edges[e].vec;
                }
            }
        }
    }
    void main() {
        string inText = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
        string[] inTexts = inText.Split('*');
        tarSet = int.Parse(inTexts[0]);
        ThinStructure.myscale = 1;
        if (inTexts.Length > 1) ThinStructure.myscale = float.Parse(inTexts[1]);
        
        ThinStructure.basicRead(tarSet);
        compIdx = new int[ThinStructure.edgeNum];
        compIdx_vert = new int[ThinStructure.verticeNum];
        selectedEdge = new List<int>();
        selectedVert = new List<int>();
        mergeComp = new List<HashSet<int>>();
        mergeComp_vert = new List<HashSet<int>>();
        for (int i = 0; i < compIdx.Length; i++) compIdx[i] = -1;
        for (int i = 0; i < compIdx_vert.Length; i++) compIdx_vert[i] = -1;
        //putWithTunning();
    }
    public void merge() {
        if (selectedEdge.Count == 0) return;
        int id = mergeComp.Count;
        HashSet <int> newComp = new HashSet<int>();
        foreach (int e in selectedEdge)
        {
            newComp.Add(e);
        }
        mergeComp.Add(newComp);
        /******************************/
        int id_vert = mergeComp_vert.Count;
        HashSet<int> newComp_vert = new HashSet<int>();
        foreach (int v in selectedVert)
        {
            newComp_vert.Add(v);
        }
        mergeComp_vert.Add(newComp_vert);
        /******************************/
        HashSet<int> toDelete = new HashSet<int>();
        int en = ThinStructure.edgeNum;
        foreach (int e in selectedEdge) {
            if (compIdx[e] >= 0) toDelete.Add(compIdx[e]);
            compIdx[e] = id;
            boundInfo[e].setActive(false);
        }
        selectedEdge.Clear();
        /******************************/
        HashSet<int> toDelete_vert = new HashSet<int>();
        foreach (int v in selectedVert)
        {
            if (compIdx_vert[v] >= 0) toDelete_vert.Add(compIdx_vert[v]);
            compIdx_vert[v] = id_vert;
            boundInfo[v+en].setActive(false);
        }
        selectedVert.Clear();
        /******************************/
        while (toDelete.Count > 0) {
            int max = int.MinValue;
            foreach (int c in toDelete) {
                max = max > c ? max : c;
            }
            toDelete.Remove(max);
            mergeComp.RemoveAt(max);
            for (int i = 0; i < compIdx.Length; i++) {
                if (compIdx[i] == max) compIdx[i] = -1;
                if (compIdx[i] >  max) compIdx[i]--;
            }
        }
        while (toDelete_vert.Count > 0)
        {
            int max = int.MinValue;
            foreach (int c in toDelete_vert)
            {
                max = max > c ? max : c;
            }
            toDelete_vert.Remove(max);
            mergeComp_vert.RemoveAt(max);
            for (int i = 0; i < compIdx_vert.Length; i++)
            {
                if (compIdx_vert[i] == max) compIdx_vert[i] = -1;
                if (compIdx_vert[i] >  max) compIdx_vert[i]--;
            }
        }
    }
    public void grow() {//單向，以兩個為選取來觸發方向，須注意成長方向的norm//bfs
        //print(selectedEdge.Count);
        if (selectedEdge.Count != 2) return;
        float thre = 0.99f;
        int en = ThinStructure.edgeNum;
        bool[] visited = new bool[ThinStructure.edgeNum];
        for (int i = 0; i < visited.Length; i++) visited[i] = false;
        Queue<int> queue = new Queue<int>();
        Vector3[] edgeCommonNorm = new Vector3[ThinStructure.edgeNum];
        int[] edgeFrom = new int[ThinStructure.edgeNum];
        for (int i = 0; i < edgeFrom.Length; i++) edgeFrom[i] = -1;
        Queue<int> queue_vert = new Queue<int>();
        /**********************/
        int root1=-1, root2=-1;
        foreach (int i in selectedEdge) {
            if (i == curEdge)
            {
                root2 = i;
                queue.Enqueue(i);
                visited[i] = true;
            }
            else {
                root1 = i;
                visited[i] = true;
            }
        }
        edgeFrom[root2] = root1;
        /**********************/
        int rootvert = ThinStructure.edgeConnMap[root1][root2];
        boundInfo[rootvert + en].setActive(true);
        addSelectVert(rootvert);
        /**********************/
        if (Mathf.Abs(Vector3.Dot(ThinStructure.edges[root1].vec, ThinStructure.edges[root2].vec)) < thre) {
            Vector3 rootnorm = Vector3.Cross(ThinStructure.edges[root1].vec, ThinStructure.edges[root2].vec).normalized;
            edgeCommonNorm[root1] = edgeCommonNorm[root2] = rootnorm;
        }
        /**********************/
        while (true) {
            bool isEmpty = true;foreach (int e in queue) { isEmpty = false; break; }
            if (isEmpty) break;
            int idx = queue.Dequeue();
            visited[idx] = true;
            Edge edge = ThinStructure.edges[idx];
            boundInfo[idx].setActive(true);
            addSelectEdge(idx);
            /****************/
            bool isEmpty_vert = true; foreach (int e in queue_vert) { isEmpty_vert = false; break; };
            if (!isEmpty_vert)
            {
                int idx_vert = queue_vert.Dequeue();
                boundInfo[idx_vert + en].setActive(true);
                addSelectVert(idx_vert);
                //selectedVert.Add(idx_vert);
            }
            /****************/
            int iv1 = edge.idx1;
            if (ThinStructure.verticesedges[iv1].Count == 2) {
                int taredge = idx;//need initial
                foreach (int temp in ThinStructure.verticesedges[iv1]) if (temp != idx) taredge = temp;
                if (!visited[taredge])
                {
                    Vector3 newnorm = Vector3.Cross(ThinStructure.edges[idx].vec, ThinStructure.edges[taredge].vec).normalized;
                    bool ahead = Mathf.Abs(Vector3.Dot(ThinStructure.edges[idx].vec, ThinStructure.edges[taredge].vec)) > thre;
                    if (!ahead) {
                        int tempcur = idx;
                        while (tempcur >= 0 && edgeCommonNorm[tempcur] == Vector3.zero)
                        {
                            edgeCommonNorm[tempcur] = newnorm;
                            tempcur = edgeFrom[tempcur];
                        }
                    }
                    if (ahead || Mathf.Abs(Vector3.Dot(edgeCommonNorm[idx], newnorm)) > thre) {
                        queue.Enqueue(taredge);
                        queue_vert.Enqueue(iv1);
                        edgeCommonNorm[taredge] = newnorm;
                        edgeFrom[taredge] = idx;
                    }
                }
            }
            int iv2 = edge.idx2;
            if (ThinStructure.verticesedges[iv2].Count == 2)
            {
                int taredge = idx;//need initial
                foreach (int temp in ThinStructure.verticesedges[iv2]) if (temp != idx) taredge = temp;
                if (!visited[taredge])
                {
                    Vector3 newnorm = Vector3.Cross(ThinStructure.edges[idx].vec, ThinStructure.edges[taredge].vec).normalized;
                    bool ahead = Mathf.Abs(Vector3.Dot(ThinStructure.edges[idx].vec, ThinStructure.edges[taredge].vec)) > thre;
                    if (!ahead)
                    {
                        int tempcur = idx;
                        while (tempcur >= 0 && edgeCommonNorm[tempcur] == Vector3.zero)
                        {
                            edgeCommonNorm[tempcur] = newnorm;
                            tempcur = edgeFrom[tempcur];
                        }
                    }
                    if (ahead || Mathf.Abs(Vector3.Dot(edgeCommonNorm[idx], newnorm)) > thre)
                    {
                        queue.Enqueue(taredge);
                        queue_vert.Enqueue(iv2);
                        edgeCommonNorm[taredge] = newnorm;
                        edgeFrom[taredge] = idx;
                    }
                }
            }
        }
        foreach (int idx in selectedEdge) {
            boundInfo[idx].rotateTo(edgeCommonNorm[idx]);
        }
    }

    public void showgroup() {
        bool showgroup = GameObject.Find("Canvas/Panel_Edit/Toggle_ShowGroup").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (showgroup) {
            Color[] colors = new Color[mergeComp.Count];
            int en = ThinStructure.edgeNum;
            int i = 0;
            for (i = 0; i < colors.Length; i++) colors[i] = new Color(Random.Range(0, 0.9f), Random.Range(0, 0.9f), Random.Range(0, 0.9f));
            i = 0;
            foreach (HashSet<int> comp in mergeComp) {
                foreach (int idx in comp) {
                    boundInfo[idx].showgroup = true;
                    boundInfo[idx].groupcolor = colors[i];
                }
                i++;
            }
            i = 0;
            foreach (HashSet<int> comp in mergeComp_vert)
            {
                foreach (int idx in comp)
                {
                    boundInfo[idx + en].showgroup = true;
                    boundInfo[idx + en].groupcolor = colors[i];
                }
                i++;
            }
        }
        else{
            for(int i=0;i<boundInfo.Length;i++)
            {
                if (boundInfo[i]) {
                    boundInfo[i].showgroup = false;
                    boundInfo[i].groupcolor = Color.white;
                }
            }
        }

    }

    public void showstate()
    {
        bool showstate = GameObject.Find("Canvas/Panel_Edit/Toggle_ShowSol").GetComponent<UnityEngine.UI.Toggle>().isOn;
        for (int i = 0; i < boundInfo.Length; i++)
        {
            if (!boundInfo[i]) continue;
            boundInfo[i].showstate = showstate;
        }
    }

    public void showstate_vert()
    {
        for (int i = ThinStructure.edgeNum; i < boundInfo.Length; i++)
        {
            if (!boundInfo[i]) continue;
            boundInfo[i].showstate = !boundInfo[i].showstate;
        }
    }
    
    public void showplane()
    {
        bool planed = GameObject.Find("Canvas/Panel_Edit/Toggle_ShowPlane").GetComponent<UnityEngine.UI.Toggle>().isOn;
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            if (!boundInfo[i]) continue;
            boundInfo[i].showPlane(planed);
        }
    }

    public void showCol()
    {
        GroupInfo.shoCol = GameObject.Find("Canvas/Panel_Edit/Toggle_ShowCol").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (!GroupInfo.shoCol)
        {
            GroupInfo.showCol(false);
        }
        else {
            if (selectedGroup != -1)
            {
                groupInfo[selectedGroup].calBoundingLen();
                float bl = groupInfo[selectedGroup].curBoundingLen;
                if (bl == checkerdis_lim)
                    GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Inf";
                else
                    GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "" + bl;
            }
            else {
                GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "";
            }
            
        }
        
    }

    void onSelect(BoundInfo bi)
    {
        if (!bi.isvert)
        {
            int en = ThinStructure.edgeNum;
            curEdge = bi.idx;
            if (selectedEdge.Count > 0)
            {

                bool validSelect = false;
                foreach (int i in selectedEdge)
                {
                    if (i != curEdge && ThinStructure.edgeConnMap[i][curEdge] >= 0)
                    {
                        validSelect = true;
                        int connvert = ThinStructure.edgeConnMap[i][curEdge];//assume only one conn
                        onSelect(boundInfo[connvert + en]);
                        break;
                    }
                }
                if (!validSelect) return;
            }
            if (compIdx[curEdge] == -1)
            {
                bi.setActive(true);
                addSelectEdge(curEdge);
            }
            else
            {
                int comp = compIdx[curEdge];
                HashSet<int> sel = mergeComp[comp];
                foreach (int i in sel)
                {
                    boundInfo[i].setActive(true);
                    addSelectEdge(i);
                }
                sel = mergeComp_vert[comp];
                foreach (int i in sel)
                {
                    boundInfo[i + en].setActive(true);
                    addSelectVert(i);
                }
                bi.setActive(true);
            }
        }
        else if (bi.isvert)
        {
            int en = ThinStructure.edgeNum;
            int curVert = bi.idx;
            if (selectedEdge.Count > 0)
            {

                bool validSelect = false;
                foreach (int i in selectedEdge)
                {
                    if (ThinStructure.verticesedges[curVert].Contains(i))
                    {
                        validSelect = true;
                        break;
                    }
                }
                if (!validSelect) return;
            }
            if (compIdx_vert[curVert] == -1)
            {
                bi.setActive(true);
                addSelectVert(curVert);
            }
            else
            {
                int comp = compIdx_vert[curVert];
                HashSet<int> sel = mergeComp[comp];
                foreach (int i in sel)
                {
                    boundInfo[i].setActive(true);
                    addSelectEdge(i);
                }
                sel = mergeComp_vert[comp];
                foreach (int i in sel)
                {
                    boundInfo[i + en].setActive(true);
                    addSelectVert(i);
                }
                bi.setActive(true);
            }
        }
    }

    void Update() {
        if (Input.GetKey(KeyCode.Space))
        {
            valid = true;
            Ctrl.valid = false;
        }
        else {
            valid = false;
            Ctrl.valid = true;
        }
        if (Input.GetMouseButtonDown(0) && valid) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                Transform Collect = GameObject.Find("Collect").transform;
                BoundInfo bi = hit.collider.gameObject.GetComponent<BoundInfo>();
                if (!groupMode)
                {
                    onSelect(bi);
                }
                else {
                    int idx;
                    if(!bi.isvert) idx = compIdx[bi.idx];
                    else idx = compIdx_vert[bi.idx];
                    if (selectedGroup != -1) {
                        foreach (GroupInfo gi in groupInfo) gi.setActive(false);
                        //groupInfo[selectedGroup].setActive(false);
                    }
                    if (selectedGroup != idx)
                    {
                        if (groupInfo[idx].isChild) {
                            groupInfo[idx].transform.parent.GetComponent<GroupInfo>().setChildActive(true);
                        }
                        groupInfo[idx].setActive(true);
                        selectedGroup = idx;
                    }
                    else {
                        selectedGroup = -1;
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && valid)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            RaycastHit[] hits = Physics.RaycastAll(ray, 10000);
            //foreach (RaycastHit h in hits) print(h.collider.name);
            if (Physics.Raycast(ray, out hit, 10000))
            {
                Transform Collect = GameObject.Find("Collect").transform;
                BoundInfo bi = hit.collider.gameObject.GetComponentInChildren<BoundInfo>();
                bi.setActive(false);
                if (!bi.isvert)
                {
                    removeSelectEdge(bi.idx);
                }
                else if (bi.isvert)
                {
                    removeSelectVert(bi.idx);
                }
                
            }
        }
        if (groupMode && valid)
        {
            float addAngle = 0;
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                addAngle = anglePlus;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                addAngle = -anglePlus;
            }
            if (addAngle != 0) {
                if (selectedGroup == -1)
                {
                    foreach (GroupInfo gi in groupInfo)
                    {
                        gi.addRotateTo(addAngle);
                    }
                }
                else
                {
                    groupInfo[selectedGroup].addRotateTo(addAngle);
                    float bl = groupInfo[selectedGroup].curBoundingLen;
                    if (bl == checkerdis_lim)
                        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Inf";
                    else
                        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "" + bl;
                }
            }
        }
        /***********************************************************************************/
        GroupInfo targi=null;if (selectedGroup != -1) targi = groupInfo[selectedGroup];
        if (groupMode && Input.GetKeyUp(KeyCode.U) && selectedGroup != -1)
            targi.tuneRotate(false, targi.curAngle2 + 5, targi.curAngle3);
        if (groupMode && Input.GetKeyUp(KeyCode.I) && selectedGroup != -1)
            targi.tuneRotate(false, targi.curAngle2 - 5, targi.curAngle3);
        if (groupMode && Input.GetKeyUp(KeyCode.J) && selectedGroup != -1)
            targi.tuneRotate(true, targi.curAngle2_2 + 5, targi.curAngle3_2);
        if (groupMode && Input.GetKeyUp(KeyCode.K) && selectedGroup != -1)
            targi.tuneRotate(true, targi.curAngle2_2 - 5, targi.curAngle3_2);
        if (groupMode && Input.GetKeyUp(KeyCode.Y) && selectedGroup != -1)
            targi.tuneRotate(false, targi.curAngle2, targi.curAngle3 + 5);
        if (groupMode && Input.GetKeyUp(KeyCode.H) && selectedGroup != -1)
            targi.tuneRotate(false, targi.curAngle2, targi.curAngle3 - 5);
        if (groupMode && Input.GetKeyUp(KeyCode.O) && selectedGroup != -1)
            targi.tuneRotate(true, targi.curAngle2_2, targi.curAngle3_2 + 5);
        if (groupMode && Input.GetKeyUp(KeyCode.L) && selectedGroup != -1)
            targi.tuneRotate(true, targi.curAngle2_2, targi.curAngle3_2 - 5);
        if (groupMode && Input.GetKeyUp(KeyCode.M) && selectedGroup != -1) {
            targi.mergeNeighborCurve();
        }
        if (Input.GetKeyUp(KeyCode.N))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                BoundInfo bi = hit.collider.gameObject.GetComponentInChildren<BoundInfo>();
                if (bi.isvert) {
                    bool isin = false;
                    HoleInfo hi = new HoleInfo(bi.idx, Tool.randomVector());
                    foreach (HoleInfo hii in holeInfos) {
                        if (hii.idx == bi.idx) {
                            isin = true;
                            hii.dir = hi.dir;
                            break;
                        }
                    }
                    if (!isin) {
                        holeInfos.Add(hi);
                    }
                    if (holeTube) Destroy(holeTube);
                    Vector3 pos = ThinStructure.vertices[bi.idx];
                    holeTube = Tool.DrawLine(pos, pos + hi.dir * 100, 5, Color.yellow);
                    holeTube.transform.parent = GameObject.Find("Assist").transform;
                    print(holeInfos.Count);
                }
            }
        }

        /***********************************************************************************/
        if (selectedGroup != -1) {
            showArg(true, true);
        }
        else {
            showArg(false, false);
        }
        /*
        if (Input.GetMouseButtonDown(0) || Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000))
            {
                valid = true;
                Ctrl.valid = false;
                Transform Collect = GameObject.Find("Collect").transform;
                for (int i = Collect.transform.childCount - 1; i >= 0; i--)
                {
                    Collect.GetChild(i).GetComponentInChildren<BoundInfo>().setActive(false);
                }
                hit.collider.transform.parent.gameObject.GetComponentInChildren<BoundInfo>().setActive(true);
            }
            else {
                valid = false;
                Ctrl.valid = true;
            }
        }
        */
    }
    public void showArg(bool arg, bool maxarg) {
        UnityEngine.UI.Text TX = GameObject.Find("Canvas/Text2").GetComponent<UnityEngine.UI.Text>();
        TX.text = "";
        if (arg)
        {
            TX.text += "π = " + groupInfo[selectedGroup].curAngle + "\nθ = " + groupInfo[selectedGroup].curAngle2 + "\nτ = " + groupInfo[selectedGroup].curAngle3;
        }
        else TX.text += "\n\n";
        if (maxarg)
        {
            TX.text += "\nMax θ1 = " + Mathf.Round(groupInfo[selectedGroup].maxAngle2) + "; Max τ1 = " + Mathf.Round(groupInfo[selectedGroup].maxAngle3);
            TX.text += "\nMax θ2 = " + Mathf.Round(groupInfo[selectedGroup].maxAngle2_2) + "; Max τ2 = " + Mathf.Round(groupInfo[selectedGroup].maxAngle3_2);
        }
        else TX.text += "\n\n";
    }
}
