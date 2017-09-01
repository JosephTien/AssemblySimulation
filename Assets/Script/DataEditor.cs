using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataEditor : MonoBehaviour {

    int startIdx, secondIdx, endIdx;
    GameObject startGo, secondGo, endGo;
    int oldpickvert = -1, pickvert = -1, pickedge = -1;
    GameObject pickvertGo;
    GameObject pickedgeGo;
    public List<TS> GTSs;
    List<TS> LTSs;
    HashSet<int> selectverts;

    // Use this for initialization
    void Start() {

    }
    public void setDefault() {
        try
        {
            //GameObject.Find("Canvas/InputField_Input").GetComponent<UnityEngine.UI.InputField>().text = "1031*1000";
            GameObject.Find("Canvas/InputField_Input").GetComponent<UnityEngine.UI.InputField>().text = "2";
            GameObject.Find("Canvas/Panel_Lin/InputField_Thre").GetComponent<UnityEngine.UI.InputField>().text = "1";
        }
        catch { }
    }


    // Update is called once per frame
    void Update() {
        GameObject selected = Tool.mouseSelect();
        if (selected && selected.name.Split('_')[0] == "Vert")
        {
            int idx = int.Parse(selected.name.Split('_')[1]);
            //print("select" + idx);
            if (Input.GetKey(KeyCode.Q))
            {
                if (startGo) Tool.setColor(startGo, Color.white);
                startGo = selected;
                startIdx = idx;
                Tool.setColor(selected, Color.red);

            }
            if (Input.GetKey(KeyCode.W))
            {
                if (secondGo) Tool.setColor(secondGo, Color.white);
                secondGo = selected;
                secondIdx = idx;
                Tool.setColor(selected, Color.green);
            }
            if (Input.GetKey(KeyCode.E))
            {
                if (endGo) Tool.setColor(endGo, Color.white);
                endGo = selected;
                endIdx = idx;
                Tool.setColor(selected, Color.blue);
            }
            if (Input.GetKey(KeyCode.R))
            {
                if (pickvertGo) Tool.setColor(pickvertGo, Color.white);
                if (pickedgeGo) Tool.setColor(pickedgeGo, Color.white);
                int newpickvert = idx;
                if (pickvert != -1 && newpickvert != pickvert) {
                    pickedge = ThinStructure.neighborMap[pickvert][newpickvert];
                    if (pickedge != -1) {
                        pickedgeGo = ThinStructure.edgeGOs[pickedge];
                        Tool.setColor(pickedgeGo, Color.green);
                    }
                    pickvertGo = ThinStructure.verticeGOs[pickvert];
                }
                oldpickvert = pickvert;
                pickvert = newpickvert;
                Tool.setColor(selected, Color.green);
            }

            if (Input.GetKey(KeyCode.A))
            {
                Transform collect = GameObject.Find("Collect").transform;
                foreach (GameObject go in ThinStructure.verticeGOs)
                {
                    //go.SetActive(false);
                }
                //print(ThinStructure.vertgroup[idx].Count);
                foreach (int g in ThinStructure.vertgroup[idx])
                {
                    for (int i = 0; i < ThinStructure.edgeNum; i++)
                    {
                        if (ThinStructure.edges[i].group == g)
                        {
                            int i1 = ThinStructure.edges[i].idx1;
                            int i2 = ThinStructure.edges[i].idx2;
                            //ThinStructure.verticeGOs[i1].SetActive(true);
                            //ThinStructure.verticeGOs[i2].SetActive(true);
                            Tool.setColor(ThinStructure.verticeGOs[i1], Color.yellow);
                            Tool.setColor(ThinStructure.verticeGOs[i2], Color.yellow);
                        }
                    }
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                GameObject[] selecteds = Tool.mouseSelects();
                print("select " + selecteds.Length);
                foreach (GameObject go in selecteds) {
                    int vi = int.Parse(go.name.Split('_')[1]);
                    selectverts.Add(vi);
                    Tool.setColor(go, Color.red);
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Z)) {
            Transform collect = GameObject.Find("Collect").transform;
            foreach (GameObject go in ThinStructure.verticeGOs)
            {
                go.SetActive(true);
                Tool.setColor(go, Color.white);
            }
            ischecked = false;
            curGroup = -1;
            selectverts = new HashSet<int>();
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            foreach (GameObject go in ThinStructure.edgeGOs)
            {
                int ei = int.Parse(go.name.Split('_')[1]);
                if (ThinStructure.edges[ei].group == curGroup)
                {
                    Tool.setColor(go, Color.red);
                }
                else {
                    Tool.setColor(go, Color.white);
                }
            }

            curGroup = (curGroup + 1) % ThinStructure.groupnum;
        }
    }
    
    public void addEdge() {
        ThinStructure.addEdge(oldpickvert, pickvert);//必然為新Group
        putEdited();
    }

    public void delEdge()
    {
        ThinStructure.delEdge(oldpickvert, pickvert);
        bool[] vertUsed = new bool[ThinStructure.verticeNum];
        foreach (Edge e in ThinStructure.edges)
        {
            vertUsed[e.idx1] = true;
            vertUsed[e.idx2] = true;
        }
        purnVertices(vertUsed, ThinStructure.vertices, ThinStructure.edges);
        addEndPointToImpo();
        putEdited();
    }

    public class vertSet {//small and big
        public int vi1;
        public int vi2;
        public vertSet(int vi1, int vi2) {
            if (vi1 > vi2) {
                int t = vi1;
                vi1 = vi2;
                vi2 = t;
            }
            this.vi1 = vi1;
            this.vi2 = vi2;
        }
    }

    public void merge() {
        int[] selectvertsarr = new int[selectverts.Count];
        int cnt = 0;
        Vector3 avr = Vector3.zero;
        foreach (int idx in selectverts) {
            selectvertsarr[cnt++] = idx;
            avr += ThinStructure.vertices[idx];
        }
        avr /= cnt;
        vertSets = new List<vertSet>();
        for (int i = 0; i < selectvertsarr.Length; i++) {
            for (int j = i + 1; j < selectvertsarr.Length; j++) {
                vertSets.Add(new vertSet(selectvertsarr[i], selectvertsarr[j]));
            }
            ThinStructure.vertices[selectvertsarr[i]] = avr;
        }
        Tool.resetColor();
        selectverts = new HashSet<int>();
        mergeCross();
        mergeCrossEdge();
    }

    public void addEndPointToImpo() {
        for (int i = 0; i < ThinStructure.verticeNum; i++) {
            if (ThinStructure.verticesvertices[i].Count == 1) {
                ThinStructure.importantVert.Add(i);
            }
        }
    }

    public void addJuncPointToInpo() {
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            if (ThinStructure.verticesvertices[i].Count > 2)
            {
                ThinStructure.importantVert.Add(i);
            }
        }
    }
    public void addSharpPointToImpo()
    {
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            if (ThinStructure.verticesedges[i].Count == 2)
            {
                int e1 = -1, e2 = -1;
                foreach (int e in ThinStructure.verticesedges[i]) {
                    if (e1 == -1) e1 = e;
                    else e2 = e;
                }
                Vector3 vec1 = ThinStructure.edges[e1].vec;
                Vector3 vec2 = ThinStructure.edges[e2].vec;
                if (ThinStructure.edges[e1].idx1 != i) vec1 *= -1;
                if (ThinStructure.edges[e2].idx1 != i) vec2 *= -1;
                if (Vector3.Angle(vec1, vec2) < 120) {
                    ThinStructure.importantVert.Add(i);
                }
            }
        }
    }

    bool ischecked = false;
    public List<vertSet> vertSets;
    public void checkCross() {
        if (ischecked) {
            mergeCross();
            mergeCrossEdge();
            return;
        }
        mergeCrossEdge();
        vertSets = new List<vertSet>();
        float thre = float.Parse(GameObject.Find("Canvas/Panel_Lin/InputField_Thre/Text").GetComponent<UnityEngine.UI.Text>().text);
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            for (int j = i + 1; j < ThinStructure.verticeNum; j++)
            {
                Vector3 vi1 = ThinStructure.vertices[i];
                Vector3 vi2 = ThinStructure.vertices[j];
                if ((vi1 - vi2).magnitude < thre) {
                    vertSets.Add(new vertSet(i, j));
                }
            }
        }
        foreach (vertSet vs in vertSets) {
            Tool.setColor(ThinStructure.verticeGOs[vs.vi1], Color.yellow);
            Tool.setColor(ThinStructure.verticeGOs[vs.vi2], Color.yellow);
        }
        ischecked = true;
    }

     public void mergeCross() {
        int[] change = new int[ThinStructure.verticeNum];
        for (int i = 0; i < change.Length; i++) change[i] = i;
        foreach (vertSet vs in vertSets)
        {
            Tool.setColor(ThinStructure.verticeGOs[vs.vi1], Color.white);
            Tool.setColor(ThinStructure.verticeGOs[vs.vi2], Color.white);
            change[vs.vi2] = vs.vi1;
        }
        for (int i = 0; i < change.Length; i++) {
            while (change[i] != change[change[i]]) {
                change[i] = change[change[i]];
            }
        }
        int[] newIndex = new int[change.Length];
        List<Vector3> newVertices = new List<Vector3>();
        HashSet<int> newimpo = new HashSet<int>();
        foreach (int impo in ThinStructure.importantVert) {//maybe empty
            newimpo.Add(newIndex[change[impo]]);
        }
        int cnt = 0;
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            if (change[i] == i)
            {
                newVertices.Add(ThinStructure.vertices[i]);
                newIndex[i] = cnt++;
            }
            else {
                newimpo.Add(newIndex[change[i]]);
            }
        }
        ThinStructure.importantVert = newimpo;
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            ThinStructure.edges[i].idx1 = newIndex[change[ThinStructure.edges[i].idx1]];
            ThinStructure.edges[i].idx2 = newIndex[change[ThinStructure.edges[i].idx2]];
        }
        ThinStructure.reGenDetail(newVertices.ToArray(), ThinStructure.edges);
        ThinStructure.genVertGroup();
        addEndPointToImpo();//統一化?
        putEdited();
        ischecked = false;
        GTSs = genSecInfo();
    }

    bool equal(Vector3 a, Vector3 b)
    {
        return (a - b).magnitude < 0.001f;
    }

    public void mergeCrossEdge()
    {

        List<Edge> newEdges = new List<Edge>();
        Vector3[] vert = ThinStructure.vertices;
        bool[] edgeToDelete = new bool[ThinStructure.edgeNum];
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            for (int j = i + 1; j < ThinStructure.edgeNum; j++)
            {
                Edge ei = ThinStructure.edges[i];
                Edge ej = ThinStructure.edges[j];
                if ((equal(vert[ei.idx1], vert[ej.idx1]) && equal(vert[ei.idx2], vert[ej.idx2]))
                || (equal(vert[ei.idx1], vert[ej.idx2]) && equal(vert[ei.idx2], vert[ej.idx1])))
                {
                    edgeToDelete[j] = true;
                }
            }
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            Edge ei = ThinStructure.edges[i];
            if (equal(vert[ei.idx1], vert[ei.idx2])){
                edgeToDelete[i] = true;
            }
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            if (!edgeToDelete[i]) {
                newEdges.Add(ThinStructure.edges[i]);
            }
        }

        bool[] vertUsed = new bool[ThinStructure.verticeNum];
        foreach (Edge e in newEdges) {
            vertUsed[e.idx1] = true;
            vertUsed[e.idx2] = true;
        }
        purnVertices(vertUsed, ThinStructure.vertices, newEdges.ToArray());
    }

    List<GameObject> drawed = new List<GameObject>();
    int curGroup = -1;
    public void aimNextGroup() {

        curGroup = (curGroup + 1) % ThinStructure.groupnum;
        foreach (GameObject go in drawed) Tool.setColor(go, Color.white);
        drawed = new List<GameObject>();
        LTSs = new List<TS>();
        foreach (TS ts in GTSs) {
            if (ThinStructure.vertgroup[ts.from].Contains(curGroup) && ThinStructure.vertgroup[ts.end].Contains(curGroup))
            {
                int from = startIdx = ts.from;
                int to = secondIdx = ts.to;
                int end = endIdx = ts.end;
                LTSs.Add(ts);
                drawed.Add(ThinStructure.verticeGOs[from]);
                drawed.Add(ThinStructure.verticeGOs[end]);
                Tool.setColor(ThinStructure.verticeGOs[from], Color.green);
                Tool.setColor(ThinStructure.verticeGOs[end], Color.green);
                while (to != end)
                {
                    drawed.Add(ThinStructure.verticeGOs[to]);
                    Tool.setColor(ThinStructure.verticeGOs[to], Color.green);
                    int next = getNextVert(from, to);
                    from = to;
                    to = next;
                }
            }
        }
    }

    private int getNextImpo(int from, int to)
    {
        while (!ThinStructure.importantVert.Contains(to)) {
            int next = getNextVert(from, to);
            from = to;
            to = next;
        }
        return to;
    }

    private int getNextImpo(int from, int to, ref int last)
    {
        while (!ThinStructure.importantVert.Contains(to))
        {
            int next = getNextVert(from, to);
            from = to;
            to = next;
        }
        last = from;
        return to;
    }

    public int getNextVert(int from, int to) {
        foreach (int v in ThinStructure.verticesvertices[to])
        {
            if (v == from) continue;
            return v;
        }
        return to;
    }
    public List<int> getSecVerts(TS ts) {
        List<int> secVerts = new List<int>();
        secVerts.Add(ts.from);
        secVerts.Add(ts.to);
        int from = ts.from;
        int to = ts.to;
        int end= ts.end;
        while (to!=end) {
            int next = getNextVert(from, to);
            from = to;
            to = next;
            secVerts.Add(to);
        }
        return secVerts;
    }
    public class TS
    {
        public TS(int from, int to, int end)
        {
            this.from = from;
            this.to = to;
            this.end = end;
        }
        public int from;
        public int to;
        public int end;
    };
    
    public List<TS> genSecInfo() {
        List<TS> TSs = new List<TS>();
        Queue<int> queue = new Queue<int>();
        HashSet<int> impo = new HashSet<int>(); foreach (int v in ThinStructure.importantVert) impo.Add(v);
        bool[][] visited = new bool[ThinStructure.verticeNum][];
        for (int i = 0; i < ThinStructure.verticeNum; i++) visited[i] = new bool[ThinStructure.verticeNum];
        while (impo.Count > 0)
        {
            int root = -1;
            foreach (int v in impo) { root = v; break; }
            queue.Enqueue(root);
            while (true)
            {
                bool isEmpty = true; foreach (int v in queue) { isEmpty = false; break; }
                if (isEmpty) break;
                int pa = queue.Dequeue();
                impo.Remove(pa);
                foreach (int ch in ThinStructure.verticesvertices[pa])
                {
                    int last = -1;
                    int end = getNextImpo(pa, ch,ref last);
                    if (!visited[pa][end])
                    {
                        queue.Enqueue(end);
                        TSs.Add(new TS(pa, ch, end));
                        visited[pa][end] = true;
                        visited[end][pa] = true;
                        //visited[pa][ch] = true;
                        //visited[ch][pa] = true;
                    }
                }
            }
        }
        return TSs;
    }

    public void autoSimplify() {
        foreach (TS ts in GTSs) {
            Color newColor = new Color(Random.Range(0, 0.99f), Random.Range(0, 0.99f), Random.Range(0, 0.99f));
            int from=ts.from;
            int to=ts.to;
            int end=ts.end;
            while (to != end) {
                Tool.setColor(ThinStructure.verticeGOs[to], newColor);
                int next = getNextVert(from, to);
                from = to;
                to = next;
            }
        }
        simplify(GTSs);
    }
    public void simplify()
    {
        if (LTSs.Count > 0)
        {
            simplify(LTSs);
            curGroup--;
            LTSs = new List<TS>();
        }
        else{
            List<TS> TSs = new List<TS>();
            TSs.Add(new TS(startIdx, secondIdx, endIdx));
            simplify(TSs);
        }
    }

    public void simplify(List<TS> TSs) {
        bool[] isEdgeDelete = new bool[ThinStructure.edgeNum];
        bool[] isVertDelete = new bool[ThinStructure.verticeNum];
        List<Edge> addEdges = new List<Edge>();
        foreach (TS ts in TSs)
        {
            int from = ts.from;
            int to = ts.to;
            int end = ts.end;
            if (to == end) continue;
            while (from != end)
            {
                int next = getNextVert(from, to);
                int nextnext = getNextVert(to, next);
                isEdgeDelete[ThinStructure.neighborMap[from][to]] = true;
                isEdgeDelete[ThinStructure.neighborMap[to][next]] = true;
                isVertDelete[to] = true;
                Edge e = new Edge(from, next);
                if (nextnext == end)
                {
                    isEdgeDelete[ThinStructure.neighborMap[next][nextnext]] = true;
                    isVertDelete[next] = true;
                    e = new Edge(from, nextnext);
                    e.group = ThinStructure.edges[ThinStructure.neighborMap[from][to]].group;
                    addEdges.Add(e);
                    break;
                }
                e.group = ThinStructure.edges[ThinStructure.neighborMap[from][to]].group;
                addEdges.Add(e);
                from = next;
                to = nextnext;
            }
        }
        /////////////////////////////////////////
        List<Edge> newEdges = new List<Edge>();
        for (int i = 0; i < ThinStructure.edgeNum; i++) {
            if (!isEdgeDelete[i])
            {
                newEdges.Add(ThinStructure.edges[i]);
            }
        }
        foreach (Edge e in addEdges) newEdges.Add(e);
        /////////////////////////////////////////
        purnVertices_(isVertDelete, ThinStructure.vertices, newEdges.ToArray());
    }
    public void purnVertices_(bool[] isUsed, Vector3[] vertices, Edge[] edges)
    {
        for (int i = 0; i < isUsed.Length; i++) {
            isUsed[i] = !isUsed[i];
        }
        purnVertices(isUsed, vertices, edges);
    }

    public void purnVertices(bool[] isUsed, Vector3[] vertices, Edge[] edges) {
        List<Vector3> newVert = new List<Vector3>();
        int[] newIndex = new int[vertices.Length];
        for (int i = 0, cnt = 0; i < vertices.Length; i++)
        {
            if (!isUsed[i])
            {
                newIndex[i] = -1;
            }
            else
            {
                newVert.Add(vertices[i]);
                newIndex[i] = cnt++;
            }
        }
        for (int i = 0; i < edges.Length; i++)
        {
            edges[i].idx1 = newIndex[edges[i].idx1];
            edges[i].idx2 = newIndex[edges[i].idx2];
        }
        /***************************************/
        ThinStructure.reGenDetail(newVert.ToArray(), edges);
        ThinStructure.genVertGroup();
        putEdited();
        /***************************************/
        /*
        int[] newImpo = new int[ThinStructure.importantVert.Count];
        int it = 0; foreach (int v in ThinStructure.importantVert) { newImpo[it++] = v; };
        for (int i = 0; i < newImpo.Length; i++)
        {
            newImpo[i] = newIndex[newImpo[i]];
        }
        ThinStructure.importantVert = new HashSet<int>(); foreach (int v in newImpo) { ThinStructure.importantVert.Add(v); }
        */
        HashSet<int> newImpo = new HashSet<int>();
        foreach (int v in ThinStructure.importantVert) {
            if (newIndex[v] != -1) {
                newImpo.Add(newIndex[v]);
            }
        }
        ThinStructure.importantVert = newImpo;

        GTSs = genSecInfo();
    }

    public void init() {
        GTSs = new List<TS>();
        LTSs = new List<TS>();
        selectverts = new HashSet<int>();
        ThinStructure.importantVert = new HashSet<int>();
        curGroup = -1;
    }

    public void putEdited() {
        Tool.clearObj();
        float tuberadii = ThinStructure.tuberadii;
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            Vector3 v = ThinStructure.vertices[i];
            /**********************************************************/
            GameObject gov = GameObject.Instantiate(Resources.Load("Sphere"), v, Quaternion.identity) as GameObject;
            gov.transform.localScale = new Vector3(tuberadii * 2, tuberadii * 2, tuberadii * 2);
            gov.transform.parent = GameObject.Find("Collect").transform;
            gov.name = "Vert_" + i;
            ThinStructure.verticeGOs[i] = gov;
        }
        Transform collect = GameObject.Find("Collect").transform;
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            int i1 = ThinStructure.edges[i].idx1;
            int i2 = ThinStructure.edges[i].idx2;
            Vector3 v1 = ThinStructure.vertices[i1];
            Vector3 v2 = ThinStructure.vertices[i2];
            /**********************************************************/
            ThinStructure.edgeGOs[i] = Tool.DrawLine(v1, v2, 8, Color.white);
            ThinStructure.edgeGOs[i].GetComponent<CapsuleCollider>().enabled = false;
            ThinStructure.edgeGOs[i].name = "Edge_" + i;
            ThinStructure.edgeGOs[i].transform.parent = collect;
            ThinStructure.edgeGOs[i].SetActive(false);
        }
        shoEdge();
        drawed.Clear();
    }

    public void ReadFromImpo()
    {
        string inText = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
        string[] inTexts = inText.Split('*');
        int tarSet = int.Parse(inTexts[0]);
        ThinStructure.myscale = 1;
        if (inTexts.Length > 1) ThinStructure.myscale = float.Parse(inTexts[1]);
        ThinStructure.readFromImpoSet(tarSet);
        putEdited();
        init();
    }

    public void EditLin() {
        string inText = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
        string[] inTexts = inText.Split('*');
        int tarSet = int.Parse(inTexts[0]);
        ThinStructure.myscale = 1;
        if (inTexts.Length > 1) ThinStructure.myscale = float.Parse(inTexts[1]);
        ThinStructure.readLin(tarSet);
        putEdited();
        init();
    }

    public void reload()
    {
        string inText = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
        string[] inTexts = inText.Split('*');
        int tarSet = int.Parse(inTexts[0]);
        ThinStructure.myscale = 1;
        if (inTexts.Length > 1) ThinStructure.myscale = float.Parse(inTexts[1]);
        ThinStructure.basicRead(tarSet);
        putEdited();
        init();
        addEndPointToImpo();
        addJuncPointToInpo();
        addSharpPointToImpo();  
    }


    public void hideNode() {
        bool shownode = GameObject.Find("Canvas/Panel_Lin/Toggle_HideNode").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (shownode)
        {
            foreach (GameObject go in ThinStructure.verticeGOs)
            {
                int ei = int.Parse(go.name.Split('_')[1]);
                go.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject go in ThinStructure.verticeGOs)
            {
                go.SetActive(true);
            }
        }
    }

    public void shoEdge()
    {
        bool showedge = GameObject.Find("Canvas/Panel_Lin/Toggle_ShowEdge").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (showedge)
        {
            foreach (GameObject go in ThinStructure.edgeGOs)
            {
                int ei = int.Parse(go.name.Split('_')[1]);
                int vi1 = ThinStructure.edges[ei].idx1;
                int vi2 = ThinStructure.edges[ei].idx2;
                if (ThinStructure.verticeGOs[vi1].activeSelf && ThinStructure.verticeGOs[vi2])
                {
                    go.SetActive(true);
                    Tool.setColor(go, Color.white);
                }
            }
        }
        else{
            foreach (GameObject go in ThinStructure.edgeGOs)
            {
                go.SetActive(false);
                Tool.setColor(go, Color.white);
            }
        }
    }

    public void shoImpo()
    {
        bool showimpo = GameObject.Find("Canvas/Panel_Lin/Toggle_ShowImpo").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (showimpo)
        {
            foreach (int vi in ThinStructure.importantVert)
            {
                Tool.setColor(ThinStructure.verticeGOs[vi], Color.red);
            }
        }
        else
        {
            foreach (GameObject go in ThinStructure.verticeGOs)
            {
                Tool.setColor(go, Color.white);
            }
        }
    }

    public void shoJunc() {
        bool showjunc = GameObject.Find("Canvas/Panel_Lin/Toggle_ShowJunc").GetComponent<UnityEngine.UI.Toggle>().isOn;
        Transform collect = GameObject.Find("Collect").transform;
        if (showjunc) {
            int cc = collect.childCount;
            for (int i=0;i< cc;i++)
            {
                GameObject go = collect.GetChild(i).gameObject;
                string[] sp = go.name.Split('_');
                if (sp[0] == "Vert"){
                    int idx = int.Parse(sp[1]);
                    if (ThinStructure.verticesedges[idx].Count > 2)
                    {
                        go.GetComponent<MeshRenderer>().material.color = Color.blue;
                    }
                }
            }
        }
        else
        {
            int cc = collect.childCount;
            for (int i = 0; i < cc; i++)
            {
                GameObject go = collect.GetChild(i).gameObject;
                go.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }
    public void outputThin() {
        string inText = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
        string[] inTexts = inText.Split('*');
        int tarSet = int.Parse(inTexts[0]);
        ThinStructure.outputThin(tarSet);
    }

}
//crossvert need purn
//crossedge need purn
//simplify need purn