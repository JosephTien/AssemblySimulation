
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class Simulation : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
		
	}

    public Simulator[] simulators;

    GameObject loadObj(int tarSet, string name, int type)
    {
        string path = "inputSet\\" + tarSet + "\\inputObj\\";
        string tarFab = "Empty";
        if (type == 1) tarFab = "Glass";
        GameObject obj = GameObject.Instantiate(Resources.Load(tarFab), Vector3.zero, Quaternion.identity) as GameObject;
        Mesh holderMesh = new Mesh();
        ObjImporter newMesh = new ObjImporter();
        holderMesh = newMesh.ImportFile(path + name + ".obj");
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        holderMesh.RecalculateNormals();
        filter.mesh = holderMesh;
        obj.transform.parent = GameObject.Find("Collect").transform;
        obj.name = this.name = name;
        obj.AddComponent<Simulator>();
        return obj;
    }
    public void load() {
        Tool.clearObj();
        string inputtext = GameObject.Find("Canvas/InputField_Input/Text").GetComponent<UnityEngine.UI.Text>().text;
        string[] inputtexts = inputtext.Split('*');
        int myscale; if (inputtexts.Length>1) myscale  = int.Parse(inputtexts[1]);
        int tarSet = int.Parse(inputtexts[0]);
        loadObj(tarSet, "output_0", 1);
        /************************************/
        int objNum = 0;
        //read  ordering
        if (File.Exists(".\\inputSet\\" + tarSet + "\\input\\ordering.txt"))
        {
            System.IO.StreamReader file = new System.IO.StreamReader(".\\inputSet\\" + tarSet + "\\input\\ordering.txt");
            string line = file.ReadLine();
            objNum = int.Parse(line);
            simulators = new Simulator[objNum];
            List<Simulator> simulators_ = new List<Simulator>();
            for (int i = 0; i < objNum; i++)
            {
                line = file.ReadLine();
                string[] items = line.Split(' ');
                int sort = int.Parse(items[0]);
                Vector3 dir = new Vector3(float.Parse(items[1]), float.Parse(items[2]), float.Parse(items[3]));
                GameObject go = loadObj(tarSet, "output_"+(i+1), 0);
                /**/
                Simulator sim = go.AddComponent<Simulator>();
                sim.id = i;
                sim.direction = dir.normalized;
                sim.sort = sort;
                simulators_.Add(sim);
                /**/
                /*
                simulators[sort] = go.AddComponent<Simulator>();
                simulators[sort].id = i;
                simulators[sort].direction = dir.normalized;
                */
                if (dir == Vector3.zero) go.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            int cnt=0;
            for (int i = 0; i < objNum; i++) {
                foreach (Simulator sim in simulators_) {
                    if (sim.sort == i)
                        simulators[cnt++] = sim;
                }
            }
            file.Close();
        }
        
    }

    public void startSplite() {
        StartCoroutine(Spliting());
    }

    float timing = 0.333f;
    IEnumerator Spliting()
    {
        int lastsort = -1;
        foreach (Simulator sim in simulators)
        {
            if(sim.sort != lastsort)yield return new WaitForSeconds(timing);
            if (sim.direction == Vector3.zero) continue;
            else sim.startmove();
            lastsort = sim.sort;
        }
	}
}
