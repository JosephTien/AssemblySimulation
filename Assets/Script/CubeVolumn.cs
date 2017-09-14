using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitNormInfo
{
    public Vector3 norm;
    public Vector3 cent;
    public int oriid;
    public int mapto;
    public bool toDelete;
    
}
public class CubeVolumn : MonoBehaviour {
    bool novoxel = true;
    public static int tarSet;
    Cubic[] cubics;
    int[][][] cubics_xyz;
    public static Vector3 max;
    public static Vector3 min;
    float radii;
    int nx, ny, nz;
    public static SplitNormInfo[] splitNormInfo;
    public static SplitNormInfo[] splitNormUsage;
    public static SortedDictionary<int, GameObject> hasid = new SortedDictionary<int, GameObject>();

    public static SortedDictionary<string, PieceInfo> pieceMap = new SortedDictionary<string, PieceInfo>();
    
    public static List<PieceGroupInfo> pieceGroup = new List<PieceGroupInfo>();
    int[] pieceBelong;
    int[][] pieceLink;
    public void start()
    {
        Tool.clearObj();
        StartCoroutine(Tool.clearTodelete());
        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "";
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
        ThinStructure.basicPut();
        initSplitNorm();
        StartCoroutine(putCubeVolumn());
        //calSet();
    }
    public void fineCube() {
        for(int n = 2; ; n *= 2) {
            bool ischange = false;
            for (int x = 0; x < nx; x+=n)
            {
                for (int y = 0; y < ny; y+=n)
                {
                    for (int z = 0; z < nz; z+=n)
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
                                 && cubics[cubics_xyz[ix][iy][iz]].instanceScale == cubics[c].instanceScale ) {
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
                    if (Mathf.Abs(Vector3.Dot(ThinStructure.splitNorms[i], ThinStructure.splitNorms[j])) > 0.9)
                    {
                        Vector3 centi = ThinStructure.edges[i].cent;
                        Vector3 centj = ThinStructure.edges[j].cent;
                        if (Mathf.Abs(Vector3.Dot((centj - centi).normalized, ThinStructure.splitNorms[i])) < 0.01) {
                            splitNormInfo[j].toDelete = true;
                            splitNormInfo[j].mapto = i;
                        }
                    }
                }
            }
        }
        List<SplitNormInfo> splitNormUsage_list = new List<SplitNormInfo>();
        int cnt=0;
        int[] change = new int[ThinStructure.edgeNum];
        for (int i = 0; i < ThinStructure.edgeNum; i++) {
            if (!splitNormInfo[i].toDelete)
            {
                splitNormUsage_list.Add(splitNormInfo[i]);
                change[i] = cnt++;
            }else change[i] = -1;
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
                    tq.Add( Quaternion.Euler(x,y,z));
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
                    cubic.ix = ix;cubic.iy = iy;cubic.iz = iz;
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
        fineCube();//簡化cubeInstance
        
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
        pieceBelong = new int[pieceMap.Count];
        int belongcnt = 0;
        int pmc = pieceMap.Count;
        foreach (string str in pieceMap.Keys)
        {
            PieceGroupInfo pgi = new PieceGroupInfo();
            pgi.initAvaliable();
            pgi.addPiece(str);
            pieceGroup.Add(pgi);
            pieceBelong[getPiece(str).id] = belongcnt;
            belongcnt++;
            /*ui*/
            if (belongcnt % 10 == 0)
            {
                GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + belongcnt + "/" + pmc + ")";
                yield return new WaitForSeconds(0);
            }
        }
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
        GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
        yield return 0;
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
    bool show = true;
    public void calNext()
    {
        if (tokeep.Count == 0)
        {
            targroup++;
            targroup %= pieceGroup.Count;
        }
        else if(targroup > 0) {
            tokeep_cnt++;
            tokeep_cnt %= tokeep_arr.Length;
            targroup = tokeep_arr[tokeep_cnt];
        }
        showHalf();
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
        StartCoroutine(iautoAll());
        
    }
    IEnumerator iautoAll() {
        while (targroup < pieceGroup.Count - 1)
        {
            GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "Loading...(" + targroup + "/" + pieceGroup.Count + ")";
            yield return new WaitForSeconds(0);
            /*****/
            autoMerge(false);
            calNext();
        }
        GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>().text = "";
        showHalf();
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
        if(show)showHalf();
    }
    public void mergeGroup(bool show) {
        List<int> keeped_list = new List<int>();
        foreach (int idx in keeped) keeped_list.Add(idx);
        keeped_list.Sort();
        int[] keeped_array = new int[keeped_list.Count];
        int cnt = 0;
        int firstgroup=-1;
        foreach (int idx in keeped_list) {
            if (firstgroup == -1) firstgroup = idx;
            keeped_array[cnt++] = idx;
        }
        List<int> keeped_reverselist = new List<int>();
        for (int i = keeped_array.Length - 1; i >= 0; i--) keeped_reverselist.Add(keeped_array[i]);
        /****************************************/
        int[] minus = new int[pieceGroup.Count];
        foreach (int idx in keeped_reverselist) {
            HashSet<string> hs = pieceGroup[idx].pieces;
            if (firstgroup == idx) break;
            foreach (string str in hs) {
                pieceGroup[firstgroup].addPiece(str);
                PieceInfo pi = getPiece(str);
                pieceBelong[pi.id] = firstgroup;
            }
            pieceGroup.RemoveAt(idx);
            minus[idx]++;
        }
        for (int i = 0, cal=0; i < minus.Length;i++) {
            cal += minus[i];
            minus[i] = cal;
        }
        for (int i = 0; i < pieceBelong.Length; i++) {
            pieceBelong[i] -= minus[pieceBelong[i]];
        }
        /****************************************/
        keeped = new HashSet<int>();
        tokeep = new HashSet<int>();
        targroup = firstgroup;
        foreach (Cubic cubic in cubics)
        {
            if (cubic.Instance) {
                Tool.setColor(cubic.Instance, Color.grey);
            }
        }
        if(show)showHalf();
        /****************************************/
    }

    public static PieceInfo getPiece(string str) {
        PieceInfo pi;
        pieceMap.TryGetValue(str, out pi);
        return pi;
    }
    public static GameObject putCube(Vector3 pos, float radii, int id) {
        GameObject go = Tool.DrawCube(pos + new Vector3(radii, radii, radii), pos - new Vector3(radii, radii, radii), Color.grey);
        go.transform.parent = GameObject.Find("Collect").transform;
        go.name = "cube_" + id;
        return go;
    }
    public static GameObject putCube(Vector3 pos, Vector3 scale, int id)
    {
        GameObject go = Tool.DrawCube(pos + scale, pos - scale, Color.grey);
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
        foreach (Cubic cubic in cubics)
        {
            cubic.SetActive(false);
        }
        bool active = GameObject.Find("Canvas/Panel_Cube/Toggle_ShowHalf").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (int k in keeped) {
            showGroup(k, active);
        }
        if(targroup>=0)showGroup(targroup, active);
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
    void showGroup(int idx, bool active, Color color)
    {
        foreach (string tarvalstr in pieceGroup[idx].pieces)
        {
            PieceInfo pi = getPiece(tarvalstr);
            foreach (Cubic cubic in pi.cubics)
            {
                cubic.SetActive(active);
                if (active) Tool.setColor(cubic.Instance, color);
            }
        }
        /***************************/
    }
    void showGroup(int idx, bool active) {
        foreach (string tarvalstr in pieceGroup[idx].pieces)
        {
            PieceInfo pi;
            pieceMap.TryGetValue(tarvalstr, out pi);
            foreach (Cubic cubic in pi.cubics)
            {
                cubic.SetActive(active);
            }
        }
        /***************************/
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    
}
public class PieceInfo{
    public int id=-1;
    public string valstr;
    public List<Cubic> cubics = new List<Cubic>();
    public List<Cubic> cubics_touched = new List<Cubic>();
    public List<Touchinfo> touchinfo = new List<Touchinfo>();
    HashSet<int> touchedge = new HashSet<int>();
    public int[] touchnum = new int[ThinStructure.splitNorms.Length];
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
}
public class PieceGroupInfo{
    public HashSet<string> pieces = new HashSet<string>();
    public List<Vector3> avaliableDirs = new List<Vector3>();
    public List<Vector3> limit = new List<Vector3>();

    public void addPiece(string str) {
        pieces.Add(str);
        PieceInfo pi = CubeVolumn.getPiece(str);
        foreach (Touchinfo ti in pi.touchinfo) {
            if (pi.touchnum[ti.contactedge] < 10) continue;
            Vector3 norm = ti.dir;
            List<Vector3> avaliableDirs_next = new List<Vector3>();
            foreach (Vector3 vec in avaliableDirs) {
                if (checkaValiable(vec, norm)) {
                    avaliableDirs_next.Add(vec);
                }
                limit.Add(vec);
            }
            avaliableDirs = avaliableDirs_next;
        }
    }
    public void initAvaliable() {
        int plus = 15;
        for (int x = 0; x < 360; x += plus) {
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
        return Vector3.Dot(vec, norm) > 0;
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
        List<Vector3> avaliableDirs_copy = avaliableDirs;
        List<Vector3> avaliableDirs_next = new List<Vector3>();
        foreach (string str in pgi.pieces) {
            PieceInfo pi = CubeVolumn.getPiece(str);
            foreach (Touchinfo ti in pi.touchinfo)
            {
                Vector3 norm = ti.dir;
                foreach (Vector3 vec in avaliableDirs_copy)
                {
                    if (checkaValiable(vec, norm))
                    {
                        avaliableDirs_next.Add(vec);
                    }
                }
                avaliableDirs_copy = avaliableDirs_next;
                avaliableDirs_next = new List<Vector3>();
            }
        }
        return avaliableDirs_copy.Count;
    }
}
