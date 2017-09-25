using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SplitNormInfo
{
    public Vector3 norm;
    public Vector3 cent;
    public int oriid;
    public int mapto;
    public bool toDelete;
}
public class CubeVolumn : MonoBehaviour {
    public static int tarSet;
    Cubic[] cubics;
    int[][][] cubics_xyz;
    public static Vector3 max;
    public static Vector3 min;
    public static float radii;
    int nx, ny, nz;
    public static SplitNormInfo[] splitNormInfo;
    public static SplitNormInfo[] splitNormUsage;
    public static SortedDictionary<int, GameObject> hasid = new SortedDictionary<int, GameObject>();
    public static SortedDictionary<string, PieceInfo> pieceMap = new SortedDictionary<string, PieceInfo>();
    public static PieceInfo[] pieceArr;
    public static string[] strMap;
    public static List<PieceGroupInfo> pieceGroup = new List<PieceGroupInfo>();
    public static SortedDictionary<int, PieceGroupInfo> GroupMap = new SortedDictionary<int, PieceGroupInfo>();
    public static int groupIdCnt = 0;
    public static PieceGroupInfo[] pieceGroupArr;
    public static int[] pieceBelong;
    public static int[][] pieceLink;
    public static int[][] pieceLinkPlane;
    public static HashSet<int>[] pieceLinkTable;
    public static int[][] groupLink;
    public static HashSet<int>[] groupLinkTable;
    PriorityQueue<GroupLinkInfo> linkQueue = new PriorityQueue<GroupLinkInfo>();
    public static Transform Assist;
    List<GameObject> cavity = new List<GameObject>();
    
    public void start()
    {
        Tool.clearObj();
        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "";
        Assist = GameObject.Find("Assist").transform;
        init();
        try
        {
            string inputtext = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
            string[] inputtexts = inputtext.Split('*');
            if (inputtexts.Length > 1) ThinStructure.myscale = int.Parse(inputtexts[1]);
            tarSet = int.Parse(inputtexts[0]);
        }
        catch
        {
            print("bad number");
            return;
        }

        ThinStructure.tuberadii = 5f;
        radii = ThinStructure.tuberadii / 2;
        ThinStructure.basicRead(tarSet);
        calMaxBounding();
        calBounding();
        moveToOrigin();
        ThinStructure.basicPut();
        initSplitNorm();
        //StartCoroutine(putCubeVolumn());
        StartCoroutine(igenShape());
    }
    public void init() {
        splitNormInfo = null;
        splitNormUsage = null;
        hasid = new SortedDictionary<int, GameObject>();
        pieceMap = new SortedDictionary<string, PieceInfo>();
        pieceArr = null;
        strMap = null;
        pieceGroup = new List<PieceGroupInfo>();
        GroupMap = new SortedDictionary<int, PieceGroupInfo>();
        groupIdCnt = 0;
        pieceGroupArr = null;
        pieceBelong = null;
        pieceLink = null;
        pieceLinkPlane = null;
        pieceLinkTable = null;
        groupLink = null;
        groupLinkTable = null;
        linkQueue = new PriorityQueue<GroupLinkInfo>();
}
    IEnumerator igenShape()
    {
        GameObject Shape = GameObject.Instantiate(Resources.Load("Space") as GameObject, (min + max) / 2, Quaternion.identity);
        Transform ToDelete = GameObject.Find("ToDelete").transform;
        Shape.transform.parent = GameObject.Find("Collect").transform;
        Shape.transform.localScale = max - min;
        Material mat = Shape.GetComponent<MeshRenderer>().material;
        Queue<GameObject> Cutor = new Queue<GameObject>();
        Queue<List<char>> Cutor_str = new Queue<List<char>>();
        Vector3 scale = Shape.transform.localScale;
        Transform collect = GameObject.Find("Collect").transform;
        Cutor.Enqueue(Shape);
        Cutor_str.Enqueue(new List<char>());
        int num = 0;
        int cnt = 1;
        float smallVol = 0;
        for (int i = 0; i < splitNormUsage.Length; i++)
        {
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + i + "/" + splitNormUsage.Length + ")";
            yield return new WaitForSeconds(0);
            int cnt_new = 0;
            for (int j = 0; j < cnt; j++) {
                GameObject go = Cutor.Dequeue();
                List<char> chs = Cutor_str.Dequeue();
                go.transform.parent = ToDelete;
                Mesh mesh = go.GetComponent<MeshFilter>().mesh;
                GameObject[] newbies = BLINDED_AM_ME.MeshCut.Cut(go, splitNormUsage[i].cent, splitNormUsage[i].norm, null);
                newbies[0].transform.localScale = newbies[1].transform.localScale = scale;
                if (Tool.calVolume(newbies[0]) < smallVol) { newbies[1].GetComponent<MeshFilter>().mesh = mesh; }
                if (Tool.calVolume(newbies[1]) < smallVol) { newbies[0].GetComponent<MeshFilter>().mesh = mesh; }
                //print("--");
                for (int k = 0; k < newbies.Length; k++)
                {
                    GameObject shape = newbies[k];
                    List<char> chs_new = new List<char>(chs.ToArray());
                    chs_new.Add(k == 0 ? '-' : '*');
                    if (shape.GetComponent<MeshFilter>().mesh.vertexCount == 0 || Tool.calVolume(newbies[k]) < smallVol) {
                        shape.transform.parent = ToDelete;
                        Destroy(shape);
                        continue;
                    }
                    Cutor.Enqueue(shape);
                    Cutor_str.Enqueue(chs_new);
                    cnt_new++;
                    shape.name = "shape_" + (num++);
                    shape.transform.parent = collect;
                    shape.transform.localScale = scale;
                    shape.SetActive(false);
                }
            }
            cnt = cnt_new;
        }
        /***************************************************************/
        //初始map
        List<PieceInfo> pieceList = new List<PieceInfo>();
        while (true) {
            bool isempty = true;
            foreach (var x in Cutor)
            {
                isempty = false;
                break;
            }
            if (isempty) break;
            GameObject go = Cutor.Dequeue();
            List<char> chs = Cutor_str.Dequeue();
            string str = new string(chs.ToArray());
            PieceInfo pi = new PieceInfo();
            pi.valstr = str;
            pi.id = pieceMap.Count;
            pi.instance = go;
            pi.calVolume();
            pi.setCenter();
            pieceMap.Add(str, pi);
            pieceList.Add(pi);
            go.GetComponent<MeshFilter>().mesh.RecalculateBounds();//#need?
            go.GetComponent<MeshFilter>().mesh.RecalculateNormals();//#need?
            go.AddComponent<MeshCollider>();
            go.name = "shape_" + pi.id;
        }
        pieceArr = pieceList.ToArray();
        strMap = new string[pieceMap.Count];
        foreach (PieceInfo pi in pieceMap.Values) strMap[pi.id] = pi.valstr;
        /***************************************************************/
        //計算touch
        int touchmethod = 1;
        if (touchmethod == 0)
        {
            foreach (PieceInfo pi in pieceMap.Values) pi.instance.SetActive(true);
            for (int i = 0; i < ThinStructure.edgeNum; i++)
            {
                Edge edge = ThinStructure.edges[i];
                Vector3 from = ThinStructure.vertices[edge.idx1];
                Vector3 to = ThinStructure.vertices[edge.idx2];
                Vector3 vec = (to - from).normalized;
                float len = edge.len;
                List<Vector3> poss = new List<Vector3>();
                for (float l = 0; l < len; l += radii) poss.Add(from + vec * l);
                for (float l = len; l > 0; l -= radii) poss.Add(from + vec * l);
                foreach (Vector3 pos in poss)
                {
                    Collider[] cols = Physics.OverlapSphere(pos, radii);
                    foreach (Collider col in cols)
                    {
                        string[] strs = col.gameObject.name.Split('_');
                        if (strs[0] == "shape")
                        {
                            int idx = int.Parse(strs[1]);
                            Touchinfo ti = new Touchinfo();
                            ti.contactedge = i;
                            ti.dir = ThinStructure.splitNorms[i];
                            ti.setisup(pieceMap[strMap[idx]].instanceCent);
                            pieceMap[strMap[idx]].addTouchInfo(ti);
                        }
                    }
                }
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + i + "/" + ThinStructure.edgeNum + ")";
                yield return new WaitForSeconds(0);
            }
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
            foreach (PieceInfo pi in pieceMap.Values) pi.instance.SetActive(false);
        }
        /***************************************************************/
        else if (touchmethod == 1) {
            //計算touch section method
            cnt = 0;
            foreach (PieceInfo pi in pieceMap.Values)
            {
                pi.calSection();
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + (cnt++) + "/" + pieceMap.Values.Count + ")";
                yield return new WaitForSeconds(0);
            }
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
            foreach (PieceInfo pi in pieceMap.Values) pi.instance.SetActive(false);
        }
        /***************************************************************/
        //計算link
        calLink();
        /***************************************************************/
        //初始group
        initGroup();
        yield return 0;
    }


