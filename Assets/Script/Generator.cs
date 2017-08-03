using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Threading;

public class MeshComponent{
    string name;
    GameObject instance;
    public bool locked = false;
    public bool filed = false;
    public bool inpool = false;//in csg pool
    public HashSet<int> vertices = new HashSet<int>();
    public HashSet<int> edges = new HashSet<int>();
    public Vector3 splitNorm = Vector3.zero;
    public AngleArg angleArg;
    public Vector3 splitDir1 =Vector3.zero;
    public Vector3 splitDir2 = Vector3.zero;
    public int ordering;
    public static string poolPath = @"CSGCommandLineTool\pool\";
    public string Name { get { return name; } }
    public GameObject Instance { get { return instance; } }
    public MeshComponent(string name){
        GameObject obj = GameObject.Instantiate(Resources.Load("Empty"), Vector3.zero, Quaternion.identity) as GameObject;
        Mesh holderMesh = new Mesh();
        ObjImporter newMesh = new ObjImporter();
        holderMesh = newMesh.ImportFile(poolPath+name+".obj");
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        holderMesh.RecalculateNormals();
        filter.mesh = holderMesh;
        obj.transform.parent = GameObject.Find("Collect").transform;
        obj.name = this.name = name;
        instance = obj;
        filed = true;
    }
    public MeshComponent(string name, GameObject instance)
    {
        this.name = name;
        this.instance = instance;
        this.instance.name = name;
        renewFile();//filed = true;
    }

    public bool isSingleEdge() {
        return edges.Count == 1 && vertices.Count == 0;
    }

    public void addRef(HashSet<int> set1, HashSet<int> set2)
    {
        foreach(int i in set1){
            vertices.Add(i);
        }
        foreach (int i in set2)
        {
            edges.Add(i);
        }
    }

    public MeshComponent copy(string name) {
        if (this.name == name) return null;
        GameObject obj = GameObject.Instantiate(this.instance, Vector3.zero, Quaternion.identity) as GameObject;
        MeshComponent meshComponent = new MeshComponent(name, obj);
        obj.transform.parent = GameObject.Find("Collect").transform;
        obj.name = name;
        obj.SetActive(true);
        filed = false;
        return meshComponent;
    }
    public void renewFile()
    {
        if (filed) return;
        string filename = poolPath + name+".obj";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString());
        }
        filed = true;
        inpool = false;
    }
    public void reloadFile()
    {
        Transform parent = instance.transform.parent;
        GameObject.Destroy(instance);
        instance = new MeshComponent(name).Instance;
        instance.transform.parent = parent;
        filed = true;
    }
    public void transform(Vector3 scale, Vector3 translate, Vector3 up, Vector3 forw) {
        instance.transform.localScale = scale;
        instance.transform.position = translate;
        /*
        MonoBehaviour.print("up");
        MonoBehaviour.print(up);
        MonoBehaviour.print("forw");
        MonoBehaviour.print(forw);
        */
        instance.transform.rotation = Quaternion.LookRotation(up, forw);
        if (up == Vector3.zero) instance.transform.rotation = Quaternion.identity;
        filed = false;
    }
    public static Vector3 mul(Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public void AppendMesh(GameObject appendee) {
        Mesh mesher = instance.GetComponent<MeshFilter>().mesh;
        Mesh meshee = appendee.GetComponent<MeshFilter>().mesh;
        int meshernum = mesher.vertexCount;
        int mesheenum = meshee.vertexCount;
        Vector3[] vertices = new Vector3[meshernum + mesheenum];
        Vector3[] normals = new Vector3[meshernum + mesheenum];
        for (int i = 0; i < meshernum; i++)
        {
            Transform t = instance.transform;
            vertices[i] = t.rotation * mul(t.lossyScale, mesher.vertices[i]) + t.position;
            normals[i] = t.rotation * mesher.normals[i];
            //vertices[i] = mesher.vertices[i];
            //normals[i] = mesher.normals[i];
        }
        for (int i = 0; i < mesheenum; i++)
        {
            Transform t = appendee.transform;
            vertices[i + meshernum] = t.rotation * mul(t.lossyScale, meshee.vertices[i]) + t.position;
            normals[i + meshernum] = t.rotation * meshee.normals[i];
            //vertices[i + meshernum] = meshee.vertices[i];
            //normals[i + meshernum] = meshee.normals[i];
        }
        int[] triangles = new int[mesher.triangles.Length + meshee.triangles.Length];
        int tribasenum = mesher.triangles.Length;
        for (int i = 0; i < mesher.triangles.Length; i++)
        {
            triangles[i] = mesher.triangles[i];
        }
        for (int i = 0; i < meshee.triangles.Length; i++)
        {
            triangles[tribasenum + i] = meshee.triangles[i] + meshernum;
        }
        mesher.vertices = vertices;
        mesher.normals = normals;
        mesher.triangles = triangles;
        mesher.RecalculateBounds();
        instance.transform.position = Vector3.zero;
        instance.transform.localScale = Vector3.one;
        instance.transform.localRotation = Quaternion.identity;
        filed = false;
    }

    string MeshToString() {
        Mesh mesh = instance.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = new Vector3[mesh.vertexCount];
        Vector3[] normals = new Vector3[mesh.vertexCount];
        for (int i = 0; i < mesh.vertexCount; i++) {
            Transform t = instance.transform;
            vertices[i] = t.rotation * mul(t.lossyScale,mesh.vertices[i]) + t.position;
            normals[i] = t.rotation * mesh.normals[i];
        }
        StringBuilder sb = new StringBuilder();
        foreach (Vector3 lv in vertices)
        {
            //sb.Append(string.Format("v {0} {1} {2}\n", -lv.x, lv.y, lv.z));
            sb.Append(string.Format("v {0} {1} {2}\n", lv.x, lv.y, lv.z));
        }
        sb.Append("\n");
        
        foreach (Vector3 lv in normals)
        {
            //sb.Append(string.Format("vn {0} {1} {2}\n", -lv.x, lv.y, lv.z));
            sb.Append(string.Format("vn {0} {1} {2}\n", lv.x, lv.y, lv.z));
        }
        sb.Append("\n");
        
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            //sb.Append(string.Format("f {2}//{2} {1}//{1} {0}//{0}\n",
            //    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            sb.Append(string.Format("f {0}//{0} {1}//{1} {2}//{2}\n",
                triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
        }
        return sb.ToString();
    }
    public static MeshComponent operator +(MeshComponent mc1, MeshComponent mc2) {
        Transform tr2 = mc2.instance.transform;
        while (tr2.parent != GameObject.Find("Collect").transform) {
            tr2 = tr2.parent;
        }
        Transform tr1 = mc1.instance.transform;
        while (tr1.parent != GameObject.Find("Collect").transform)
        {
            tr1 = tr1.parent;
        }
        Vector3 oriLoosyScale = tr2.lossyScale;
        tr2.parent = tr1;
        return mc1;
    }
}

public class CSGQueue {
    public class CSGCMDSet {
        public CSGCMDSet(string command, string name1, string name2, string name3)
        {
            this.command = command;
            this.name1 = name1;
            this.name2 = name2;
            this.name3 = name3;
            skip = false;
        }
        public CSGCMDSet(string command, string name1, string name2, string name3, bool skip)
        {
            this.command = command;
            this.name1 = name1;
            this.name2 = name2;
            this.name3 = name3;
            this.skip = skip;
        }
        public string command;
        public string name1;
        public string name2;
        public string name3;
        public bool skip;
    }
    public static bool skipLoad = false;
    public static int totalCommand = 0;
    public static Queue<CSGCMDSet> queue = new Queue<CSGCMDSet>();
    public CSGQueue(){
    }

