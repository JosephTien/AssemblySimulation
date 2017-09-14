using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInfo : MonoBehaviour {
    public Cubic cubic;
	// Use this for initialization
	void Start () {

	}
    void Update () {
		
	}
}
public class Touchinfo
{
    public bool isup;
    public int contactedge;
    public Vector3 dir = Vector3.zero;
}
public class Cubic {
    public uint[] vals;
    public char[] valchars;
    public string valstr;
    public bool istube;
    /**/
    public List<Touchinfo> touchinfo = new List<Touchinfo>();
    /**/
    public int id;
    public Vector3 instanceCent;
    public float instanceScale;
    public bool iscovered = false;
    public int ix, iy, iz;
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
    public Cubic(){}
    public GameObject Instance;
    public Cubic(Vector3 position, Vector3 scale, Quaternion rotation) {
        instanceCent = this.position = position;
        instanceScale = 1f;
        this.scale = scale;
        this.rotation = rotation;
    }
    public void init()
    {
        vals = new uint[(CubeVolumn.splitNormUsage.Length + 31) / 32];
        for (int i = 0; i < vals.Length; i++) vals[i] = 0x0;
        valchars = new char[CubeVolumn.splitNormUsage.Length];
    }
    public void setVal(int i, bool v)
    {
        i = CubeVolumn.splitNormInfo[i].mapto;
        int div = i / 32;
        int mod = i % 32;
        uint mask = 0x1; mask = mask << mod;
        if (v) vals[div] = vals[div] | mask;
        else vals[div] = vals[div] & (~mask);
        if (v) valchars[i] = '*';
        else valchars[i] = '-';
    }
    public void genValstr()
    {
        valstr = new string(valchars);
    }
    public void setValchars(int i, char c) {
        valchars[i] = c;
    }

    public bool equals(string valstr_) {
        return valstr == valstr_;
    }
    public bool equals(uint[] vals_)
    {
        for (int i = 0; i < vals.Length; i++)
        {
            if (vals[i] != vals_[i]) return false;
        }
        return true;
    }
    public static bool operator == (Cubic ci1, Cubic ci2)
    {
        //return ci1.equals(ci2.vals);
        return ci1.equals(ci2.valstr);
    }
    public static bool operator !=(Cubic ci1, Cubic ci2)
    {
        //return !ci1.equals(ci2.vals);
        return !ci1.equals(ci2.valstr);
    }
    public void SetActive(bool active) {
        if (Instance == null && active && !iscovered) {
            if (!CubeVolumn.hasid.ContainsKey(id))
            {
                Instance = CubeVolumn.putCube(instanceCent, scale * instanceScale, id);
                CubeVolumn.hasid.Add(id, Instance);
            }
            else {
                CubeVolumn.hasid.TryGetValue(id, out Instance);
            }
            Instance.AddComponent<CubeInfo>().cubic = this;
        }
        if(Instance!=null)Instance.SetActive(active);
    }
}