    public void calLink() {
        pieceLink = new int[pieceMap.Count][];
        pieceLinkPlane = new int[pieceMap.Count][];
        pieceLinkTable = new HashSet<int>[pieceLink.Length];
        for (int i = 0; i < pieceLink.Length; i++)
        {
            pieceLink[i] = new int[pieceMap.Count];
            for (int j = 0; j < pieceLink[i].Length; j++)
            {
                pieceLink[i][j] = 0;
            }
            pieceLinkPlane[i] = new int[pieceMap.Count];
            pieceLinkTable[i] = new HashSet<int>();
        }
        foreach (string stra in pieceMap.Keys)
        {
            foreach (string strb in pieceMap.Keys)
            {
                char[] cha = stra.ToCharArray();
                char[] chb = strb.ToCharArray();
                int dif = 0;
                int difidx = 0;
                for (int i = 0; i < cha.Length; i++)
                {
                    if (cha[i] != chb[i])
                    {
                        dif++;
                        difidx = i;
                    }
                }
                if (dif == 1)
                {
                    int ida = getPiece(stra).id;
                    int idb = getPiece(strb).id;
                    pieceLink[ida][idb] = 1;//#接觸面積?
                    pieceLinkPlane[ida][idb] = difidx;
                    pieceLinkTable[ida].Add(idb);
                }
            }
        }
    }
    public void fineCube() {
        for (int n = 2; ; n *= 2) {
            bool ischange = false;
            for (int x = 0; x < nx; x += n)
            {
                for (int y = 0; y < ny; y += n)
                {
                    for (int z = 0; z < nz; z += n)
                    {
                        int c = cubics_xyz[x][y][z];
                        if (cubics[c].instanceScale != n / 2) continue;
                        List<int> list = new List<int>();
                        for (int i = 0; i < 8; i++) {
                            int ix = (i / 4) * n / 2 + x;
                            int iy = (i % 4 / 2) * n / 2 + y;
                            int iz = (i % 2) * n / 2 + z;
                            if (ix < nx && iy < ny && iz < nz) {
                                if (cubics[cubics_xyz[ix][iy][iz]].valstr == cubics[c].valstr
                                 && cubics[cubics_xyz[ix][iy][iz]].instanceScale == cubics[c].instanceScale) {
                                    list.Add(cubics_xyz[ix][iy][iz]);
                                }
                            }
                        }
                        if (list.Count == 8) {
                            Vector3 cent = cubics[c].instanceCent + new Vector3(radii * n / 2, radii * n / 2, radii * n / 2);
                            foreach (int t in list)
                            {
                                cubics[t].id = cubics[c].id;
                                cubics[t].instanceCent = cent;
                                cubics[t].instanceScale = n;
                            }
                            ischange = true;
                        }
                    }
                }
            }
            if (!ischange) break;
        }
    }
    void initSplitNorm() {
        splitNormInfo = new SplitNormInfo[ThinStructure.edgeNum];
        for (int i = 0; i < ThinStructure.splitNorms.Length; i++)
        {
            ThinStructure.splitNorms[i].Normalize();
            splitNormInfo[i] = new SplitNormInfo();
            splitNormInfo[i].mapto = i;
            splitNormInfo[i].norm = ThinStructure.splitNorms[i];
            splitNormInfo[i].cent = ThinStructure.edges[i].cent;
        }
        for (int i = 0; i < ThinStructure.splitNorms.Length; i++) {
            for (int j = i + 1; j < ThinStructure.splitNorms.Length; j++)
            {
                if (!splitNormInfo[i].toDelete && !splitNormInfo[j].toDelete) {
                    float angle = Vector3.Angle(ThinStructure.splitNorms[i], ThinStructure.splitNorms[j]);
                    if (angle < 10 || angle > 170)
                    {
                        Vector3 centi = ThinStructure.edges[i].cent;
                        Vector3 centj = ThinStructure.edges[j].cent;
                        float dis = Vector3.Dot( centj - centi, ThinStructure.splitNorms[i]);
                        if (Mathf.Abs(dis) < 10) {//0.01
                            splitNormInfo[j].toDelete = true;
                            splitNormInfo[j].mapto = i;
                        }
                    }
                }
            }
        }
        List<SplitNormInfo> splitNormUsage_list = new List<SplitNormInfo>();
        int cnt = 0;
        int[] change = new int[ThinStructure.edgeNum];
        for (int i = 0; i < ThinStructure.edgeNum; i++) {
            if (!splitNormInfo[i].toDelete)
            {
                splitNormUsage_list.Add(splitNormInfo[i]);
                change[i] = cnt++;
            } else change[i] = -1;
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++) {
            splitNormInfo[i].mapto = change[splitNormInfo[i].mapto];
        }
        splitNormUsage = splitNormUsage_list.ToArray();
    }