    public static void addCSGSet(string ch, string er, string ee1, string ee2) {
        queue.Enqueue(new CSGCMDSet(ch, er, ee1, ee2));
        totalCommand++;
    }
    
    public static void addCSGSet_skip(string ch, string er, string ee1, string ee2)
    {
        queue.Enqueue(new CSGCMDSet(ch, er, ee1, ee2, true));
        totalCommand++;
    }

    public static void applyCSG(string ch, int er, int ee1, int ee2, bool skip)//load 和 write可以放到queue?
    {
        if (Executor.debug)MonoBehaviour.print("Unity : op" + ch + " " + Pool.list[er].Name + " " + Pool.list[ee1].Name+ " " + Pool.list[ee2].Name);

        Pool.list[er].renewFile();
        Pool.list[ee1].renewFile();
        Pool.list[ee2].renewFile();
        if (!Pool.list[er].inpool) {
            Executor.csgcommand("LOAD " + Pool.list[er].Name);
            Pool.list[er].inpool = true;
        }
        if (!Pool.list[ee1].inpool)
        {
            Executor.csgcommand("LOAD " + Pool.list[ee1].Name);
            Pool.list[ee1].inpool = true;
        }
        if (!Pool.list[ee2].inpool)
        {
            Executor.csgcommand("LOAD " + Pool.list[ee2].Name);
            Pool.list[ee2].inpool = true;
        }
        Executor.csgcommand(ch + " " + Pool.list[er].Name + " " + Pool.list[ee1].Name + " " + Pool.list[ee2].Name);
        if (!skip) {
            Executor.csgcommand("WRITE " + Pool.list[er].Name);
            Pool.list[er].filed = false;//you should becareful of the skip
            Pool.list[er].locked = true;
            Pool.lockNum++;
        }
    }
    public static void applyCopy(string ch, int er, int ee, bool skip) {
        if (Executor.debug) MonoBehaviour.print("Unity : op " + ch + " " + Pool.list[er].Name + " " + Pool.list[ee].Name);
        Pool.list[ee].renewFile();
        if (!Pool.list[er].inpool)
        {
            Executor.csgcommand("LOAD " + Pool.list[ee].Name);
            Pool.list[er].inpool = true;
        }
        Executor.csgcommand(ch + " " + Pool.list[er].Name + " " + Pool.list[ee].Name);
        if (!skip)
        {
            Executor.csgcommand("WRITE " + Pool.list[er].Name);
            Pool.list[er].locked = true;
            Pool.lockNum++;
            Pool.list[er].filed = false;
        }
    }

    public static bool isEmpty() {
        bool isempty = true;
        foreach (CSGCMDSet x in queue)
        {
            isempty = false;
            break;
        }
        return isempty;
    }

