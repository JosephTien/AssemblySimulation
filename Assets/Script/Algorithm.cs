using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Algorithm : MonoBehaviour {

    public static float angleFix(float angle, float radii) {
        float angleFix = 1e-5f;
        if (angle <= 90) angleFix = Mathf.Max(angleFix, radii / Mathf.Tan(angle / 2 / 180 * Mathf.PI));
        else angleFix = Mathf.Max(angleFix, radii * Mathf.Cos((angle - 90) / 180 * Mathf.PI));
        return angleFix;
    }

    public static float angleFix(int edge, int which, float radii)
    {
        if (which == 0)
        {
            if (ThinStructure.verticesedges[ThinStructure.edges[edge].idx1].Count == 1) return radii;
            float angleFix1 = angleFix(ThinStructure.edges[edge].minAngle1, radii);
            angleFix1 = Mathf.Max(radii, angleFix1);
            return angleFix1;
        }
        else if (which == 1)
        {
            if (ThinStructure.verticesedges[ThinStructure.edges[edge].idx2].Count == 1) return radii;
            float angleFix2 = angleFix(ThinStructure.edges[edge].minAngle2, radii);
            angleFix2 = Mathf.Max(radii, angleFix2);
            return angleFix2;
        }
        else return 0;
    }


    public static Vector3 nodeNorm(int node) {
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
        return common;
    }

    public static List<Vector3> nodeNorms(List<Vector3> vecs, int nodeIdx, ref Vector3 outputcommon) {//assume normalized
        List<Vector3> norms = new List<Vector3>();
        if (vecs.Count == 2)
        {
            Vector3 common = Vector3.Cross(vecs[1], vecs[0]).normalized;
            if (Tool.isParallel(vecs[1], vecs[0]))
            {
                int edgeIdx=-1;
                foreach (int e in ThinStructure.verticesedges[nodeIdx]) {
                    edgeIdx = e;
                }
                common = ThinStructure.splitNorms[edgeIdx];
            }
            foreach (Vector3 vec in vecs)
            {
                Vector3 norm = Vector3.Cross(Vector3.Cross(vec, common), vec);
                norms.Add(norm);
            }
            outputcommon = common;
        }
        if (vecs.Count == 3) {
            Vector3 common = Vector3.Cross(vecs[1] - vecs[0], vecs[2] - vecs[0]).normalized;
            int inverseCnt = 0;
            foreach (Vector3 vec in vecs) {
                if (Vector3.Dot(vec, common) < 0) inverseCnt++;
            }
            if (inverseCnt >= 2) common *= -1;
            outputcommon = common;
            foreach (Vector3 vec in vecs)
            {
                Vector3 norm = Vector3.Cross(Vector3.Cross(vec, common), vec).normalized;
                norms.Add(norm);
            }
        }
        if (vecs.Count == 4)
        {
            Vector3 common = Vector3.zero;
            Vector3 stdvec = Vector3.zero;
            for (int i = 0; i < 4; i++) {
                Vector3 vec = Vector3.Cross(vecs[(i + 1)%4] - vecs[i], vecs[(i + 2)%4] - vecs[i]).normalized;
                if (i == 0) stdvec = vec;
                else {
                    if (Vector3.Dot(vec, stdvec) < 0) vec *= -1;
                }
                common += vec;
            }
            common.Normalize();
            int inverseCnt = 0;
            foreach (Vector3 vec in vecs)
            {
                if (Vector3.Dot(vec, common) < 0) inverseCnt++;
            }
            if (inverseCnt > 2) common *= -1;
            outputcommon = common;
            foreach (Vector3 vec in vecs)
            {
                Vector3 norm = Vector3.Cross(Vector3.Cross(vec, common), vec).normalized;
                norms.Add(norm);
            }
        }
        return norms;
    }
    class LS
    {
        public LS(Vector3 v1, Vector3 v2) {
            this.v1 = v1;
            this.v2 = v2;
        }
        public Vector3 vec { get { return (v2 - v1).normalized; } }
        public Vector3 v1;
        public Vector3 v2;
    };
    public static Vector3 calFittingPlane(List<Vector3> positions) {

        if (positions.Count <= 2) return new Vector3(Random.Range(0, 0.99f), Random.Range(0, 0.99f), Random.Range(0, 0.99f));
        /********************************/
        Vector3[] pos = positions.ToArray();
        Vector3 norm = Vector3.zero;
        List<LS> searchSpace_ = new List<LS>();
        /********************************/
        for (int i = 0; i < pos.Length-1;i++) {
            searchSpace_.Add(new LS(pos[i], pos[i + 1]));
        }
        LS[] searchSpace = searchSpace_.ToArray();
        float minval = float.MaxValue;
        for (int i = 0; i < searchSpace.Length; i++)
        {
            for (int j = i+1; j < searchSpace.Length; j++)
            {
                float val = Mathf.Abs(Vector3.Dot(searchSpace[i].vec, searchSpace[j].vec));
                if (val < minval) {
                    minval = val;
                    norm = Vector3.Cross(searchSpace[i].vec, searchSpace[j].vec).normalized;
                }
            }
        }
        /********************************/
        /*
        for (int i = 0; i < pos.Length;i++) {
            for (int j = i+1; j < pos.Length; j++)
            {
                searchSpace_.Add(new LS(pos[i], pos[j]));
            }
        }
        LS[] searchSpace = searchSpace_.ToArray();
        float maxval = 0;
        for (int i = 0; i < searchSpace.Length; i++)
        {
            for (int j = i+1; j < searchSpace.Length; j++)
            {
                Vector3 localnorm = Vector3.Cross(searchSpace[i].vec, searchSpace[j].vec).normalized;
                float val = 0;
                for (int k = 0; k < searchSpace.Length; k++) {
                    val += Vector3.Dot(searchSpace[k].vec, localnorm);
                }
                if (val > maxval) {
                    maxval = val;
                    norm = localnorm;
                }
            }
        }
        */
        return norm;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}