    void calMaxBounding() {
        int plus = 15;
        List<Quaternion> tq = new List<Quaternion>();
        for (int x = 0; x < 360; x += plus)
        {
            for (int y = 0; y < 360; y += plus)
            {
                for (int z = 0; z < 360; z += plus)
                {
                    tq.Add(Quaternion.Euler(x, y, z));
                }
            }
        }
        ThinStructure.store();
        /**/
        calBounding();
        Vector3 dis = max - min;
        float minVol = dis.x * dis.y * dis.z;
        Quaternion minq = Quaternion.identity;
        foreach (Quaternion q in tq) {
            ThinStructure.restore();
            ThinStructure.rotate(q);
            calBounding();
            dis = max - min;
            float vol = dis.x * dis.y * dis.z;
            if (vol < minVol) {
                minVol = vol;
                minq = q;
            }
        }
        /**/
        ThinStructure.restore();
        ThinStructure.rotate(minq);
        calBounding();
    }
    void moveToOrigin() {
        Vector3 ori = (max + min) / 2;
        max -= ori;
        min -= ori;
        for (int i = 0; i < ThinStructure.verticeNum; i++) {
            ThinStructure.vertices[i] -= ori;
        }
    }
    void calBounding() {
        /*************************************************/
        //邊值計算
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            Vector3 v = ThinStructure.vertices[i];
            max.x = Mathf.Max(max.x, v.x);
            max.y = Mathf.Max(max.y, v.y);
            max.z = Mathf.Max(max.z, v.z);
            min.x = Mathf.Min(min.x, v.x);
            min.y = Mathf.Min(min.y, v.y);
            min.z = Mathf.Min(min.z, v.z);
        }
        float tuberadii = ThinStructure.tuberadii;
        max += new Vector3(radii * 3 + tuberadii, radii * 3 + tuberadii, radii * 3 + tuberadii);
        min -= new Vector3(radii + tuberadii + 1, radii + tuberadii + 1 + 1, radii + tuberadii + 1);
    }

    IEnumerator putCubeVolumn() {

        /*************************************************/
        //製作cube
        int cnt = 0;
        int total = 0;
        List<Cubic> cubics_list = new List<Cubic>();
        nx = ny = nz = 0;
        for (float x = min.x; x < max.x; x += radii * 2, nx++) ;
        for (float y = min.y; y < max.y; y += radii * 2, ny++) ;
        for (float z = min.z; z < max.z; z += radii * 2, nz++) ;
        total = nx * ny * nz;
        cubics_xyz = new int[nx][][];
        int ix = 0;
        for (float x = min.x; x < max.x; x += radii * 2, ix++)
        {
            cubics_xyz[ix] = new int[ny][];
            int iy = 0;
            for (float y = min.y; y < max.y; y += radii * 2, iy++)
            {
                cubics_xyz[ix][iy] = new int[nz];
                int iz = 0;
                for (float z = min.z; z < max.z; z += radii * 2, iz++)
                {
                    Cubic cubic = new Cubic(new Vector3(x, y, z), new Vector3(radii, radii, radii), Quaternion.identity);
                    cubic.id = cnt++;
                    cubic.ix = ix; cubic.iy = iy; cubic.iz = iz;
                    cubics_xyz[ix][iy][iz] = cubic.id;
                    cubics_list.Add(cubic);
                    cubic.init();
                }
            }
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + cnt + "/" + total + ")";
            yield return new WaitForSeconds(0);
        }
        cubics = cubics_list.ToArray();
        /***************************************************************/
        //計算代表值
        cnt = 0;
        foreach (Cubic cubic in cubics)
        {
            for (int i = 0; i < ThinStructure.splitNorms.Length; i++)
            {
                Vector3 pos = ThinStructure.edges[i].cent;
                Vector3 norm = ThinStructure.splitNorms[i];
                Vector3 vec = cubic.position - pos;
                if (Vector3.Dot(norm, vec) >= 0) cubic.setVal(i, true);
                else cubic.setVal(i, false);
            }
            cubic.genValstr();

            /*ui*/
            if (cnt % 10000 == 0)
            {
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + cnt + "/" + total + ")";
                yield return new WaitForSeconds(0);
            }
            cnt++;
        }
        //fineCube();//簡化cubeInstance

        /***************************************************************/
        //計算重疊和piece
        cnt = 0;
        foreach (Cubic cubic in cubics)
        {
            Collider[] cols = Physics.OverlapBox(cubic.position, cubic.scale, cubic.rotation);
            cubic.istube = cols.Length > 0;
            foreach (Collider col in cols)
            {
                string[] strs = col.gameObject.name.Split('_');
                if (strs[0] == "Edge")
                {
                    int e = int.Parse(strs[1]);
                    Touchinfo ti = genTouchinfo(cubic, e);
                    cubic.touchinfo.Add(ti);
                }
            }
            PieceInfo pi = addToPieceMap(cubic);
            /*ui*/
            if (cnt % 10000 == 0)
            {
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + cnt + "/" + total + ")";
                yield return new WaitForSeconds(0);
            }
            cnt++;
        }
        /***************************************************************/
        //初始group
        initGroup();
        /***************************************************************/
        //計算pieceLink
        pieceLink = new int[pieceMap.Count][];
        for (int i = 0; i < pieceLink.Length; i++) {
            pieceLink[i] = new int[pieceMap.Count];
            for (int j = 0; j < pieceLink[i].Length; j++) {
                pieceLink[i][j] = 0;
            }
        }
        for (int i = 0; i < cubics.Length; i++) {
            List<int> neiidlist = new List<int>();
            Cubic c = cubics[i];
            if (c.iz > 0) neiidlist.Add(cubics_xyz[c.ix][c.iy][c.iz - 1]);
            if (c.iz < nz - 1) neiidlist.Add(cubics_xyz[c.ix][c.iy][c.iz + 1]);
            if (c.iy > 0) neiidlist.Add(cubics_xyz[c.ix][c.iy - 1][c.iz]);
            if (c.iy < ny - 1) neiidlist.Add(cubics_xyz[c.ix][c.iy + 1][c.iz]);
            if (c.ix > 0) neiidlist.Add(cubics_xyz[c.ix - 1][c.iy][c.iz]);
            if (c.ix < nx - 1) neiidlist.Add(cubics_xyz[c.ix + 1][c.iy][c.iz]);
            int covered = 0;
            foreach (int neiid in neiidlist) {
                if (cubics[i].valstr != cubics[neiid].valstr)
                {
                    PieceInfo pia = getPiece(cubics[i].valstr);
                    PieceInfo pib = getPiece(cubics[neiid].valstr);
                    pieceLink[pia.id][pib.id]++;
                }
                else { covered++; }
            }
            if (covered == 6) cubics[i].iscovered = true;
            /*ui*/
            if (i % 10000 == 0) {
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + i + "/" + cubics.Length + ")";
                yield return new WaitForSeconds(0);
            }
        }
        /***************************************************************/
        calLink();
        /***************************************************************/
        GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
        yield return 0;
    }
    public void calGroupLink() {
        int pgn = pieceGroup.Count;
        groupLink = new int[pgn][];
        groupLinkTable = new HashSet<int>[pgn];
        for (int i=0;i<pgn;i++) {
            groupLink[i] = new int[pgn];
            groupLinkTable[i] = new HashSet<int>();
        }
        /*******************************/
        foreach (PieceInfo pia in pieceMap.Values)
        {
            foreach (PieceInfo pib in pieceMap.Values) {
                if (pieceLink[pia.id][pib.id] > 0 && pieceBelong[pia.id]!=pieceBelong[pib.id])
                {
                    groupLink[pieceBelong[pia.id]][pieceBelong[pib.id]]++;
                    groupLinkTable[pieceBelong[pia.id]].Add(pieceBelong[pib.id]);
                }
            }
        }
    }
    void calLinkQueue() {
        linkQueue = new PriorityQueue<GroupLinkInfo>();
        for (int i = 0; i < pieceGroupArr.Length; i++)
        {
            for (int j = i + 1; j < pieceGroupArr.Length; j++)
            {
                if (groupLink[i][j] > 0)
                {
                    GroupLinkInfo gli = new GroupLinkInfo(pieceGroupArr[i].id, pieceGroupArr[j].id);
                    gli.setWeight();
                    linkQueue.Enqueue(gli);
                }
            }
        }
    }

    public void initGroup() {
        //初始group
        pieceBelong = new int[pieceMap.Count];
        int belongcnt = 0;
        int pmc = pieceMap.Count;
        foreach (string str in pieceMap.Keys)
        {
            PieceGroupInfo pgi = new PieceGroupInfo();
            pgi.initAvaliable();
            pgi.addPiece(str);
            pgi.initInstance();
            pgi.initColor();
            if (pgi.instance != null) {
                pgi.instance.name = "group_" + belongcnt;
            }
            pgi.idx = belongcnt;
            pgi.id = groupIdCnt++;
            pgi.setVolume();
            pieceGroup.Add(pgi);
            GroupMap.Add(pgi.id, pgi);
            pieceBelong[getPiece(str).id] = belongcnt;
            belongcnt++;
        }
        pieceGroupArr = pieceGroup.ToArray();
        calGroupLink();
    }
    public PieceInfo addToPieceMap(Cubic cubic) {
        PieceInfo pi;
        if (!pieceMap.TryGetValue(cubic.valstr, out pi))
        {
            pi = new PieceInfo();
            pi.valstr = cubic.valstr;
            pi.id = pieceMap.Count;
            pieceMap.Add(cubic.valstr, pi);
        }
        pi.cubics.Add(cubic);
        if (cubic.istube) pi.addTouchedCubic(cubic);
        return pi;
    }
    public Touchinfo genTouchinfo(Cubic cubic, int e) {
        Touchinfo touchinfo = new Touchinfo();
        Vector3 pos = ThinStructure.edges[e].cent;
        Vector3 norm = ThinStructure.splitNorms[e];
        Vector3 vec = cubic.position - pos;
        if (Vector3.Dot(norm, vec) >= 0) touchinfo.isup = true;
        else touchinfo.isup = false;
        touchinfo.dir = touchinfo.isup ? norm : -norm;
        touchinfo.contactedge = e;
        return touchinfo;
    }

    PieceGroupInfo keepgroup;
    HashSet<int> keeped = new HashSet<int>();
    HashSet<int> tokeep = new HashSet<int>();
    int[] tokeep_arr;
    int tokeep_cnt;
    public List<GameObject> avaGos;
    public void calNext(bool show)
    {
        if (tokeep.Count == 0)
        {
            targroup++;
            targroup %= pieceGroup.Count;
        }
        else if (targroup > 0)
        {
            tokeep_cnt++;
            tokeep_cnt %= tokeep_arr.Length;
            targroup = tokeep_arr[tokeep_cnt];
        }
        if(show)showHalf();
    }
    public void showAva() {
        foreach (GameObject go in avaGos) Destroy(go);
        avaGos = new List<GameObject>();
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowAva").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (active) {
            Vector3 pos = getGroupCenter(pieceGroupArr[targroup].idx);
            List<Vector3> ava = pieceGroup[targroup].getCurLock();
            ava = pieceGroup[targroup].avaliableDirs;
            Vector3 sum = Vector3.zero;
            foreach (Vector3 vec in ava)
            {
                GameObject go = Tool.DrawLine(pos, pos + vec.normalized * 100, radii / 2, Color.green);
                go.transform.parent = Assist;
                avaGos.Add(go);
                sum += vec;
            }
            //GameObject go = Tool.DrawLine(pos, pos + sum.normalized * 100, radii / 2, Color.green);
            //go.transform.parent = Assist;
            //avaGos.Add(go);
        }
    }
    public Vector3 getGroupCenter(int idx) {
        Vector3 cent = Vector3.zero;
        int num = 0;
        foreach (string str in pieceGroupArr[idx].pieces) {
            cent += pieceMap[str].instanceCent;
            num++;
        }
        cent /= num;
        return cent;
    }
    public void calNext()
    {
        calNext(true);
        showAva();
    }
    public void setKeepColor(Color color) {
        foreach (string tarvalstr in keepgroup.pieces)
        {
            foreach (Cubic cubic in getPiece(tarvalstr).cubics)
            {
                Tool.setColor(cubic.Instance, color);
            }
        }
    }
    public void autoAll() {
        StartCoroutine(iautoAll2());
    }
    IEnumerator iautoAll() {
        calGroupLink();
        int num = int.MaxValue;
        while (num > pieceGroup.Count) {
            num = pieceGroup.Count;
            targroup = 0;
            while (targroup < pieceGroup.Count - 1)
            {
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + targroup + "/" + pieceGroup.Count + ")";
                yield return new WaitForSeconds(0);
                /*****/
                autoMerge(false);
                calNext(false);
            }
        }
        for (int i = 0; i < pieceGroupArr.Length; i++)
        {
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + i + " / " + pieceGroupArr.Length + ")";
            yield return new WaitForSeconds(0);
            pieceGroupArr[i].initInstance();
        }
        GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
        showHalf();
        showAll();
        showTopo();
        yield return 0;
    }
    IEnumerator iautoAll2()
    {
        //*********************************************
        for (int i = 0; i < pieceGroupArr.Length; i++)
        {
            foreach (string str in pieceGroupArr[i].pieces)
            {
                foreach (Touchinfo ti in pieceMap[str].touchinfo)
                {
                    if (ti.cavityinstance != null) ti.cavityinstance.transform.parent = Assist;
                }
            }
            pieceGroupArr[i].initInstance();
        }
        //*********************************************
        calGroupLink();
        calLinkQueue();
        int cnt = 0;
        while (linkQueue.Peek().weight > 0) {//不可能抓不到
            //if ((cnt++) > 5) break;
            //int ln = linkQueue.Count();
            GroupLinkInfo gli = linkQueue.Dequeue();
            PieceGroupInfo pgia, pgib;
            if (!GroupMap.TryGetValue(gli.ida, out pgia) || !GroupMap.TryGetValue(gli.idb, out pgib))continue;
            int idxa = pgia.idx;
            int idxb = pgib.idx;
            keeped.Add(idxa);
            keeped.Add(idxb);
            mergeGroup(false);
            
            PriorityQueue<GroupLinkInfo> linkQueue_new = new PriorityQueue<GroupLinkInfo>();
            foreach (GroupLinkInfo glii in linkQueue) {
                PieceGroupInfo pgiia, pgiib;
                if (!GroupMap.TryGetValue(glii.ida, out pgiia) || !GroupMap.TryGetValue(glii.idb, out pgiib)) continue;
                linkQueue_new.Enqueue(glii);
            }
            linkQueue = linkQueue_new;
            
            //print(linkQueue.Count());
            //calLinkQueue();
            //print(linkQueue.Count());
            //print("--");

            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + pieceGroup.Count + ")";
            yield return new WaitForSeconds(0);
        }
        for(int i=0;i<pieceGroupArr.Length;i++) {
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + i + " / " + pieceGroupArr.Length + ")";
            yield return new WaitForSeconds(0);
            pieceGroupArr[i].initInstance();
        }
        showAll();
        
        GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
        yield return 0;
    }
    public void autoMerge(bool show) {
        keepGroup(false);
        while (autoChooseNext() > 0) {
            keepGroup(false);
        }
        mergeGroup(show);
    }
    int autoChooseNext() {
        int maxworth = int.MinValue;
        int maxtar = -1;
        foreach (int tar in tokeep)
        {
            int worth = keepgroup.calGrowWorth(tar);
            if (worth > maxworth)
            {
                maxworth = worth;
                maxtar = tar;
            }
        }
        targroup = maxtar;
        PieceGroupInfo pgi = pieceGroup[targroup];
        foreach (string str in pgi.pieces) {
            /*
            foreach (Touchinfo ti in pieceMap[str].touchinfo) {
                print(targroup + " " + ti.contactedge + " " + pieceMap[str].touchnum[ti.contactedge]);
            }
            print(maxworth);
            */
        }
        return maxworth;
        /*
        Vector3 mean = Vector3.zero;
        foreach (int tar in keeped) {
            mean += pieceGroup[tar].calMeanDir();
        }
        mean.Normalize();
        float minangle = float.MaxValue;
        int mintar = -1;
        foreach (int tar in tokeep)
        {
            Vector3 tomean = pieceGroup[tar].calMeanDir();
            float angle = Vector3.Angle(tomean, mean);
            if (angle < minangle) {
                minangle = angle;
                mintar = tar;
            }
        }
        targroup = mintar;
        */
    }
    public void keepGroup(bool show) {
        if (targroup < 0) return;
        if (keeped.Count == 0) {
            keepgroup = new PieceGroupInfo();
            keepgroup.initAvaliable();
        }
        keeped.Add(targroup);
        foreach (string tarvalstr in pieceGroup[targroup].pieces)
        {
            keepgroup.addPiece(tarvalstr);
            if (show) {
                foreach (Cubic cubic in getPiece(tarvalstr).cubics)
                {
                    Tool.setColor(cubic.Instance, Color.blue);
                }
            }
        }
        if (show) Tool.setColor(pieceGroup[targroup].instance, Color.blue);
        /**********************/
        foreach (string valstr in pieceGroup[targroup].pieces) {
            PieceInfo pia = getPiece(valstr);
            foreach (PieceInfo pib in pieceMap.Values)
            {
                if (pieceLink[pia.id][pib.id] > 0)
                {
                    tokeep.Add(pieceBelong[pib.id]);
                }
            }
        }
        /**********************/
        //string s = ""; foreach (int g in tokeep) s += " " + g; print(s);
        foreach (int t in keeped) {
            tokeep.Remove(t);
        }
        /**********************/
        tokeep_arr = new int[tokeep.Count];
        int cnt = 0; foreach (int t in tokeep) tokeep_arr[cnt++] = t;
        tokeep_cnt = 0;
        if (tokeep_arr.Length == 0) targroup = -1;
        else targroup = tokeep_arr[tokeep_cnt];
        /**********************/
        if (show) showHalf();
    }
    public void mergeGroup(bool show) {
        List<int> keeped_list = new List<int>();
        foreach (int idx in keeped) keeped_list.Add(idx);
        keeped_list.Sort();
        int[] keeped_array = new int[keeped_list.Count];
        int cnt = 0;
        int firstgroup = -1;
        foreach (int idx in keeped_list) {
            if (firstgroup == -1) firstgroup = idx;
            keeped_array[cnt++] = idx;
        }
        List<int> keeped_reverselist = new List<int>();
        for (int i = keeped_array.Length - 1; i >= 0; i--) keeped_reverselist.Add(keeped_array[i]);
        /****************************************/
        int[] minus = new int[pieceGroup.Count];
        HashSet<int> involeId = new HashSet<int>();
        List<GameObject> addros = new List<GameObject>();
        foreach (int idx in keeped_reverselist) {//idx遞減
            foreach (int nei in groupLinkTable[idx])
            {
                involeId.Add(pieceGroupArr[nei].id);
            }
            if (firstgroup == idx) break;
            HashSet<string> hs = pieceGroupArr[idx].pieces;
            foreach (string str in hs) {
                pieceGroupArr[firstgroup].addPiece(str);
                PieceInfo pi = getPiece(str);
                pieceBelong[pi.id] = firstgroup;
            }
            addros.Add(pieceGroupArr[idx].instance);
            //pieceGroupArr[firstgroup].addInstance(pieceGroupArr[idx].instance);

            Tool.deleteObj(pieceGroupArr[idx].instance);
            GroupMap.Remove(pieceGroupArr[idx].id);
            pieceGroup.RemoveAt(idx);
            minus[idx]++;
        }
        //pieceGroupArr[firstgroup].addInstance(addros.ToArray());
        /**/
        for (int i = 0, cal = 0; i < minus.Length; i++) {
            cal += minus[i];
            minus[i] = cal;
        }
        for (int i = 0; i < pieceBelong.Length; i++) {
            pieceBelong[i] -= minus[pieceBelong[i]];
        }
        
        foreach (PieceGroupInfo pgi in pieceGroup) {
            pgi.idx -= minus[pgi.idx];
        }
        /**/
        GroupMap.Remove(pieceGroupArr[firstgroup].id);
        int ida = pieceGroupArr[firstgroup].id = groupIdCnt++;
        pieceGroupArr[firstgroup].instance.name = "group_" + ida;
        pieceGroupArr[firstgroup].setVolume();
        pieceGroup[firstgroup] = pieceGroup[firstgroup];//call by ref?
        GroupMap.Add(ida, pieceGroupArr[firstgroup]);
        pieceGroupArr = pieceGroup.ToArray();
        /**/
        foreach (int idb in involeId)
        {
            if (!GroupMap.ContainsKey(idb)) continue;
            GroupLinkInfo gli = new GroupLinkInfo(ida, idb);
            gli.setWeight();
            linkQueue.Enqueue(gli);
        }
        calGroupLink();
        /****************************************/
        keeped = new HashSet<int>();
        tokeep = new HashSet<int>();
        targroup = firstgroup;
        if (show) {
            if (cubics != null)
            {
                foreach (Cubic cubic in cubics)
                {
                    if (cubic.Instance)
                    {
                        Tool.setColor(cubic.Instance, Color.grey);
                    }
                }
            }
            foreach (PieceGroupInfo pgi in pieceGroup)
            {
                if (pgi.instance != null)
                {
                    Tool.setColor(pgi.instance, Color.grey);
                }
            }
            showHalf();
        }
        /****************************************/
    }

    public void showAni() {
        foreach (PieceGroupInfo pgi in pieceGroup) {
            if (pgi.targetPos == Vector3.zero)
            {
                Vector3 sum = Vector3.zero;
                foreach (Vector3 vec in pgi.avaliableDirs)
                {
                    sum += vec.normalized;    
                }
                pgi.targetPos = sum.normalized * 100;
                //if (pgi.targetPos == Vector3.zero) pgi.targetPos = getGroupCenter(pgi.idx);
                //pgi.targetPos = getGroupCenter(pgi.idx);
            }
            else {
                pgi.targetPos = Vector3.zero;
            }
        }
    }
    public void showAni2()
    {
        foreach (PieceGroupInfo pgi in pieceGroup)
        {
            if (pgi.avaliableDirs.Count == 0) {
                if (pgi.targetPos == Vector3.zero)
                {
                    pgi.targetPos = getGroupCenter(pgi.idx);
                }
                else
                {
                    pgi.targetPos = Vector3.zero;
                }
            }
        }
    }
    public static void getPiece(string str, out PieceInfo pi)
    {
        pieceMap.TryGetValue(str, out pi);
    }
    public static PieceInfo getPiece(string str) {
        PieceInfo pi;
        pieceMap.TryGetValue(str, out pi);
        return pi;
    }
    public static GameObject putCube(Vector3 pos, float radii, int id) {
        GameObject go = Tool.DrawCubeMesh(pos + new Vector3(radii, radii, radii), pos - new Vector3(radii, radii, radii), Color.grey);
        go.transform.parent = GameObject.Find("Collect").transform;
        go.name = "cube_" + id;
        return go;
    }
    public static GameObject putCube(Vector3 pos, Vector3 scale, int id)
    {
        GameObject go = Tool.DrawCubeMesh(pos + scale, pos - scale, Color.grey);
        go.transform.parent = GameObject.Find("Collect").transform;
        go.name = "cube_" + id;
        return go;
    }
    public void showPlane()
    {
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowPlane").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (GameObject plane in ThinStructure.planeGOs)
        {
            plane.SetActive(active);
        }
    }
    public void showTube() {
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowTube").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (Cubic cubic in cubics)
        {
            if (cubic.istube) cubic.SetActive(active);
        }
    }
    public int targroup = 0;
    public void showHalf()
    {
        /**/
        bool activeAll = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowAll").GetComponent<UnityEngine.UI.Toggle>().isOn;
        if (cubics != null) {
            foreach (Cubic cubic in cubics)
            {
                cubic.SetActive(false);
            }
        }

        foreach (PieceGroupInfo pgi in pieceGroup) {
            if (pgi.instance != null) {
                if (!activeAll)
                {
                    pgi.instance.SetActive(false);
                }
                else
                {
                    Tool.setColor(pgi.instance, pgi.color);
                }
            }
        }
        
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowHalf").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (int k in keeped) {
            showGroup(k, active);
        }
        if (targroup >= 0) showGroup(targroup, active, Color.white);
        /*
        foreach (string str in pieceGroup[targroup])
        {
            PieceInfo pia = getPiece(str);
            foreach (PieceInfo pib in pieceMap.Values)
            {
                if (pieceLink[pia.id][pib.id]>0)
                {
                    foreach (Cubic cubic in pib.cubics)
                    {
                        cubic.SetActive(active);
                        if (active) Tool.setColor(cubic.Instance, Color.black);
                    }
                }
            }
        }
        */
    }
    public void showCurve() {
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowCurve").GetComponent<UnityEngine.UI.Toggle>().isOn;
        for (int i = 0; i < ThinStructure.verticeNum; i++)
        {
            ThinStructure.verticeGOs[i].SetActive(active);
        }
        for (int i = 0; i < ThinStructure.edgeNum; i++) {
            ThinStructure.edgeGOs[i].SetActive(active);
        }
    }
    List<GameObject> topoGos = new List<GameObject>();
    public void showTopo() {
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowTopo").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (GameObject go in topoGos)
        {
            Tool.deleteObj(go);
            Destroy(go);
        }
        topoGos = new List<GameObject>();
        if (active)
        {
            calGroupLink();
            Transform Assist = GameObject.Find("Assist").transform;
            foreach (PieceGroupInfo pgi in pieceGroup) {
                GameObject go = Tool.DrawBall(getGroupCenter(pgi.idx), radii *2, pgi.color);
                topoGos.Add(go);
                go.transform.parent = Assist;
                Tool.getMesh(pgi.instance).RecalculateBounds();
            }
            int i=0;
            foreach (PieceGroupInfo pgia in pieceGroup) {
                int j = 0;
                foreach (PieceGroupInfo pgib in pieceGroup)
                {
                    if (j > i &&groupLink[i][j]>0) {
                        GameObject go = Tool.DrawLine(getGroupCenter(pgia.idx), getGroupCenter(pgib.idx), radii / 2, Color.gray);
                        topoGos.Add(go);
                        go.transform.parent = Assist;
                    }
                    j++;
                }
                i++;
            }
        }
    }

    public void showAll()
    {
        /**/
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowAll").GetComponent<UnityEngine.UI.Toggle>().isOn;
        int pn = pieceGroup.Count;
        for (int i = 0; i < pn; i++)
        {
            showGroup(i, active, active ? pieceGroup[i].color : Color.white);
        }
        
    }

    public void showPart(int state)
    {
        if (state == 0) {
            bool active = true;
            int pn = pieceGroup.Count;
            for (int i = 0; i < pn; i++)
            {
                if(pieceGroup[i].avaliableDirs.Count>0)showGroup(i, active, active ? pieceGroup[i].color : Color.white);
                else showGroup(i, false);
            }
        }
        if (state == 1)
        {
            bool active = true;
            int pn = pieceGroup.Count;
            for (int i = 0; i < pn; i++)
            {
                if (pieceGroup[i].avaliableDirs.Count == 0) showGroup(i, active, active ? pieceGroup[i].color : Color.white);
                else showGroup(i, false);
            }
        }

    }

    void showGroup(int idx, bool active, Color color)
    {
        foreach (string tarvalstr in pieceGroup[idx].pieces)
        {
            PieceInfo pi = getPiece(tarvalstr);
            foreach (Cubic cubic in pi.cubics)
            {
                if (cubic.touchinfo.Count > 0) {
                    cubic.SetActive(active);
                    if (active) Tool.setColor(cubic.Instance, color);
                }
            }
        }
        if (pieceGroup[idx].instance != null)
        {
            pieceGroup[idx].instance.SetActive(active);
            Tool.setColor(pieceGroup[idx].instance, color);
        }
        showCavity(idx, active);
    }
    void showGroup(int idx, bool active)
    {
        foreach (string tarvalstr in pieceGroup[idx].pieces)
        {
            PieceInfo pi = getPiece(tarvalstr);
            foreach (Cubic cubic in pi.cubics)
            {
                cubic.SetActive(active);
            }
        }
        if (pieceGroup[idx].instance != null)
        {
            pieceGroup[idx].instance.SetActive(active);
        }
        showCavity(idx, active);
    }
    void showCavity(int idx, bool active) {
        bool active2 = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowCavity").GetComponent<UnityEngine.UI.Toggle>().isOn;
        active = active2 && active;
        PieceGroupInfo pgi = pieceGroup[idx];
        foreach (string str in pgi.pieces) {
            foreach (Touchinfo ti in pieceMap[str].touchinfo) {
                if (ti.cavityinstance != null)
                {
                    ti.cavityinstance.SetActive(active);
                    if (active)
                    {
                        ti.cavityinstance.transform.parent = pgi.instance.transform;
                    }
                }
            }
        }
    }

    public void outputMesh() {///bad!!!
        string poolPath = @"CSGCommandLineTool\pool\";
        /*
        for (int i=0;i<pieceGroupArr.Length;i++) {
            PieceGroupInfo pgi = pieceGroupArr[i];
            using (StreamWriter sw = new StreamWriter(poolPath + "shape_" + i + ".obj"))
            {
                sw.Write(MeshComponent.MeshToString(pgi.instance));
                sw.Close();
            }
        }
        */
        Generator generator = GameObject.Find("DataManager").GetComponent<Generator>();
        Generator.iR = radii / Generator.mul;
        Generator.tarSet = tarSet;
        generator.genShape();
    }
    public void inputMesh()
    {
        GameObject Collect = GameObject.Find("Collect");
        for (int i = 0; i < Collect.transform.childCount; i++) {
            if (Collect.transform.GetChild(i).name.Split('_')[0] == "Comp") {
                Destroy(Collect.transform.GetChild(i).gameObject);
            }
        }
        string poolPath = "inputSet\\" + tarSet + "\\cubeObj\\";
        for (int i = 0; i < pieceGroupArr.Length; i++)
        {
            PieceGroupInfo pgi = pieceGroupArr[i];
            Mesh holderMesh = new Mesh();
            ObjImporter newMesh = new ObjImporter();
            holderMesh = newMesh.ImportFile(poolPath + "output_" + i + ".obj");
            GameObject obj = pgi.instance;
            if (obj.GetComponent<MeshRenderer>() == null) obj.AddComponent<MeshRenderer>();
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (obj.GetComponent<MeshFilter>() == null) obj.AddComponent<MeshFilter>();
            MeshFilter filter = obj.GetComponent<MeshFilter>();
            holderMesh.RecalculateNormals();
            filter.mesh = holderMesh;
            obj.transform.localScale = Vector3.one;
        }
    }
    /***************************/
    // Use this for initialization
    void Start () {
		
	}
    private void Update()
    {
        bool bb = Input.GetKey(KeyCode.B);
        if (Input.GetMouseButtonDown(0)&& bb)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                string[] strs = hit.collider.name.Split('_');
                if (strs[0] == "group") {
                    int thisid = int.Parse(strs[1]);
                    PieceGroupInfo pgi = GroupMap[thisid];
                    targroup = pgi.idx;
                    Tool.setColor(pgi.instance, Color.black);
                }
            }
        }
    }
    float timediv = 10;
    float dismul = 30;
    float disthre = 0.5f;
	// Update is called once per frame
	void FixedUpdate () {
        foreach (PieceGroupInfo pgi in pieceGroup)
        {
            if (pgi.instance == null) continue;
            Vector3 curpos = pgi.instance.transform.position;
            Vector3 vec = (pgi.targetPos - curpos) / timediv;
            if ((pgi.instance.transform.position - pgi.targetPos).magnitude < disthre) pgi.instance.transform.position = pgi.targetPos;
            else {
                if (vec.magnitude < disthre) vec = vec.normalized * disthre;
                pgi.instance.transform.position += vec;
            }
        }
    }

    
}
public class PieceInfo{
    public int id=-1;
    public string valstr;
    public List<Cubic> cubics = new List<Cubic>();
    public List<Cubic> cubics_touched = new List<Cubic>();
    public List<Touchinfo> touchinfo = new List<Touchinfo>();
    public GameObject instance = null;
    public Vector3 instanceCent;
    public float volume;
    HashSet<int> touchedge = new HashSet<int>();
    public int[] touchnum = new int[ThinStructure.splitNorms.Length];
    public void setCenter() {
        Mesh mesh = Tool.getMesh(instance);
        instanceCent = Vector3.zero;
        int num = 0;
        for (int i = 0; i < mesh.vertices.Length; i++) {
            instanceCent += mesh.vertices[i];
            num++;
        }
        instanceCent /= num;
        Vector3 s = instance.transform.localScale;
        instanceCent = new Vector3(instanceCent.x * s.x, instanceCent.y * s.y, instanceCent.z * s.z);
        //instanceCent = Tool.getCenter(instance);
    }
    public void addTouchedCubic(Cubic cubic) {
        cubics_touched.Add(cubic);
        foreach (Touchinfo cubti in cubic.touchinfo) {
            if (!touchedge.Contains(cubti.contactedge)) {
                touchinfo.Add(cubti);
                touchedge.Add(cubti.contactedge);
            }
            touchnum[cubti.contactedge]++;
        }
    }
    public void addTouchInfo(Touchinfo ti)
    {
        if (!touchedge.Contains(ti.contactedge))
        {
            touchinfo.Add(ti);
            touchedge.Add(ti.contactedge);
        }
        touchnum[ti.contactedge]++;
    }