    public static bool executeSingle() {
        if (isEmpty()) return false;
        string command = queue.Peek().command;
        if (command == "COPY" || command == "copy")
        {
            int tar1 = Pool.find(queue.Peek().name1);
            int tar2 = Pool.find(queue.Peek().name2);
            bool skip = queue.Peek().skip;
            if (tar1 == -1 || tar2 == -1) return false;
            if (!Pool.list[tar1].locked && !Pool.list[tar2].locked)
            {
                applyCopy(command, tar1, tar2, skip);
                queue.Dequeue();
                return true;
            }
            else
            {
                return false;
            }
        }
        else {
            int tar1 = Pool.find(queue.Peek().name1);
            int tar2 = Pool.find(queue.Peek().name2);
            int tar3 = Pool.find(queue.Peek().name3);
            bool skip = queue.Peek().skip;
            if (tar1 == -1 || tar2 == -1 || tar3 == -1) return false;
            if (!Pool.list[tar1].locked && !Pool.list[tar2].locked && !Pool.list[tar3].locked)
            {
                applyCSG(command, tar1, tar2, tar3, skip);
                queue.Dequeue();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public static void fetchFromReady()
    {
        string unlocktar = "";
        foreach (string x in Executor.ready)
        {
            unlocktar = x;
            break;
        }
        if (unlocktar != "")
        {
            int idx = Pool.find(unlocktar);
            if(!skipLoad)Pool.list[idx].reloadFile();//filed = true;
            Pool.list[idx].locked = false;
            Pool.lockNum--;
            Executor.ready.Dequeue();
            Generator.showObject(unlocktar, true);
        }
    }
}

public class Pool{
    public static List<MeshComponent> list = new List<MeshComponent>();
    public static List<string> toDelete = new List<string>();
    CSGQueue csgQueue = new CSGQueue();
    public static int lockNum = 0;
    static List<List<int>> originList = new List<List<int>>();
    static List<string> originName = new List<string>();

    public static int find(string name) {
        int i = 0;
        foreach (MeshComponent mc in list) {
            if (mc.Name == name) {
                return i;
            }
            i++;
        }
        return -1;
    }

    public void cleanPool()
    {
        foreach (string name in toDelete)
        {
            Executor.deleteObjFileInPool(name);
        }
        foreach (MeshComponent mc in list)
        {
            GameObject.Destroy(mc.Instance);
        }
        list.Clear();
        toDelete.Clear();
    }
    public static void deleteObj(string name) {
        for (int i = list.Count - 1; i >= 0; i--) { 
            MeshComponent mc = list[i];
            if (mc.Name == name) {
                GameObject.Destroy(mc.Instance);
                toDelete.Add(name);
                list.RemoveAt(i);
                break;
            }
        }
    }
    
    public static MeshComponent addPool(string name)
    {
        /*
        int tar = -1;
        int i = 0;
        int cnt = 0;
        foreach (MeshComponent mc in list)
        {
            if (cnt > 0)
            {
                if (mc.Name == name + "_" + cnt)
                {
                    cnt++;
                }
            }
            else if (mc.Name == name)
            {
                tar = i;
                cnt++;
            }
            i++;
        }
        if (tar == -1)
        {
            list.Add(new MeshComponent(name));
        }
        else
        {
            list.Add(list[tar].copy(name + "_" + cnt));
            toDelete.Add(name + "_" + cnt);
        }
		return list[list.Count - 1];
        */
        /*******************************/
        int tar = -1;
        int i = 0;
        foreach (string str in originName)
        {
            if (str == name)
            {
                tar = i;
                break;
            }
            i++;
        }
        int tail = list.Count;
        if (tar == -1)
        {
            list.Add(new MeshComponent(name));
            originName.Add(name);
            originList.Add(new List<int>());
        }
        else
        {
            int cnt = originList[tar].Count;
            list.Add(list[tar].copy(name + "_" + cnt));
            originList[tar].Add(tail);
            toDelete.Add(name + "_" + cnt);//可不必要
        }
        return list[tail];
    }

}

public class Generator : MonoBehaviour {
    Pool pool = new Pool();
    public static MeshComponent[] Complist = new MeshComponent[5000];//need more
    int curComp = 0;
    int compNum;
    int tarSet = 13;//13or4
    // Use this for initialization
    void Start() {
        //manulOutput("halftube");
    }

    public void manulOutput(string name) {
        MeshComponent mc = new MeshComponent(name, GameObject.Find(name).transform.GetChild(0).gameObject);
    }

    public void reset() {
        pool = new Pool();
        Complist = new MeshComponent[5000];
        Tool.clearObj();
    }

    public void start() {
        reset();
        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "";
        try {
            string inputtext = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
            string[] inputtexts = inputtext.Split('*');
            if (inputtexts.Length > 1) ThinStructure.myscale = int.Parse(inputtexts[1]);
            tarSet = int.Parse(inputtexts[0]);
            addSample();
        } catch
        {
            print("bad number");
            return;
        }
        genTest();
    }
    bool geninside = true;
    bool cave = false;//cave inside or not (save time)
    bool conn = false;//connector inside or not (save time)
    bool simple = false;
    void genTest() {
        /*
         * Comp :
         * inner stucture
         * node Cut
         * node Connnect
         * Assem Comp
         */
        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Preparing...";
        GameObject Collect = GameObject.Find("Collect");
        ThinStructure.basicRead(tarSet);
        genThinComp();
        genAssemComps();
        collectComps();
        int childCount = Collect.transform.childCount;
        int compnum = childCount - 1;
        for (int i = 1; i < childCount; i++)
        {
            CSGMergeComp(i);
        }
        for (int i = 1; i < childCount; i++) {//產生connector，挖出內層
            genConn(i);
            if (conn) {
                //MeshComponent comp = Pool.list[Pool.find("comp_" + i)];
                MeshComponent comp = Pool.list[Pool.find(Complist[i].Name)];
                int verticeCount = comp.vertices.Count;
                int edgeCount = comp.edges.Count;
                if (verticeCount <= 2 && edgeCount == 1)
                {
                    CSGQueue.addCSGSet("-", Complist[i].Name, Complist[i].Name, Complist[i + compnum].Name);
                }
                else if (verticeCount == 1)
                {
                    CSGQueue.addCSGSet("+", Complist[i].Name, Complist[i].Name, Complist[i + compnum].Name);
                }
            }
            if (cave && !Complist[i].isSingleEdge())
            {
                CSGQueue.addCSGSet("-", Complist[i].Name, Complist[i].Name, Complist[0].Name);
            }
        }
        for (int i = 1; i < childCount; i++)//產生knife
        {
            genCut(i);
        }
        for (int i = 1; i < childCount; i++)//copy並使用knife
        {
            if (Complist[i].isSingleEdge())
            {
                Complist[i].renewFile();
                int e = -1;
                foreach (int ee in Complist[i].edges) e = ee;
                copyCompByInst(i, Quaternion.AngleAxis(180, ThinStructure.edges[e].vec));
            }
            else
            {
                copyComp(i);
                CSGQueue.addCSGSet("*", Complist[i + compnum * 3].Name, Complist[i].Name, Complist[i + compnum * 2].Name);
                CSGQueue.addCSGSet("-", Complist[i].Name, Complist[i].Name, Complist[i + compnum * 2].Name);
            }
        }
        int n = GameObject.Find("Collect").transform.childCount;
        for (int i = 0; i < n; i++) {
            GameObject.Find("Collect").transform.GetChild(i).gameObject.SetActive(false);
        }
        compNum = compnum;
        /********************/
        StartCoroutine(outputToSet());
        genOrderingInfo();
    }

    public void btn_outputToSet() {
        if (pushed) return;
        StartCoroutine(outputToSet());
        genOrderingInfo();
    }


    public static void showObject(string name, bool state) {
        GameObject go = Pool.list[Pool.find(name)].Instance;
        go.gameObject.transform.parent.gameObject.SetActive(state);
        go.SetActive(state);
    }
    bool pushed = false;
    IEnumerator outputToSet() {
        pushed = true;
        while (!CSGQueue.isEmpty() || Pool.lockNum > 0)
        {
            GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "CSG Waiting : " + Executor.CSGCommandsCnt + " / " + CSGQueue.totalCommand;
            GameObject.Find("Canvas/Text2").GetComponent<UnityEngine.UI.Text>().text = Executor.curCommand;
            yield return new WaitForSeconds(0.05f);
        }

        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Writing...";
        Executor.clearInputSet(tarSet);
        
        Executor.mkObjDir(tarSet);
        Executor.cpoyObjFileInPoolToInputSet(Complist[0].Name, tarSet, "output_0");
        int cnt=0;
        for (int i = 1; i < compNum + 1; i++)
        {
            //Executor.cpoyObjFileInPoolToInputSet(Complist[i].Name,tarSet, "output_"+i);
            Executor.cpoyObjFileInPoolToInputSet(Complist[i].Name, tarSet, "output_" + (cnt + 1));
            cnt++;
            //Executor.cpoyObjFileInPoolToInputSet(Complist[i+ compNum*3].Name, tarSet, "output_" + (i+ compNum));
            Executor.cpoyObjFileInPoolToInputSet(Complist[i + compNum * 3].Name, tarSet, "output_" + (cnt + 1));
            cnt++;
        }
        Executor.flushBath();
        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Done!";
        if (Executor.allinone) {
            Executor.flushCSG();
            GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Need Manual Scripting!";
        }
        
        
        for (int i = 1; i < compNum+1; i++)
        {
            GameObject go1 = GameObject.Find("Collect").transform.Find("Comp_" + i).gameObject;
            GameObject go2 = GameObject.Find("Collect").transform.Find("Comp_" + (i + compNum * 3)).gameObject;
            GameObject go3 = GameObject.Find("Collect").transform.Find("Comp_" + (i + compNum * 1)).gameObject;
            GameObject go4 = GameObject.Find("Collect").transform.Find("Comp_" + (i + compNum * 2)).gameObject;
            go1.SetActive(true);
            go1.transform.GetChild(go1.transform.childCount - 1).gameObject.SetActive(true);
            go2.SetActive(true);
            go2.transform.GetChild(go2.transform.childCount - 1).gameObject.SetActive(true);
            go3.SetActive(false);
            go4.SetActive(false);
        }
    }

    void genOrderingInfo() {
        Vector3[] dirs = new Vector3[compNum * 2];
        int[] compIdxs = new int[compNum * 2];
        int cnt = 0;
        for (int i = 1; i < compNum + 1; i++)
        {
            if (Complist[i].angleArg == null)
            {
                dirs[cnt] = Complist[i].splitNorm;//應該要用find的，但assign時使用捷徑
                compIdxs[cnt] = i;
                cnt++;
                dirs[cnt] = dirs[cnt - 1] * -1;
                compIdxs[cnt] = i;
                cnt++;
            }
            else {
                AngleArg aa = Complist[i].angleArg;
                dirs[cnt] = BoundInfo.anglearg2vec(aa.axis, aa.norm, aa.angle2, aa.angle3);
                compIdxs[cnt] = i;
                cnt++;
                dirs[cnt] = -BoundInfo.anglearg2vec(aa.axis, aa.norm, aa.angle2_2, aa.angle3_2);
                compIdxs[cnt] = i;
                cnt++;
            }
            
        }
        genOrdering(compIdxs, dirs);
    }

    void genOrdering(int[] compIdxs, Vector3[] dirs) {//exhaustive
        StringBuilder sb = new StringBuilder();
        sb.Append((compNum * 2) + "\n");
        int[] ordering = new int[(compNum * 2)];
        int cnt = 0;
        for (int i = 0; i < compIdxs.Length; i++) {
            //MeshComponent mc = Pool.list[Pool.find("comp_" + compIdxs[i])];
            MeshComponent mc = Pool.list[Pool.find(Complist[compIdxs[i]].Name)];
            if (mc.edges.Count >= 1)
            {
                ordering[i] = cnt++;
            }
        }
        for (int i = 0; i < compIdxs.Length; i++)
        {
            //MeshComponent mc = Pool.list[Pool.find("comp_" + compIdxs[i])];
            MeshComponent mc = Pool.list[Pool.find(Complist[compIdxs[i]].Name)];
            if (mc.edges.Count == 0)
            {
                ordering[i] = cnt++;
            }
        }
        //output ordering

        for (int i = 0; i < compIdxs.Length; i++)
        {
            sb.Append(string.Format("{0} ", ordering[i]));
            sb.Append(string.Format("{0} {1} {2}\n", dirs[i].x, dirs[i].y, dirs[i].z));
        }
        string filename = "inputSet\\" + tarSet + "\\input\\ordering.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }

    void addSample() {
        GameObject[] Samples = {
            Pool.addPool("cube").Instance,
            Pool.addPool("cylinder").Instance,
            Pool.addPool("sphere").Instance,
            Pool.addPool("column").Instance,
            Pool.addPool("halftube").Instance };
        //GameObject obj = GameObject.Instantiate(Resources.Load("Comp"), Vector3.zero, Quaternion.identity) as GameObject;
        //obj.name = "Sample";
        //obj.transform.parent = GameObject.Find("Sample").transform;
        GameObject obj = GameObject.Find("Sample");
        Samples[0].transform.parent = obj.transform; Samples[0].SetActive(false);
        Samples[1].transform.parent = obj.transform; Samples[1].SetActive(false);
        Samples[2].transform.parent = obj.transform; Samples[2].SetActive(false);
        Samples[3].transform.parent = obj.transform; Samples[3].SetActive(false);
        Samples[4].transform.parent = obj.transform; Samples[4].SetActive(false);
    }

    int CSGMergeComp_NoCSG(int tar) {
        GameObject comp = GameObject.Find("Collect/Comp_" + tar);
        int childCount = comp.transform.childCount;
        GameObject root = comp.transform.GetChild(0).gameObject;
        int rootmcidx = Pool.find(root.name);
        Pool.list[rootmcidx].addRef(Pool.list[Pool.find(root.name)].vertices, Pool.list[Pool.find(root.name)].edges);

        int vertcnt = 0;
        MeshFilter[] mfs = comp.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < mfs.Length; i++) {
            vertcnt += mfs[i].mesh.vertices.Length;
        }
        Vector3[] vertices = new Vector3[vertcnt];
        Vector3[] normals = new Vector3[vertcnt];
        int tail = 0;
        for (int n = 0; n < mfs.Length; n++) {
            for (int i = 0; i < mfs[n].mesh.vertices.Length; i++) {
                Transform t = mfs[n].transform;
                vertices[tail] = t.rotation * MeshComponent.mul(t.lossyScale, mfs[n].mesh.vertices[i]) + t.position;
                normals[tail] = t.rotation * mfs[n].mesh.normals[i];
                tail++;
            }
        }
        int tricnt = 0;
        for (int i = 0; i < mfs.Length; i++)
        {
            tricnt += mfs[i].mesh.triangles.Length;
        }
        int[] triangles = new int[tricnt];
        int triflag=0;
        tail = 0;
        for (int n = 0; n < mfs.Length; n++)
        {
            for (int i = 0; i < mfs[n].mesh.triangles.Length; i++) {
                triangles[tail++] = mfs[n].mesh.triangles[i] + triflag;
            }
            triflag += mfs[n].mesh.vertices.Length;
        }
        print("Total Merged Vertices : " + vertices.Length);
        root.GetComponent<MeshFilter>().mesh.vertices = vertices;
        root.GetComponent<MeshFilter>().mesh.normals = normals;
        root.GetComponent<MeshFilter>().mesh.triangles = triangles;
        root.transform.position = Vector3.zero;
        root.transform.localScale = Vector3.one;
        root.transform.localRotation = Quaternion.identity;
        Pool.list[rootmcidx].filed = false;
        Pool.list[rootmcidx].renewFile();

        for (int i = 1; i < childCount; i++)
        {
            GameObject obj = comp.transform.GetChild(i).gameObject;
            obj.SetActive(false);
            MeshComponent objmc = Pool.list[Pool.find(obj.name)];
            Pool.list[rootmcidx].addRef(objmc.vertices, objmc.edges);
        }
        Complist[tar] = Pool.list[rootmcidx];
        return rootmcidx;
    }

    void CSGMergeComp(int tar)//精簡版
    {
        GameObject comp = GameObject.Find("Collect/Comp_" + tar);
        int childCount = comp.transform.childCount;
        if (childCount == 0) {
            MeshComponent temp = Pool.addPool("cube");
            temp.transform(
                    new Vector3(1, 1, 1),
                    new Vector3(10000, 10000, 10000),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0)
                );
            temp.Instance.transform.parent = comp.transform;
        }
        GameObject root = comp.transform.GetChild(0).gameObject;
        int rootmcidx = Pool.find(root.name);
        Pool.list[rootmcidx].addRef(Pool.list[Pool.find(root.name)].vertices, Pool.list[Pool.find(root.name)].edges);

        for (int i = 1; i < childCount; i++)
        {
            GameObject obj = comp.transform.GetChild(i).gameObject;
            if (i < childCount - 1) CSGQueue.addCSGSet_skip("+", root.name, root.name, obj.name);
            else CSGQueue.addCSGSet("+", root.name, root.name, obj.name);
            obj.SetActive(false);
            MeshComponent objmc = Pool.list[Pool.find(obj.name)];
            Pool.list[rootmcidx].addRef(objmc.vertices, objmc.edges);
        }
        Complist[tar] = Pool.list[rootmcidx];
    }

    /*
    void CSGMergeComp(int tar) {
        GameObject comp = GameObject.Find("Collect/Comp_" + tar);
        int childCount = comp.transform.childCount;  
        GameObject root = comp.transform.GetChild(0).gameObject;
        MeshComponent rootmc = new MeshComponent("comp_" + tar, Instantiate(root, root.transform.parent));
        Pool.list.Add(rootmc);

        int rootmcidx = Pool.find(rootmc.Name);
        Pool.list[rootmcidx].addRef(Pool.list[Pool.find(root.name)].vertices, Pool.list[Pool.find(root.name)].edges);

        root.SetActive(false);
        root = rootmc.Instance;
        for (int i = 1; i < childCount; i++) {
            GameObject obj = comp.transform.GetChild(i).gameObject;
            if(i< childCount-1)CSGQueue.addCSGSet_skip("+", root.name, root.name, obj.name);
            else CSGQueue.addCSGSet("+", root.name, root.name, obj.name);
            obj.SetActive(false);
            MeshComponent objmc = Pool.list[Pool.find(obj.name)];
            Pool.list[rootmcidx].addRef(objmc.vertices, objmc.edges);
        }
        Complist[tar] = Pool.list[rootmcidx];
    }
    */

    void collectComps()
    {
        GameObject Collect = GameObject.Find("Collect");
        int childCount = Collect.transform.childCount;
        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i < childCount; i++)
        {
            if (Collect.transform.GetChild(i).gameObject.name == "Sample") ;
            else if (Collect.transform.GetChild(i).gameObject.name.Split('_')[0] == "Comp") ;
            else objs.Add(Collect.transform.GetChild(i).gameObject);
        }
        foreach (GameObject obj in objs)
        {
            GameObject Comp = GameObject.Instantiate(Resources.Load("Comp"), Vector3.zero, Quaternion.identity) as GameObject;
            Comp.name = "Comp_" + (curComp++);
            Comp.transform.parent = Collect.transform;
            Queue<GameObject> toOpen = new Queue<GameObject>();
            toOpen.Enqueue(obj);
            while (toOpen.Count > 0)
            {
                GameObject cur = toOpen.Dequeue();
                int curChildCount = cur.transform.childCount;
                for (int j = 0; j < curChildCount; j++)
                {
                    GameObject tar = cur.transform.GetChild(j).gameObject;
                    toOpen.Enqueue(tar);
                }
                cur.transform.parent = Comp.transform;
            }
        }
    }
    void deleteLastComp() {
        int tar = --curComp;
        GameObject Comp = GameObject.Find("Collect/comp_" + tar);
        //MeshComponent compmc = Pool.list[Pool.find("comp_" + tar)];
        MeshComponent compmc = Pool.list[Pool.find(Complist[tar].Name)];
        Pool.list.Remove(compmc);
        Complist[tar] = null;
        GameObject.Destroy(Comp);
    }

    GameObject copyCompByInst(int tar, Quaternion rotation)
    {
        GameObject Comp = GameObject.Instantiate(Resources.Load("Comp"), Vector3.zero, Quaternion.identity) as GameObject;
        Comp.transform.parent = GameObject.Find("Collect").transform;
        Comp.name = "Comp_" + (curComp++);
        int newtar = curComp - 1;
        GameObject Inst = Instantiate(Complist[tar].Instance, Comp.transform);
        Inst.transform.rotation = rotation * Complist[tar].Instance.transform.rotation;
        //Inst.transform.localScale = Complist[tar].Instance.transform.localScale;
        //Inst.transform.position = Complist[tar].Instance.transform.position;
        MeshComponent mc = new MeshComponent("comp_" + newtar, Inst);
        Pool.list.Add(mc);
        Complist[newtar] = mc;
        return mc.Instance;
    }

    GameObject copyComp(int tar)
    {
        GameObject Comp = GameObject.Instantiate(Resources.Load("Comp"), Vector3.zero, Quaternion.identity) as GameObject;
        Comp.transform.parent = GameObject.Find("Collect").transform;
        Comp.name = "Comp_" + (curComp++);
        int newtar = curComp - 1;
        MeshComponent mc = new MeshComponent("comp_" + newtar, Instantiate(Complist[tar].Instance, Comp.transform));
        Pool.list.Add(mc);
        Complist[newtar] = mc;
        //CSGQueue.addCSGSet("COPY", "comp_" + newtar, "comp_" + tar, "");
        CSGQueue.addCSGSet("COPY", "comp_" + newtar, Complist[tar].Name, "");
        return mc.Instance;
    }

    MeshComponent addCompToPool(int type, int idx) {
        /*
         * type0 : node;
         * type1 : edge;
         */
        if (type == 0)
        {
            MeshComponent comp = Pool.addPool("sphere");
            if (idx >= 0) Pool.list[Pool.find(comp.Name)].vertices.Add(idx);
            return comp;
        }
        else if (type == 1)
        {
            MeshComponent comp = Pool.addPool("cylinder");
            if (idx >= 0) Pool.list[Pool.find(comp.Name)].edges.Add(idx);
            return comp;
        }
        else if (type == 11)
        {
            MeshComponent comp = Pool.addPool("halftube");
            if (idx >= 0) Pool.list[Pool.find(comp.Name)].edges.Add(idx);
            return comp;
        }
        else if (type == 2)
        {
            MeshComponent comp = Pool.addPool("column");
            if (idx >= 0) Pool.list[Pool.find(comp.Name)].edges.Add(idx);
            return comp;
        }
        else if (type == 3)
        {
            MeshComponent comp = Pool.addPool("cube");
            if (idx >= 0) Pool.list[Pool.find(comp.Name)].edges.Add(idx);
            return comp;
        }
        else {

        }
        return null;
    }

    /**********************************************************************************************************************************************/
    /**********************************************************************************************************************************************/
    /**********************************************************************************************************************************************/

    static float iR = 0.1f;
    static float oR = 0.2f;
    static float ioR = (iR + oR) / 2;
    static float mul = 50;
    static float iRreal = iR * mul;
    static float oRreal = oR * mul;
    static float ioRreal = ioR * mul;
    void genAssemComps()
    {
        int columntype = simple ? 3 : 1;
        MeshComponent[] edgeInstance = new MeshComponent[ThinStructure.edgeNum];
        for (int i = 0; i < edgeInstance.Length; i++) edgeInstance[i] = null;
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            int thisvert = i;
            //MeshComponent thisvertInstance = Pool.addPool("sphere");
            MeshComponent thisvertInstance = addCompToPool(0, thisvert);
            thisvertInstance.transform(
                new Vector3(oR, oR, oR),
                ThinStructure.vertices[thisvert],
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0)
            );
            foreach (int edge in ThinStructure.verticesedges[thisvert])
            {
                bool swap = !(thisvert == ThinStructure.edges[edge].idx1);
                int thatvert = !swap ? ThinStructure.edges[edge].idx2 : ThinStructure.edges[edge].idx1;
                Vector3 v1 = ThinStructure.vertices[thisvert];
                Vector3 v2 = ThinStructure.vertices[thatvert];
                Vector3 n = (v2 - v1).normalized;
                float len = (v2 - v1).magnitude;
                float angleFix1 = Algorithm.angleFix(edge, swap?1:0, oRreal);
                float angleFix2 = Algorithm.angleFix(edge, swap?0:1, oRreal);
                Vector3 p1 = ThinStructure.vertices[thisvert] + angleFix1 * n;
                Vector3 p2 = ThinStructure.vertices[thisvert] + (len - angleFix2) * n;
                if (!ThinStructure.linkInfo[thisvert].Contains(edge))
                {
                    MeshComponent columnInstance = addCompToPool(columntype, -1);
                    columnInstance.transform(
                        new Vector3(oR, oR, (p1 - v1).magnitude / (mul * 2)),
                        (p1 + v1) / 2,
                        p1 - v1,
                        -ThinStructure.splitNorms[edge]
                    );
                    if (!ThinStructure.linkInfo[thatvert].Contains(edge) && edgeInstance[edge] == null)
                    {
                        MeshComponent indepClmnInstance = addCompToPool(11, edge);
                        indepClmnInstance.transform(
                            new Vector3(oR, oR, (p2 - p1).magnitude / (mul * 2)),
                            (v1 + v2) / 2,
                            v2 - v1,
                            -ThinStructure.splitNorms[edge]
                        );
                        edgeInstance[edge] = indepClmnInstance;
                    }
                    thisvertInstance = thisvertInstance + columnInstance;
                }
                else
                {
                    if (edgeInstance[edge] == null)
                    {
                        MeshComponent columnInstance = addCompToPool(columntype, edge);
                        if (!ThinStructure.linkInfo[thatvert].Contains(edge))
                        {
                            columnInstance.transform(
                                new Vector3(oR, oR, (p2 - v1).magnitude / (mul * 2)),
                                (p2 + v1) / 2,
                                p2 - v1,
                                -ThinStructure.splitNorms[edge]
                            );
                            thisvertInstance = thisvertInstance + columnInstance;
                            edgeInstance[edge] = columnInstance;
                            //short
                        }
                        else
                        {
                            columnInstance.transform(
                                new Vector3(oR, oR, (v2 - v1).magnitude / (mul * 2)),
                                (v2 + v1) / 2,
                                v2 - v1,
                                -ThinStructure.splitNorms[edge]
                            );
                            thisvertInstance = thisvertInstance + columnInstance;
                            edgeInstance[edge] = columnInstance;
                            //long
                        }
                    }
                    else
                    {
                        if (edgeInstance[edge].Instance == thisvertInstance.Instance) ;
                        else
                        {
                            thisvertInstance = thisvertInstance + edgeInstance[edge];
                            edgeInstance[edge] = thisvertInstance;
                            //merge
                        }
                    }
                }
            }
        }
    }

    void genAssemComps2()//All Cut
    {
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            int thisvert = i;
            if (ThinStructure.verticesedges[thisvert].Count == 1) continue;
            MeshComponent thisvertInstance = Pool.addPool("sphere");
            thisvertInstance.transform(
                new Vector3(oR, oR, oR),
                ThinStructure.vertices[thisvert],
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0)
            );
            foreach (int edge in ThinStructure.verticesedges[thisvert])
            {
                int thatvert = (thisvert == ThinStructure.edges[edge].idx1) ? ThinStructure.edges[edge].idx2 : ThinStructure.edges[edge].idx1;
                Vector3 v1 = ThinStructure.vertices[thisvert];
                Vector3 v2 = ThinStructure.vertices[thatvert];
                Vector3 n = (v2 - v1).normalized;
                float len = (v2 - v1).magnitude;
                float angleFix1 = Algorithm.angleFix(edge, 0, oR * mul);
                float angleFix2 = Algorithm.angleFix(edge, 1, oR * mul);
                Vector3 p1 = ThinStructure.vertices[thisvert] + angleFix1 * n;
                Vector3 p2 = ThinStructure.vertices[thisvert] + (len - angleFix2) * n;
                MeshComponent columnInstance = Pool.addPool("cylinder");
                columnInstance.transform(
                    new Vector3(oR, oR, (p1 - v1).magnitude / (mul * 2)),
                    (p1 + v1) / 2,
                    p1 - v1,
                    -ThinStructure.splitNorms[edge]
                );
                thisvertInstance = thisvertInstance + columnInstance;
            }
        }
        //edges
        int cnt = 0;
        foreach (Edge edge in ThinStructure.edges) {
            Vector3 v1 = ThinStructure.vertices[edge.idx1];
            Vector3 v2 = ThinStructure.vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized;
            float len = (v2 - v1).magnitude;
            bool l1 = (ThinStructure.verticesedges[edge.idx1].Count == 1);
            bool l2 = (ThinStructure.verticesedges[edge.idx2].Count == 1);
            Vector3 p1 = v1, p2 = v2;
            if (!l1) p1 = v1 + oRreal * vec;
            if (!l2) p2 = v2 - oRreal * vec;
            MeshComponent columnInstance = Pool.addPool("cylinder");
            columnInstance.transform(
                new Vector3(oR, oR, (p2 - p1).magnitude / (mul * 2)),
                (p2 + p1) / 2,
                (p2 - p1),
                -ThinStructure.splitNorms[cnt++]
            );
            if (l1) {
                MeshComponent vertInstance = Pool.addPool("sphere");
                vertInstance.transform(
                    new Vector3(oR, oR, oR),
                    ThinStructure.vertices[edge.idx1],
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0)
                );
                columnInstance = columnInstance + vertInstance;
            }
            if (l2)
            {
                MeshComponent vertInstance = Pool.addPool("sphere");
                vertInstance.transform(
                    new Vector3(oR, oR, oR),
                    ThinStructure.vertices[edge.idx2],
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0)
                );
                columnInstance = columnInstance + vertInstance;
            }
        }
    }

    void genConn(int tar) {
        //MeshComponent comp = Pool.list[Pool.find("comp_" + tar)];
        MeshComponent comp = Pool.list[Pool.find(Complist[tar].Name)];
        int verticeCount = comp.vertices.Count;
        int edgeCount = comp.edges.Count;

        if (verticeCount <= 2 && edgeCount == 1)
        {
            foreach (int edge in comp.edges)
            {
                genEdgeConn(edge);
            }
        }
        else if (verticeCount == 1)
        {
            foreach (int node in comp.vertices)
            {
                genNodeConn(node);
            }
        }
        else if (verticeCount > 1 && edgeCount > 1)
        {
            int[] nodes = new int[comp.vertices.Count];
            int i = 0;
            foreach (int v in comp.vertices)
            {
                nodes[i++] = v;
            }
            genCurveConn(nodes);
        }
        else {
            print("conn space not contain : " + verticeCount + " / "+ edgeCount);
        }
    }

    void genCurveConn(int[] nodes) {
        GameObject Comp = genEmptyCollect();
        int cnt = 0;
        foreach (int node in nodes)
        {
            Vector3 pos = ThinStructure.vertices[node];
            foreach (int e in ThinStructure.verticesedges[node])
            {
                if (ThinStructure.linkInfo[node].Contains(e)) return;
                MeshComponent cube = Pool.addPool("cylinder");
                Edge edge = ThinStructure.edges[e];
                int evi1 = edge.idx1 == node ? edge.idx1 : edge.idx2;
                int evi2 = edge.idx1 == node ? edge.idx2 : edge.idx1;
                Vector3 v1 = ThinStructure.vertices[evi1];
                Vector3 v2 = ThinStructure.vertices[evi2];
                Vector3 vec = (v2 - v1).normalized;
                v2 = v1 + vec * (oRreal + iRreal);
                Vector3 cent = (v2 + v1) / 2;
                Vector3 norm = ThinStructure.splitNorms[e].normalized;
                float len = (v2 - v1).magnitude;
                cube.transform(
                    new Vector3(ioR, ioR, len / mul / 2),
                    cent,
                    vec,
                    norm
                );
                cube.Instance.transform.parent = Comp.transform;
                cnt++;
            }
        }
        if (cnt == 0) {
            MeshComponent cube = Pool.addPool("cylinder");
            cube.transform(
                    new Vector3(0.001f, 0.001f, 0.001f),
                    new Vector3(10000f, 10000f, 10000f),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0)
                );
            cube.Instance.transform.parent = Comp.transform;
        }
        CSGMergeComp(curComp - 1);
    }

    void genNodeConn(int node)
    {
        GameObject Comp = genEmptyCollect();
        Vector3 pos = ThinStructure.vertices[node];
        foreach (int e in ThinStructure.verticesedges[node])
        {
            if (ThinStructure.linkInfo[node].Contains(e)) return;
            MeshComponent cube = Pool.addPool("cylinder");
            Edge edge = ThinStructure.edges[e];
            int evi1 = edge.idx1 == node ? edge.idx1 : edge.idx2;
            int evi2 = edge.idx1 == node ? edge.idx2 : edge.idx1;
            Vector3 v1 = ThinStructure.vertices[evi1];
            Vector3 v2 = ThinStructure.vertices[evi2];
            Vector3 vec = (v2 - v1).normalized;
            v2 = v1 + vec * (oRreal + iRreal);
            Vector3 cent = (v2 + v1) / 2;
            Vector3 norm = ThinStructure.splitNorms[e].normalized;
            float len = (v2 - v1).magnitude;
            cube.transform(
                new Vector3(ioR, ioR, len / mul / 2),
                cent,
                vec,
                norm
            );
            cube.Instance.transform.parent = Comp.transform;
        }
        CSGMergeComp(curComp - 1);
    }

    void genEdgeConn(int e)
    {
        GameObject Comp = genEmptyCollect();
        Edge edge = ThinStructure.edges[e];
        Vector3 v1 = ThinStructure.vertices[edge.idx1];
        Vector3 v2 = ThinStructure.vertices[edge.idx2];
        Vector3 p1 = v1;
        Vector3 p2 = v2;
        Vector3 vec = (p2 - p1).normalized;
        float angleFix1 = Algorithm.angleFix(e, 0, oRreal);
        float angleFix2 = Algorithm.angleFix(e, 1, oRreal);
        p2 = p1 + vec * (angleFix1 + iRreal);
        p1 = p1 + vec * (angleFix1 - iRreal);
        Vector3 cent = (p2 + p1) / 2;
        Vector3 norm = ThinStructure.splitNorms[e].normalized;
        float len = (p2 - p1).magnitude;
        if (!ThinStructure.linkInfo[edge.idx1].Contains(e)) {
            MeshComponent cylinder = Pool.addPool("cylinder");
            cylinder.transform(
                    new Vector3(ioR, ioR, len / mul / 2),
                    cent,
                    vec,
                    norm
                );
            cylinder.Instance.transform.parent = Comp.transform;
        }
        //////////////////////////
        p1 = v2;
        p2 = v1;
        vec = (p2 - p1).normalized;
        p2 = p1 + vec * (angleFix2 + iRreal);
        p1 = p1 + vec * (angleFix2 - iRreal);
        cent = (p2 + p1) / 2;
        norm = ThinStructure.splitNorms[e].normalized;
        len = (p2 - p1).magnitude;
        if (!ThinStructure.linkInfo[edge.idx2].Contains(e))
        {
            MeshComponent cylinder = Pool.addPool("cylinder");
            cylinder.transform(
                    new Vector3(ioR, ioR, len / mul / 2),
                    cent,
                    vec,
                    norm
                );
            cylinder.Instance.transform.parent = Comp.transform;
        }
        //////////////////////////
        CSGMergeComp(curComp - 1);
    }

    void genNodeFix(int node) {
        GameObject Comp = genEmptyCollect();
        Vector3 pos = ThinStructure.vertices[node];
        foreach (int e in ThinStructure.verticesedges[node])
        {
            MeshComponent cube = Pool.addPool("cube");
            Edge edge = ThinStructure.edges[e];
            Vector3 v1 = ThinStructure.vertices[edge.idx1];
            Vector3 v2 = ThinStructure.vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized;
            Vector3 cent = (v2 + v1) / 2;
            Vector3 norm = ThinStructure.splitNorms[e].normalized;
            float len = (v2 - v1).magnitude;
            cube.transform(
                new Vector3(oR, len / mul / 2 - oR, oR),
                cent,
                norm,
                vec
            );
            cube.Instance.transform.parent = Comp.transform;
        }
        CSGMergeComp(curComp - 1);
    }

    bool compNoSol(int tar) {
        //MeshComponent comp = Pool.list[Pool.find("comp_" + tar)];
        MeshComponent comp = Pool.list[Pool.find(Complist[tar].Name)];
        bool nosol = false;
        foreach (int v in comp.vertices)
        {
            if (ThinStructure.solvedinfo[v] < 1)
            {
                nosol = true;
            }
        }
        foreach (int e in comp.edges)
        {
            if (ThinStructure.solvedinfo[e + ThinStructure.verticeNum] < 2)
            {
                nosol = true;
            }
        }
        return nosol;
    }

    void genCut(int tar) {
        if (compNoSol(tar)) {
            GameObject Comp = genEmptyCollect();
            MeshComponent cube = Pool.addPool("cube");
            cube.Instance.transform.parent = Comp.transform;
            cube.transform(
                new Vector3(1, 1, 1),
                new Vector3(10000, 10000, 10000),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0)
            );
            CSGMergeComp(curComp - 1);
            return;
        }
        /*************************************/
        //MeshComponent comp = Pool.list[Pool.find("comp_" + tar)];
        //MeshComponent comp = Pool.list[Pool.find(Complist[tar].Name)];
        MeshComponent comp = Complist[tar];

        int verticeCount = comp.vertices.Count;
        int edgeCount = comp.edges.Count;
        Vector3 splitNorm = Vector3.zero;
        Vector3 splitDir1 = Vector3.zero;
        Vector3 splitDir2 = Vector3.zero;
        if (verticeCount <= 2 && edgeCount == 1)
        {
            foreach (int e in comp.edges)
            {
                splitNorm = genEdgeCut(e);
                AngleArg aa = ThinStructure.angleArgs[e];
                aa.norm = splitNorm;aa.axis = ThinStructure.edges[e].vec;
                Complist[tar].angleArg = Pool.list[Pool.find(Complist[tar].Name)].angleArg = aa;
            }
        }
        else if (verticeCount == 1)
        {
            foreach (int node in comp.vertices)
            {
                splitNorm = genNodeCut(node);
            }
        }
        else if (verticeCount > 1 && edgeCount > 1)
        {
            Vector3[] edgesvec = new Vector3[comp.edges.Count];
            Vector3[] edgescent = new Vector3[comp.edges.Count];
            int[] edgesidx = new int[comp.edges.Count];
            int i = 0;
            foreach (int e in comp.edges)
            {
                Edge edge = ThinStructure.edges[e];
                edgesvec[i] = (ThinStructure.vertices[edge.idx2] - ThinStructure.vertices[edge.idx1]).normalized;
                edgescent[i] = (ThinStructure.vertices[edge.idx2] + ThinStructure.vertices[edge.idx1]) / 2;
                edgesidx[i] = e;
                i++;
            }
            //splitNorm = genCurveCut(edgescent, edgesvec);
            splitNorm = genCurveCut(edgesidx);
        }
        else {
            print("cut space not contain : " + verticeCount + " / " + edgeCount);
        }
        Complist[tar].splitNorm = Pool.list[Pool.find(Complist[tar].Name)].splitNorm = splitNorm;//最終使用Complist比較快
    }
    Vector3 genCurveCut(int[] edgesidx)
    {
        GameObject Comp = genEmptyCollect();
        Vector3 stdNorm = ThinStructure.splitNorms[edgesidx[edgesidx.Length/2]];
        foreach (int idx in edgesidx) {
            MeshComponent cube = Pool.addPool("cube");
            cube.Instance.transform.parent = Comp.transform;
            Vector3 vec = ThinStructure.edges[idx].vec;
            Vector3 cent = ThinStructure.edges[idx].cent;
            Vector3 norm = ThinStructure.splitNorms[idx];
            if (Vector3.Dot(stdNorm, norm) < 0) norm *= -1;
            float len = ThinStructure.edges[idx].len;
            cube.transform(
                new Vector3(oR * 1.001f, len / mul, oR),
                cent - norm * oR * mul,
                norm,
                vec
            );
        }
        CSGMergeComp(curComp - 1);
        return stdNorm;
    }
    Vector3 genCurveCut(Vector3[] edgescent, Vector3[] edgesvec) {
        GameObject Comp = genEmptyCollect();
        MeshComponent cube = Pool.addPool("cube");
        Vector3 cent=Vector3.zero;
        foreach (Vector3 c in edgescent) {
            cent += c;
        }
        cent /= edgescent.Length;
        Vector3 vec1 = edgesvec[0];
        Vector3 vec2 = edgesvec[edgesvec.Length-1];
        Vector3 cent1 = edgescent[0];
        Vector3 cent2 = edgescent[edgescent.Length - 1];
        Vector3 norm = (Vector3.Cross(vec1, vec2)).normalized;
        cube.transform(
            new Vector3(10, 10, oR / 2),
            cent - norm * oR * mul / 2,
            norm,
            cent2- cent1
        );
        cube.Instance.transform.parent = Comp.transform;
        CSGMergeComp(curComp - 1);
        return norm;
    }
    Vector3 genEdgeCut(int e) {
        GameObject Comp = genEmptyCollect();
        MeshComponent cube = Pool.addPool("cube");
        Edge edge = ThinStructure.edges[e];
        Vector3 v1 = ThinStructure.vertices[edge.idx1];
        Vector3 v2 = ThinStructure.vertices[edge.idx2];
        Vector3 vec = (v2 - v1).normalized;
        Vector3 cent = (v2 + v1) / 2;
        Vector3 norm = ThinStructure.splitNorms[e].normalized;
        float len = (v2 - v1).magnitude;
        cube.transform(
            new Vector3(oR * 1.001f, len / mul, oR),
            cent - norm * oR * mul,
            norm,
            vec
        );
        cube.Instance.transform.parent = Comp.transform;
        CSGMergeComp(curComp - 1);
        return norm;
    }
    Vector3 genNodeCut(int node) {
        GameObject Comp = genEmptyCollect();
        Vector3 pos = ThinStructure.vertices[node];
        List<Vector3> norms = new List<Vector3>();
        List<Vector3> vecs = new List<Vector3>();
        foreach (int e in ThinStructure.verticesedges[node])
        {
            Edge edge = ThinStructure.edges[e];
            int idx1 = node == edge.idx1 ? edge.idx1 : edge.idx2;
            int idx2 = node == edge.idx2 ? edge.idx1 : edge.idx2;
            Vector3 v1 = ThinStructure.vertices[idx1];
            Vector3 v2 = ThinStructure.vertices[idx2];
            Vector3 vec = (v2 - v1).normalized;
            vecs.Add(vec);
        }
        Vector3 common = Vector3.zero;
        norms = Algorithm.nodeNorms(vecs, node, ref common);
        int cnt = 0;
        foreach (int e in ThinStructure.verticesedges[node]) {
            MeshComponent cube = Pool.addPool("cube");
            Edge edge = ThinStructure.edges[e];
            Vector3 v1 = ThinStructure.vertices[edge.idx1];
            Vector3 v2 = ThinStructure.vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized;
            Vector3 cent = (v2 + v1) / 2;
            //Vector3 norm = ThinStructure.splitNorms[e].normalized;
            Vector3 norm = norms[cnt++]; 
            float len = (v2 - v1).magnitude;
            cube.transform(
                new Vector3(oR * 1.001f, len / mul, oR),
                cent - norm * oR * mul,
                norm,
                vec
            );
            cube.Instance.transform.parent = Comp.transform;
        }
        CSGMergeComp(curComp - 1);
        return common;
    }

    void genNodeCut2(int node, Vector3 plane)
    {
        GameObject Comp = genEmptyCollect();
        Vector3 pos = ThinStructure.vertices[node];
        MeshComponent cut = Pool.addPool("cube");
        cut.transform(
            new Vector3(1, 1, 1),
            pos - plane.normalized * 50,
            plane,
            new Vector3(plane.y, plane.x, plane.z)
        );
        cut.Instance.transform.parent = Comp.transform;

        int en = ThinStructure.verticesedges[node].Count;
        int[] es = new int[en];
        int i = 0;
        foreach (int e in ThinStructure.verticesedges[node])
        {
            es[i++] = e;
        }
        for (i = 0; i < en; i++)
        {
            for (int j = i + 1; j < en; j++)
            {
                int e1 = es[i];
                int e2 = es[j];
                Edge edge1 = ThinStructure.edges[e1];
                Edge edge2 = ThinStructure.edges[e2];
                Vector3 v11 = ThinStructure.vertices[edge1.idx1];
                Vector3 v12 = ThinStructure.vertices[edge1.idx2];
                Vector3 v21 = ThinStructure.vertices[edge2.idx1];
                Vector3 v22 = ThinStructure.vertices[edge2.idx2];

                Vector3 vec1 = (v12 - v11).normalized;
                Vector3 vec2 = (v22 - v21).normalized;
                Vector3 vec = (vec1 + vec2).normalized;
                Vector3 norm = Vector3.Cross(vec1, vec2).normalized;
                if (Vector3.Dot(norm, plane) < 0)
                {
                    norm *= -1;
                }
                Vector3 cent = pos + vec * oR * mul;
                float len = oR * mul / 2;
                float width = (vec1 * oR * mul - vec2 * oR * mul).magnitude;
                if (Vector3.Dot(norm, plane) > 0.95) continue;
                MeshComponent cube = Pool.addPool("cube");
                cube.transform(
                    new Vector3(width / 2 / mul, len / mul, oR / 2),
                    cent - norm * oR * mul / 2,
                    norm,
                    vec
                );
                cube.Instance.transform.parent = Comp.transform;
            }
        }
        CSGMergeComp(curComp - 1);
    }

    void genThinComp() {
        if (!geninside) {
            GameObject tempComp = genEmptyCollect();
            MeshComponent cube = Pool.addPool("cube");
            cube.Instance.transform.parent = tempComp.transform;
            cube.transform(
                new Vector3(1, 1, 1),
                new Vector3(10000, 10000, 10000),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0)
            );
            CSGMergeComp(curComp - 1);
            return;
        }
        GameObject Comp = genEmptyCollect();
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            Vector3 vertice = ThinStructure.vertices[i];
            MeshComponent thisvertInstance = Pool.addPool("sphere");
            thisvertInstance.transform(
                new Vector3(iR, iR, iR),
                vertice,
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0)
            );
            thisvertInstance.Instance.transform.parent = Comp.transform;
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++)
        {
            Edge edge = ThinStructure.edges[i];
            Vector3 v1 = ThinStructure.vertices[edge.idx1];
            Vector3 v2 = ThinStructure.vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized;
            float mag = (v2 - v1).magnitude;
            Vector3 cent = (v2 + v1) / 2;
            Vector3 norm = ThinStructure.splitNorms[i].normalized;
            norm = Vector3.Cross(Vector3.Cross(vec, norm), vec);

            MeshComponent thisedgeInstance = Pool.addPool("cylinder");
            thisedgeInstance.transform(
                new Vector3(iR, iR, mag/mul/2),
                (v1+v2)/2,
                vec,
                norm
            );
            thisedgeInstance.Instance.transform.parent = Comp.transform;
        }
        int rootidx = CSGMergeComp_NoCSG(curComp - 1);
        Pool.list[rootidx].renewFile();
        return;
    }

    GameObject genEmptyCollect() {
        GameObject Comp = GameObject.Instantiate(Resources.Load("Comp"), Vector3.zero, Quaternion.identity) as GameObject;
        Comp.transform.parent = GameObject.Find("Collect").transform;
        Comp.name = "Comp_" + (curComp++);
        return Comp;
    }

/**********************************************************************************************************************************************/
/**********************************************************************************************************************************************/
/**********************************************************************************************************************************************/

    // Update is called once per frame
    void Update () {
        //fetch new done object
        CSGQueue.fetchFromReady();
        //send new object to CSG
        CSGQueue.executeSingle();
    }
    private void OnDisable()
    {
        try
        {

        }
        catch
        {

        }
    }
}
//To-do
//decrease the case of duplicated reading
//translate in csgtool?