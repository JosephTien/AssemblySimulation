using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupInfo : MonoBehaviour
{
    /***************************************/
    public BoundInfo[] boundInfo;
    int div = 180;
    public float[] boundingLen;
    public HashSet<int> vert;
    public HashSet<int> edge;
    public int nodeidx = -1;
    Vector3 initNorm;
    public Vector3 curDir;
    public Vector3 curDir2;
    Vector3 cent;
    float aniangle = 0;
    public float curAngle = 0;
    public float curAngle2 = 0;
    public float curAngle3 = 0;
    public float curAngle2_2 = 0;
    public float curAngle3_2 = 0;
    public float maxAngle2;
    public float maxAngle3;
    public float maxAngle2_2;
    public float maxAngle3_2;
    public float curBoundingLen;
    public static GameObject arrowInst;
    public static GameObject arrowInst2;
    public bool active;
    public static bool shoCol;
    public static bool showed;
    public static HashSet<BoundInfo> coltar;
    /***************************************/
    // Use this for initialization
    void Start()
    {
    }

    public static void showCol(bool show)
    {
        if (show && !showed)
        {
            foreach (BoundInfo col in coltar)
            {
                col.showCol = true;
            }
            showed = true;
        }
        if(!show && showed)
        {
            foreach (BoundInfo bi in Bounding.boundInfo)
            {
                bi.showCol = false;
            }
            showed = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (arrowInst && arrowInst.activeSelf && active)
        {
            arrowInst.transform.position = cent + curDir.normalized * 150;
            Vector3 per = new Vector3(curDir.y, curDir.x, curDir.z);
            aniangle = (aniangle + 2) % 360;
            per = Quaternion.AngleAxis(aniangle, curDir) * per;
            arrowInst.transform.rotation = Quaternion.LookRotation(curDir, per);
        }
        if (arrowInst2 && arrowInst2.activeSelf && active)
        {
            arrowInst2.transform.position = cent + curDir2.normalized * 150;
            Vector3 per = new Vector3(curDir2.y, curDir2.x, curDir2.z);
            aniangle = (aniangle + 2) % 360;
            per = Quaternion.AngleAxis(aniangle, -curDir2) * per;
            arrowInst2.transform.rotation = Quaternion.LookRotation(curDir2, per);
        }
        
    }
    public void init()
    {
        //vert = new HashSet<int>();
        //edge = new HashSet<int>();
        getChildrenBoundInfo();

        if (edge.Count == 0 && vert.Count == 1)
        {
            foreach (int v in vert) {
                nodeidx = v;
            }

            initNorm = Algorithm.nodeNorm(nodeidx);
            curDir = initNorm;
            curDir2 = -curDir;
            Bounding.boundInfo[nodeidx + ThinStructure.edgeNum].curNorm = initNorm;
            return;
        }
        else {
            rotateTo(0);
            initNorm = curDir;
            
        }
    }
    public void getChildrenBoundInfo()
    {
        int n = transform.childCount;
        boundInfo = new BoundInfo[n];
        for (int i = 0; i < n; i++)
        {
            boundInfo[i] = transform.GetChild(i).GetComponent<BoundInfo>();
        }
    }
    public void calBoundingLen()
    {
        boundingLen = new float[div];
        for (int i = 0; i < div; i++)
        {
            boundingLen[i] = float.MaxValue;
        }
        foreach (BoundInfo bi in boundInfo)
        {
            for (int i = 0; i < div; i++)
            {
                if (!bi.isvert)
                {
                    boundingLen[i] = Mathf.Min(boundingLen[i], bi.boundingLenLim[i]);
                }
            }
        }
    }

    public void addRotateTo(float angle)
    {
        curAngle = curAngle + angle;
        curAngle = Mathf.Clamp(curAngle, 0, 179);
        rotateTo(curAngle);
    }

    public void tuneRotate(bool reverse, float angle2, float angle3) {
        List<Vector3> dir = new List<Vector3>();
        angle2 = Mathf.Clamp(angle2, -45, 45);
        angle3 = Mathf.Clamp(angle3, -15, 15);
        if (!reverse)
        {
            curAngle2 = angle2;
            curAngle3 = angle3;
            foreach (BoundInfo bi in boundInfo) {
                dir.Add(bi.anglearg2vec(curAngle, curAngle2, curAngle3));
            }
            curDir = avrDir(dir.ToArray());
        }
        else {
            curAngle2_2 = angle2;
            curAngle3_2 = angle3;
            foreach (BoundInfo bi in boundInfo)
            {
                dir.Add(-bi.anglearg2vec(curAngle, curAngle2_2, curAngle3_2));
            }
            curDir2 = avrDir(dir.ToArray());
        }
        checkCurBoundLim();
        checkArg();
    }
    public void rotateTo(Vector3 vec) {
        //if (Vector3.Dot(initNorm, vec) < 0) vec *= -1;
        curDir = vec;
        curDir2 = -curDir;
        int reverseCnt = 0;
        foreach (BoundInfo bi in boundInfo)
        {
            if (!bi.isvert)
            {
                bool reverse;
                bi.rotateTo(vec, out reverse);
                if (reverse) reverseCnt++;
                else reverseCnt--;
                /*
                if (Vector3.Dot(vec, bi.curNorm) >= 0)
                {
                    //if (bi.angle > 10 && bi.angle < 150) 
                    print("aa");
                }
                else
                {
                    //if (bi.angle > 10 && bi.angle < 150) 
                    print("bb");
                }
                */
                bi.rotateAssisPlane();
            }
        }
        if (reverseCnt > 0) {
            curDir *= -1;
            curDir2 *= -1;
        }
        curAngle = Vector3.Angle(initNorm, curDir);//rough
        
        checkCurBoundLim();
        checkArg();
    }
    public void rotateTo(float angle)
    {
        angle %= 180;
        curAngle = angle;
        List<Vector3> dirs1 = new List<Vector3>();
        List<Vector3> dirs2 = new List<Vector3>();
        foreach (BoundInfo bi in boundInfo)
        {
            if (!bi.isvert)
            {
                bi.rotateTo(angle);
                bi.rotateAssisPlane();
                Vector3 vec = bi.curNorm;
                //norms.Add(vec.normalized * ThinStructure.edges[bi.idx].len);
                dirs1.Add(bi.anglearg2vec(curAngle, curAngle2, curAngle3));
                dirs2.Add(-bi.anglearg2vec(curAngle, curAngle2_2, curAngle3_2));
            }
        }
        curDir = avrDir(dirs1.ToArray());
        curDir2 = avrDir(dirs2.ToArray());
        //curBoundingLen = boundingLen[(int)curAngle];
        checkCurBoundLim();
        checkArg();
    }

    public void checkArg() {
        maxAngle2 = 0;
        maxAngle3 = 0;
        maxAngle2_2 = 0;
        maxAngle3_2 = 0;
        foreach (BoundInfo bi in boundInfo)
        {
            if (!bi.isvert)
            {
                float angle, angle2, angle3;
                angle = bi.vec2anglearg(bi.curNorm, curDir, out angle2, out angle3);
                if (Mathf.Abs(angle2) > Mathf.Abs(maxAngle2)) maxAngle2 = angle2;
                if (Mathf.Abs(angle3) > Mathf.Abs(maxAngle3)) maxAngle3 = angle3;
                angle = bi.vec2anglearg(-bi.curNorm, curDir2, out angle2, out angle3);
                if (Mathf.Abs(angle2) > Mathf.Abs(maxAngle2_2)) maxAngle2_2 = angle2;
                if (Mathf.Abs(angle3) > Mathf.Abs(maxAngle3_2)) maxAngle3_2 = angle3;
            }
        }
    }

    public void checkCurBoundLim()
    {
        foreach (BoundInfo bi in boundInfo)
        {
            if (bi.isvert) bi.gameObject.GetComponent<SphereCollider>().enabled = false;
            else bi.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        }
        showCol(false);
        coltar = new HashSet<BoundInfo>();
        curBoundingLen = float.MaxValue;
        foreach (BoundInfo bi in boundInfo)
        {
            if (!bi.isvert)
            {
                float coldis1 = bi.checkCertainBoundLim(bi.curNorm, curDir);
                if(bi.tempCollider!=null)foreach(BoundInfo bii in bi.tempCollider) coltar.Add(bii);
                float coldis2 = bi.checkCertainBoundLim(bi.curNorm, curDir2);
                if (bi.tempCollider != null) foreach (BoundInfo bii in bi.tempCollider) coltar.Add(bii);
                curBoundingLen = Mathf.Min(curBoundingLen, Mathf.Min(coldis1, coldis2));
            }
        }
        showCol(shoCol);
        foreach (BoundInfo bi in boundInfo)
        {
            if (bi.isvert) bi.gameObject.GetComponent<SphereCollider>().enabled = true;
            else bi.gameObject.GetComponent<CapsuleCollider>().enabled = true;
        }
    }
    
    public Vector3 avrDir(Vector3[] dirs)
    {
        Vector3 avr = Vector3.zero;
        foreach (Vector3 vec in dirs)
        {
            avr += vec;
        }
        return avr.normalized;
    }

    public void setActive(bool active)
    {
        this.active = active;
        foreach (BoundInfo gbi in boundInfo)
        {
            gbi.setActive(active);
            if (gbi.assistPlane) gbi.assistPlane.SetActive(active);
        }
        if (!arrowInst)
        {
            arrowInst = GameObject.Instantiate(Resources.Load("Arrow") as GameObject);
            arrowInst.name = "Arrow";
            arrowInst.transform.parent = GameObject.Find("Assist").transform;
            arrowInst2 = GameObject.Instantiate(Resources.Load("Arrow") as GameObject);
            arrowInst2.name = "Arrow";
            arrowInst2.transform.parent = GameObject.Find("Assist").transform;
            arrowInst.transform.localScale = new Vector3(10, 1, 30);
            arrowInst2.transform.localScale = new Vector3(10, 1, 30);
        }
        arrowInst.SetActive(active);
        arrowInst2.SetActive(active);
        if (active)
        {
            if (nodeidx > 0) {
                cent = ThinStructure.vertices[nodeidx];
                return;
            } 
            Vector3 c = Vector3.zero;
            foreach (BoundInfo bi in boundInfo)
            {
                c += ThinStructure.edges[bi.idx].cent;
            }
            c /= boundInfo.Length;
            float minVal = float.MaxValue;
            Vector3 minC = Vector3.zero;
            foreach (int e in edge)
            {
                float len = (c - ThinStructure.edges[e].cent).magnitude;
                if (len < minVal)
                {
                    minC = ThinStructure.edges[e].cent;
                    minVal = len;
                }
            }
            cent = minC;
        }
    }
    /********************************************************************************/
    /*Higher logic*/
    public bool mergeMany = false;
    public bool isChild = false;
    public Vector3 mainNorm;
    public static int mergeGroupCnt = 0;//becareful the init
    public GameObject genMergeRoot() {
        if (isChild) return transform.parent.gameObject;
        GameObject Collect = GameObject.Find("Collect");
        GameObject go = GameObject.Instantiate(Resources.Load("Group") as GameObject);
        go.name = "Merge_" + (mergeGroupCnt++);
        go.transform.parent = Collect.transform;
        transform.parent = go.transform;
        isChild = true;
        go.GetComponent<GroupInfo>().mergeMany = true;
        go.GetComponent<GroupInfo>().mainNorm = curDir;
        return go;
    }
    public void mergeNeighborCurve() {//must be node
        if (nodeidx == -1) return;
        GameObject parent = genMergeRoot();
        foreach (int e in ThinStructure.verticesedges[nodeidx]) {
            GroupInfo gi = Bounding.groupInfo[Bounding.compIdx[e]];
            if (gi.isChild) continue;
            //Bounding.boundInfo[e].rotateTo(initNorm);//gi.rotateTo(Bounding.boundInfo[e].angle);
            gi.rotateTo(curDir);
            //test
            //foreach (BoundInfo bi in gi.boundInfo) if (!bi.isvert && bi.angle > 10 && bi.angle < 150) { if (Vector3.Dot(curDir, bi.curNorm) >= 0) print("a"); else print("b"); }

            gi.setActive(true);
            gi.transform.parent = parent.transform;
            gi.isChild = true;
        }
        setActive(true);
    }
    public void setChildActive(bool active)
    {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).GetComponent<GroupInfo>().setActive(active);
        }
    }
}