    public void calSection() {
        foreach (string str in CubeVolumn.pieceMap.Keys)
        {
            char[] chs = str.ToCharArray();
            for(int j=0;j<ThinStructure.edgeNum;j++)
            {
                Edge edge = ThinStructure.edges[j];
                bool disapear = false;
                Vector3 va = ThinStructure.vertices[edge.idx1];
                Vector3 vb = ThinStructure.vertices[edge.idx2];
                for (int i = 0; i < CubeVolumn.splitNormUsage.Length; i++)
                {
                    SplitNormInfo sni = CubeVolumn.splitNormUsage[i];
                    Vector3 veca = (va - sni.cent);
                    Vector3 vecb = (vb - sni.cent);
                    float da = Vector3.Dot(veca.normalized, sni.norm);
                    float db = Vector3.Dot(vecb.normalized, sni.norm);
                    if (Mathf.Abs(da) < 0.01 && Mathf.Abs(db) < 0.01) continue;
                    else if (Mathf.Abs(da) < 0.01 && db > 0 && chs[i] == '-') disapear = true;
                    else if (Mathf.Abs(da) < 0.01 && db < 0 && chs[i] == '*') disapear = true;
                    else if (Mathf.Abs(db) < 0.01 && da > 0 && chs[i] == '-') disapear = true;
                    else if (Mathf.Abs(db) < 0.01 && da < 0 && chs[i] == '*') disapear = true;
                    else if (da * db < 0)
                    {
                        Vector3 c = Vector3.Dot((vb - va).normalized, (sni.cent - va)) * (vb - va).normalized + va;
                        if (da < 0 && chs[i] == '*') va = c;
                        if (da < 0 && chs[i] == '-') vb = c;
                        if (da > 0 && chs[i] == '*') vb = c;
                        if (da > 0 && chs[i] == '-') va = c;
                    }
                    else if ((da > 0 && db > 0 && chs[i] == '-') || (da < 0 && db < 0 && chs[i] == '*')) disapear = true;
                    if (disapear) break;
                }
                if (!disapear) {
                    Touchinfo ti = new Touchinfo();
                    ti.contactedge = j;
                    ti.dir = ThinStructure.splitNorms[j];
                    ti.setisup(CubeVolumn.pieceMap[str].instanceCent);
                    ti.va = va;
                    ti.vb = vb;

                    GameObject go = Tool.DrawLine(ti.va, ti.vb, CubeVolumn.radii, Color.black);
                    go.transform.parent = CubeVolumn.Assist;
                    go.name = "cavity";
                    go.SetActive(false);
                    ti.cavityinstance = go;

                    CubeVolumn.pieceMap[str].addTouchInfo(ti);
                    CubeVolumn.pieceMap[str].touchnum[j] = 11;//force greater

                    
                }
            }

        }
    }

