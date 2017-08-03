using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BoundInfo : MonoBehaviour
{
    public bool isvert = false;
    public int solved = 2;
    public bool showstate = false;
    public bool showgroup = false;
    public bool showCol = false;
    public static Color standColor;
    public Color groupcolor;
    bool forceshowplane = false;
    /**************************************/
    public bool active = false;
    public int idx;
    public Vector3 axis;
    public Vector3 initNorm;
    public Vector3 curNorm;
    public Quaternion curRot;
    public float angle = 0;
    public float len = 20;
    public float minAngle1;
    public float minAngle2;
    public float fixDis1;
    public float fixDis2;
    /**************************************/
    public float[] boundingLen;
    public float[] boundingLenLim;
    public float maxLen1, maxLen2;
    public float bestRotAngle1, bestRotAngle2;
    public Vector3 bestVec1, bestVec2;
    public HashSet<BoundInfo> tempCollider;
    /**************************************/
    public float plus = 10;
    public static int div = 180;
    GameObject checker;
    Vector3 checkerInitPos;
    float checkerangle;
    float checkerdis;
    public static float checkerdis_lim = 2000;
    float checkerdisstep = 5;
    float checkeranglestep = 180 / div;
    /**************************************/
    GameObject assistLine;
    public MeshRenderer assistLineMR;
    public GameObject assistPlane;
    /**************************************/
    // Use this for initialization
    public void rotateTo(Vector3 to)
    {
        curNorm = to;
        angle = Vector3.Angle(initNorm, to);
        curRot = Quaternion.AngleAxis(angle, axis);
        if (Vector3.Dot(axis, Vector3.Cross(initNorm, to)) < 0 || angle >= 180) angle = 180 - angle;
    }
    public void rotateTo(float angle)
    {
        if (angle >= 180) angle = angle - 180;
        if (angle < 0) angle = 180 + angle;
        this.angle = angle;
        curRot = Quaternion.AngleAxis(angle, axis);
        curNorm = (curRot * initNorm).normalized;
    }
    public void rotateAssisPlane()
    {
        curRot = Quaternion.AngleAxis(angle, axis);
        curNorm = (curRot * initNorm).normalized;
        assistPlane.transform.localRotation = Quaternion.LookRotation(curNorm, axis);
    }
    public int isSoled()
    {
        if (isvert)
        {
            if (ThinStructure.verticesedges[idx].Count > 4)
            {
                return 0;
            }
            if (ThinStructure.verticesedges[idx].Count > 3)
            {
                return 1;
            }
            else
            {
                return 2;}

        }
        bool well = false;
        float lim = 10;
        for (int i = 0; i < boundingLenLim.Length; i++)
        {
            if (boundingLenLim[i] > lim)
            {
                well = true;
            }
        }
        if (well) return 2;
        return 0;
    }
    void Start()
    {
        standColor = GetComponent<Renderer>().material.color;
        boundingLen = new float[div * 2];
        boundingLenLim = new float[div];
    }
    void FixedUpdate()
    {
        if (showCol) {
            setColor(Color.red);
        }
        else if (showgroup)
        {
            setColor(groupcolor);
        }
        else if (showstate)
        {
            if (solved == 0)
            {
                setColor(Color.red);
            }
            else if (solved == 1)
            {
                //setColor(new Color(1,0.5f,0,1));
                setColor(Color.green);
            }
            else
            {
                setColor(Color.green);
            }
        }
        else
        {
            //setColor(standColor);
            setColor(Color.white);
        }
        /*******************************/
        if (isvert) return;
        if (active)
        {
            if (!Bounding.groupMode)
            {
                if (Bounding.valid)
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        angle += plus;
                        angle = Mathf.Clamp(angle, 0, 179);
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        angle -= plus;
                        angle = Mathf.Clamp(angle, 0, 179);
                    }
                }
                curRot = Quaternion.AngleAxis(angle, axis);
                curNorm = (curRot * initNorm).normalized;
                assistPlane.transform.localRotation = Quaternion.LookRotation(curNorm, axis);
                if (!forceshowplane) assistPlane.SetActive(true);
                //len = boundingLen[(int)angle];
                if (true)
                {//暫時只顯示某個被選擇的距離值。之後要改成整個comp。
                    if (boundingLen[(int)angle] == checkerdis_lim)
                        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "Inf";
                    else
                        GameObject.Find("Canvas/Text").GetComponent<UnityEngine.UI.Text>().text = "" + boundingLenLim[(int)angle];
                }
            }
        }
        else
        {
            if (!forceshowplane) assistPlane.SetActive(false);
        }
    }
    public void showPlane(bool toActive)
    {
        forceshowplane = toActive;
        assistPlane.SetActive(toActive);
    }
    public void setActive(bool toActive)
    {
        if (toActive != active)
        {
            active = toActive;
            if (active)
            {

            }
            else
            {
                if (assistLine) Destroy(assistLine);
            }
        }
        if (active)
        {
            if (isvert) generateAssist_();
            else generateAssist();
        }
    }
    public void setColor(Color color)
    {
        if (GetComponent<Renderer>().material.color == color) return;
        GetComponent<Renderer>().material.color = color;
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
        {
            r.material.color = color;
        }
    }
    public void setAssistColor(Color color)
    {
        if (!assistLineMR) return;
        if (assistLineMR.material.color == color) return;
        assistLineMR.material.color = color;
        Renderer[] rs = assistLineMR.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
        {
            r.material.color = color;
        }
    }
    public void generateAssist_()
    {
        Vector3 p = ThinStructure.vertices[idx];
        if (!assistLine)
        {
            assistLine = Tool.DrawBall(p, ThinStructure.tuberadii * 1.02f, Color.green);
            assistLine.name = "AssistBall";
            assistLine.transform.parent = GameObject.Find("Assist").transform;
            assistLineMR = assistLine.GetComponent<MeshRenderer>();

            float r = ThinStructure.tuberadii * 1.02f;
            foreach (int edge in ThinStructure.verticesedges[idx])
            {
                Vector3 vec = ThinStructure.edges[edge].vec;
                float dis;
                if (ThinStructure.edges[edge].idx1 == idx)
                {
                    dis = ThinStructure.edges[edge].fixDis1;
                }
                else
                {
                    vec *= -1;
                    dis = ThinStructure.edges[edge].fixDis2;
                }
                Tool.DrawLine(p, p + vec * dis, r, Color.green).transform.parent = assistLine.transform;
            }
        }
        else
        {
            assistLineMR.material.color = Color.green;
        }
    }
    public void generateAssist()
    {
        bool onlysec = true;
        bool showfixed = true;
        Vector3 p1 = ThinStructure.vertices[ThinStructure.edges[idx].idx1];
        Vector3 p2 = ThinStructure.vertices[ThinStructure.edges[idx].idx2];
        Vector3 p12 = (p2 - p1);
        Vector3 cent = (p1 + p2) / 2;
        if (onlysec)
        {
            if (showfixed)
            {
                p1 += axis * fixDis1;
                p2 -= axis * fixDis2;
            }
            else
            {
                p1 += axis * ThinStructure.tuberadii;
                p2 -= axis * ThinStructure.tuberadii;
            }
            if (Vector3.Dot((p2 - p1), p12) < 0)
            {
                p1 = cent;
                p2 = cent;
            }
        }
        if (!assistLine)
        {
            assistLine = Tool.DrawLine(p1, p2, ThinStructure.tuberadii * 1.02f, Color.green);
            assistLine.name = "AssistTube";
            assistLine.transform.parent = GameObject.Find("Assist").transform;
            assistLineMR = assistLine.GetComponent<MeshRenderer>();
        }
        else
        {
            assistLineMR.material.color = Color.green;
        }
    }
    /*************************************************************************************/
    public void findBestBound()
    {///擴散法
        float[] boundingLenVal = new float[div];
        for (int i = 0; i < div; i++)
        {
            boundingLenVal[i] = Mathf.Min(checkerdis_lim - boundingLen[i], checkerdis_lim - boundingLen[i + div]);
        }
        for (int n = 0; n < 100; n++)
        {
            float[] boundingLenVal_new = new float[div];
            for (int i = 0; i < div; i++)
            {
                boundingLenVal_new[i] = (boundingLenVal[i] + boundingLenVal[(i + 1) % div] + boundingLenVal[(i + div - 1) % div]) / 3;
            }
            for (int i = 0; i < div; i++)
            {
                boundingLenVal[i] = boundingLenVal_new[i];
            }
        }
        for (int i = 0; i < div; i++)
        {
            boundingLenVal[i] = checkerdis_lim - boundingLenVal[i];
        }
        /****************************************************************************/
        float maxLenVal = float.MinValue;
        float maxAngle = 0;
        for (checkerangle = 0; checkerangle < 180; checkerangle += checkeranglestep)
        {
            if (maxLenVal < boundingLenVal[(int)checkerangle])
            {
                maxLenVal = boundingLenVal[(int)checkerangle];
                maxAngle = checkerangle;
            }
        }

        maxLen1 = boundingLen[(int)maxAngle];
        maxLen2 = boundingLen[(int)maxAngle + 180];
        bestRotAngle1 = maxAngle;
        bestRotAngle2 = maxAngle + 180;
        bestVec1 = (Quaternion.AngleAxis(bestRotAngle1, axis) * initNorm).normalized;
        bestVec2 = (Quaternion.AngleAxis(bestRotAngle2, axis) * initNorm).normalized;
        angle = bestRotAngle1;
    }
    public void checkAllBound()
    {
        if (isvert) return;
        GetComponent<CapsuleCollider>().enabled = false;
        int cnt;
        for (cnt = 0, checkerangle = 0; checkerangle < 360; checkerangle += checkeranglestep, cnt++)
        {
            boundingLen[cnt] = checkerdis_lim;
            for (checkerdis = 0; checkerdis <= checkerdis_lim; checkerdis += checkerdisstep)
            {
                if (checkBound(checkerangle, checkerdis))
                {
                    boundingLen[cnt] = checkerdis - checkerdisstep;//becareful of the 0
                    break;
                }
            }
        }
        for (int i = 0; i < div; i++) {
            boundingLenLim[i] = Mathf.Min(boundingLen[i], boundingLen[i+div]);
        }
        GetComponent<CapsuleCollider>().enabled = true;
    }
    public bool checkBound(float testangle, float dis)
    {//becareful for the short edge!!!
        Quaternion testRot = Quaternion.AngleAxis(testangle, axis);
        Vector3 testNorm = (testRot * initNorm).normalized;
        Quaternion bbrot = Quaternion.LookRotation(testNorm, axis);//bounding box rotation
        Edge edge = ThinStructure.edges[idx];
        if (edge.len < fixDis1 + fixDis2) return false;
        Vector3 p1 = ThinStructure.vertices[edge.idx1] + axis * (fixDis1 * 1.001f);//axis has bee normalized, 1.01 to miner the bound
        Vector3 p2 = ThinStructure.vertices[edge.idx2] - axis * (fixDis2 * 1.001f);//axis has bee normalized, 1.01 to miner the bound
        Vector3 p1p, p2p;
        float r = ThinStructure.tuberadii * 0.99f;//0.99 to miner the bound
        float len = (p1 - p2).magnitude;

        p1p = p1 + testNorm * dis;
        p2p = p2 + testNorm * dis;
        bool collision1 = Physics.CheckCapsule(p1p, p2p, r) && Physics.CheckBox((p1p + p2p) / 2, new Vector3(r, (p1p - p2p).magnitude / 2, r), bbrot);//capsule & box
        return collision1;
        /*雙向一起計算的方法
        p1p = p1 - testNorm * dis;
        p2p = p2 - testNorm * dis;
        bool collision2 = Physics.CheckCapsule(p1p, p2p, r) && Physics.CheckBox((p1p + p2p) / 2, new Vector3(r, (p1p - p2p).magnitude / 2, r), bbrot);//capsule & box
        return collision1 | collision2;
        */
        /*bkup*/
        //checker.transform.position = checkerInitPos + testNorm * dis;
        //print(Physics.OverlapCapsule(p1, p2, ThinStructure.tuberadii * 0.99f)[0].gameObject.transform.parent.name);
        /*bkup*/
    }
    public bool checkBound(float testangle, float testangle2, float testangle3, float dis)
    {
        Quaternion testRot = Quaternion.AngleAxis(testangle, axis);
        Vector3 testNorm = (testRot * initNorm).normalized;
        Quaternion bbrot = Quaternion.LookRotation(testNorm, axis);//bounding box rotation
        Edge edge = ThinStructure.edges[idx];
        if (edge.len < fixDis1 + fixDis2) return false;
        Vector3 p1 = ThinStructure.vertices[edge.idx1] + axis * (fixDis1 * 1.001f);//axis has bee normalized, 1.01 to miner the bound
        Vector3 p2 = ThinStructure.vertices[edge.idx2] - axis * (fixDis2 * 1.001f);//axis has bee normalized, 1.01 to miner the bound
        Vector3 p1p, p2p;
        float r = ThinStructure.tuberadii * 0.99f;//0.99 to miner the bound
        float len = (p1 - p2).magnitude;

        Vector3 axis2 = Vector3.Cross(testNorm, axis).normalized;
        Vector3 direction = Quaternion.AngleAxis(testangle3, axis) * (Quaternion.AngleAxis(testangle2, axis2) * testNorm);

        p1p = p1 + direction * dis;
        p2p = p2 + direction * dis;
        return Physics.CheckCapsule(p1p, p2p, r) && Physics.CheckBox((p1p + p2p) / 2, new Vector3(r, (p1p - p2p).magnitude / 2, r), bbrot);//capsule & box
    }
    public bool checkBound(Vector3 testNorm, Vector3 direction, float dis)//1.1?
    {
        Quaternion bbrot = Quaternion.LookRotation(testNorm, axis);//bounding box rotation
        Edge edge = ThinStructure.edges[idx];
        Vector3 p1 = ThinStructure.vertices[edge.idx1] + axis * (fixDis1 * 1.1f);//axis has bee normalized, 1.01 to miner the bound
        Vector3 p2 = ThinStructure.vertices[edge.idx2] - axis * (fixDis2 * 1.1f);//axis has bee normalized, 1.01 to miner the bound
        Vector3 p1p, p2p;
        float r = ThinStructure.tuberadii * 0.99f;//0.99 to miner the bound
        float len = (p1 - p2).magnitude;
        p1p = p1 + direction * dis;
        p2p = p2 + direction * dis;

        bool rtn = Physics.CheckCapsule(p1p, p2p, r) && Physics.CheckBox((p1p + p2p) / 2, new Vector3(r, (p1p - p2p).magnitude / 2, r), bbrot);//capsule & box
        tempCollider = new HashSet<BoundInfo>();
        if (rtn) {
            Collider[] capCol = Physics.OverlapCapsule(p1p, p2p, r);
            Collider[] capBox = Physics.OverlapBox((p1p + p2p) / 2, new Vector3(r, (p1p - p2p).magnitude / 2, r), bbrot);
            foreach (Collider col1 in capCol) {
                foreach (Collider col2 in capCol)
                {
                    if (col1.name == col2.name) {
                        tempCollider.Add(col1.gameObject.GetComponent<BoundInfo>());
                    }
                }
            }
        }
        return rtn;
    }
    public float vec2anglearg(Vector3 norm, Vector3 vec, out float angle2, out float angle3)
    {
        norm = Vector3.Cross(axis, Vector3.Cross(norm, axis));
        float angle = Vector3.Angle(initNorm, norm);
        if (Vector3.Dot(Vector3.Cross(initNorm, norm), axis) < 0) angle = 360 - angle;

        Vector3 cmptar = Vector3.Cross(axis, Vector3.Cross(vec, axis));
        angle2 = Vector3.Angle(cmptar, vec);
        if (Vector3.Dot(Vector3.Cross(cmptar, vec), Vector3.Cross(cmptar, axis)) < 0) angle = -angle;
        angle3 = Vector3.Angle(norm, cmptar);
        if (Vector3.Dot(Vector3.Cross(norm, cmptar), axis) < 0) angle = - angle;
        return angle;
    }



    public static Vector3 anglearg2vec(Vector3 axis, Vector3 testNorm , float angle2, float angle3)
    {
        Vector3 axis2 = Vector3.Cross(testNorm, axis).normalized;
        Vector3 direction = Quaternion.AngleAxis(angle3, axis) * (Quaternion.AngleAxis(angle2, axis2) * testNorm);
        return direction;
    }

    public Vector3 anglearg2vec(float angle, float angle2, float angle3)
    {
        Quaternion testRot = Quaternion.AngleAxis(angle, axis);
        Vector3 testNorm = (testRot * initNorm).normalized;
        Vector3 axis2 = Vector3.Cross(testNorm, axis).normalized;
        Vector3 direction = Quaternion.AngleAxis(angle3, axis) * (Quaternion.AngleAxis(angle2, axis2) * testNorm);
        return direction;
    }

    public float checkCertainBoundLim(Vector3 norm, Vector3 dir) {
        GetComponent<CapsuleCollider>().enabled = false;
        for (checkerdis = 0; checkerdis <= checkerdis_lim; checkerdis += checkerdisstep)
        {
            if (checkBound(norm, dir, checkerdis)) {
                checkerdis -= checkerdisstep;
                if (checkerdis < 0) {
                    checkerdis = 0;
                }
                break;
            }
        }
        GetComponent<CapsuleCollider>().enabled = true;
        return checkerdis;
    }
}
public class AngleArg {
    public AngleArg()
    {
        angle2 = 0;
        angle3 = 0;
        angle2_2 = 0;
        angle3_2 = 0;
    }
    public AngleArg(float a2, float a3, float a2_2, float a3_2) {
        angle2 = a2;
        angle3 = a3;
        angle2_2 = a2_2;
        angle3_2 = a3_2;
    }
    public Vector3 norm;
    public Vector3 axis;
    public float angle2, angle3, angle2_2, angle3_2;   
}