    public void calVolume() {
        Mesh mesh = instance.GetComponent<MeshFilter>().mesh;
        Vector3 s = instance.transform.localScale;
        volume = 0;
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
        for (int i = 0; i < mesh.triangles.Length; i += 3) {
            volume += SignedVolumeOfTriangle(mesh.vertices[mesh.triangles[i]] * s.x, mesh.vertices[mesh.triangles[i + 1]] * s.y, mesh.vertices[mesh.triangles[i + 2]] * s.z);
        }
        volume = Mathf.Abs(volume);
    }
    public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var v321 = p3.x * p2.y * p1.z;
        var v231 = p2.x * p3.y * p1.z;
        var v312 = p3.x * p1.y * p2.z;
        var v132 = p1.x * p3.y * p2.z;
        var v213 = p2.x * p1.y * p3.z;
        var v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }
}
public class PieceGroupInfo{
    public HashSet<string> pieces = new HashSet<string>();
    public List<Vector3> avaliableDirs = new List<Vector3>();
    public List<Vector3> limit = new List<Vector3>();
    public GameObject instance;
    public Color color;
    public Vector3 targetPos;
    public int idx = -1;
    public int id = -1;
    public float volume;

    public void setVolume() {
        if(instance!=null)volume = Tool.calVolume(instance);
    }
    //15degree and acos0.1
    public void initColor() {
        color = Tool.RandomColor();
    }
    public bool ignore(ref int[] touchnum, Touchinfo ti)
    {
        return touchnum[ti.contactedge] < 10;
    }
    /*
    public bool smallVolumn(PieceGroupInfo pgi) {
        return pgi.volume < 10000;
    }
    */
    public void addPiece(string str) {
        pieces.Add(str);
        PieceInfo pi = CubeVolumn.getPiece(str);
        foreach (Touchinfo ti in pi.touchinfo) {
            if (ignore(ref pi.touchnum, ti)) continue;
            Vector3 norm = ti.dir;
            List<Vector3> avaliableDirs_next = new List<Vector3>();
            foreach (Vector3 vec in avaliableDirs) {
                if (checkaValiable(vec, norm)) {
                    avaliableDirs_next.Add(vec);
                }
                limit.Add(vec);
            }
            avaliableDirs = new List<Vector3>(avaliableDirs_next.ToArray());
        }
    }
    public List<Vector3> getCurLock() {
        List<Vector3> avaliableDirs_ = new List<Vector3>(avaliableDirs.ToArray());
        foreach (string str in pieces) {
            PieceInfo pi = CubeVolumn.getPiece(str);
            int pia = pi.id;
            List<Vector3> avaliableDirs_next = new List<Vector3>();
            foreach (int pib in CubeVolumn.pieceLinkTable[pia]) {
                if (CubeVolumn.pieceBelong[pib] == idx) continue;
                int planeidx = CubeVolumn.pieceLinkPlane[pia][pib];
                Vector3 dir = CubeVolumn.splitNormUsage[planeidx].norm;
                Vector3 cent = CubeVolumn.splitNormUsage[planeidx].cent;
                Vector3 pos = pi.instanceCent;
                Vector3 vec = pos - cent;
                if (Vector3.Dot(dir, vec) < 0) dir *= -1;
                avaliableDirs_ = purnAvaliable(avaliableDirs_, dir, 1);
            }
        }
        return avaliableDirs_;
    }
    public List<Vector3> purnAvaliable(List<Vector3> orivecs, Vector3 norm, int state) {
        List<Vector3> newvecs = new List<Vector3>();
        foreach (Vector3 vec in orivecs) {
            if (checkaValiable(vec, norm, state))
            {
                newvecs.Add(vec);
            }
        }
        return newvecs;
    }
    public void initInstance() {
        bool first = false;
        if (instance != null) GameObject.Destroy(instance);
        List<GameObject> addors = new List<GameObject>();
        bool second = false;
        foreach (string str in pieces) {
            GameObject src = CubeVolumn.getPiece(str).instance;
            if (!first)
            {
                if (src != null) {
                    instance = GameObject.Instantiate(src, src.transform.position, src.transform.rotation, src.transform.parent);
                    instance.transform.localScale = src.transform.localScale;
                    first = true;
                }
            }
            else {
                //addInstance(CubeVolumn.getPiece(str).instance);
                addors.Add(CubeVolumn.getPiece(str).instance);
                second = true;
            }
        }
        if(second)addInstance(addors.ToArray());
    }
    public void recalBound() {
        instance.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }
    public void addInstance(GameObject addor) {
        if (instance == null) return;
        instance = MeshComponent.AppendMesh(instance, addor);
        /*
        for (int i = 0; i < addor.transform.childCount; i++) {
            addor.transform.GetChild(i).parent = instance.transform;
        }
        */
    }
    public void addInstance(GameObject[] addors)
    {
        if (instance == null) return;
        instance = MeshComponent.AppendMesh(instance, addors);
        /*
        foreach (GameObject addor in addors) {
            for (int i = 0; i < addor.transform.childCount; i++)
            {
                addor.transform.GetChild(i).parent = instance.transform;
            }
        }
        */
    }
    public void initAvaliable() {
        int plus = 15;
        for (int x = 0; x < 360; x += plus)
        {
            for (int y = 0; y < 360; y += plus)
            {
                for (int z = 0; z < 360; z += plus)
                {
                    avaliableDirs.Add(Quaternion.Euler(x, y, z) * (new Vector3(1, 0, 0)));
                }
            }
        }
    }
    bool checkaValiable(Vector3 vec, Vector3 norm) {
        return checkaValiable(vec, norm, 0);
    }
    bool checkaValiable(Vector3 vec, Vector3 norm, int state)
    {
        vec.Normalize();
        norm.Normalize();
        if (state == 0) return Vector3.Dot(vec, norm) > 0.1f;
        if (state == 1) return Vector3.Dot(vec, norm) > -0.1f;
        return false;
    }
    public Vector3 calMeanDir() {
        Vector3 mean = Vector3.zero;
        foreach (string str in pieces)
        {
            PieceInfo pi = CubeVolumn.getPiece(str);
            foreach (Touchinfo ti in pi.touchinfo)
            {
                mean += ti.dir;
            }
        }
        return mean;
    }
    public int calGrowWorth(int g)
    {
        PieceGroupInfo pgi = CubeVolumn.pieceGroup[g];
        //if (smallVolumn(pgi)) return avaliableDirs.Count;
        /*
        PieceGroupInfo pgi_copy = pgi;
        pgi_copy.initAvaliable();
        foreach (string str in pieces)
        {
            pgi_copy.addPiece(str);
        }
        foreach (string str in pgi.pieces)
        {
            pgi_copy.addPiece(str);
        }
        return pgi_copy.avaliableDirs.Count;
        /*/
        List<Vector3> avaliableDirs_copy = new List<Vector3>(avaliableDirs.ToArray());
        List<Vector3> avaliableDirs_next = new List<Vector3>();
        foreach (string str in pgi.pieces) {
            PieceInfo pi = CubeVolumn.getPiece(str);
            foreach (Touchinfo ti in pi.touchinfo)
            {
                if (ignore(ref pi.touchnum, ti)) continue;
                Vector3 norm = ti.dir;
                foreach (Vector3 vec in avaliableDirs_copy)
                {
                    if (checkaValiable(vec, norm))
                    {
                        avaliableDirs_next.Add(vec);
                    }
                }
                avaliableDirs_copy = new List<Vector3>(avaliableDirs_next.ToArray());
                avaliableDirs_next = new List<Vector3>();
            }
        }
        return avaliableDirs_copy.Count;
    }
}
public class GroupLinkInfo : System.IComparable<GroupLinkInfo>
{
    public GroupLinkInfo(int ida, int idb)
    {
        if (idb < ida)
        {
            int t = idb;
            idb = ida;
            ida = t;
        }
        this.ida = ida;
        this.idb = idb;
    }
    public GroupLinkInfo(int ida, int idb, int weight)
    {
        if (idb < ida)
        {
            int t = idb;
            idb = ida;
            ida = t;
        }
        this.ida = ida;
        this.idb = idb;
        this.weight = weight;
    }
    public int ida, idb;
    public int weight;
    public float weight2;
    public void setWeight() {//為負數?
        int avaa = CubeVolumn.GroupMap[ida].avaliableDirs.Count + CubeVolumn.GroupMap[idb].avaliableDirs.Count;
        int avab = CubeVolumn.GroupMap[ida].calGrowWorth(CubeVolumn.GroupMap[idb].idx) * 2;
        float vola = CubeVolumn.GroupMap[ida].volume;
        float volb = CubeVolumn.GroupMap[idb].volume;
        weight = avab - avaa;
        //
        weight = avab;
        weight2 = Mathf.Min(vola, volb);
    }
    public int CompareTo(GroupLinkInfo obj)
    {
        if(((GroupLinkInfo)obj).weight - weight!=0)return ((GroupLinkInfo)obj).weight - weight;
        return (int)(((GroupLinkInfo)obj).weight2 - weight2)*(1);
    }
}

//Map, List, Arr