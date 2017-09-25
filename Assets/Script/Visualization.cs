using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class Shape{
    public Shape(Vector3 vec) {
        this.vec = vec;
    }
    public GameObject instance;
    public Vector3 vec;
    public Vector3 pos;
    //int order;
    public void setMesh(string path, string name){
        instance = GameObject.Instantiate(Resources.Load("Empty") as GameObject);
        Tool.inputMesh(instance, path + name + ".obj");
        Tool.indMesh(instance);
        instance.transform.parent = GameObject.Find("Collect").transform;
        instance.name = name;
        instance.AddComponent<MeshCollider>();
    }
}
public class Visualization : MonoBehaviour {

    UnityEngine.UI.Dropdown Dropdown;
    UnityEngine.UI.Text Msg;
    UnityEngine.UI.Slider Slider;
    List<GameObject> knifeGos;
    //**************************************
    void Start() {
        Dropdown = GameObject.Find("Canvas/Dropdown").GetComponent<UnityEngine.UI.Dropdown>();
        foreach (DirectoryInfo file in (new DirectoryInfo(".\\data\\")).GetDirectories()) {
            Dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { text = file.Name });
        }
        ThinStructure.tuberadii = 5.0f;
        Msg = GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>();
        Slider = GameObject.Find("Animation_Slider").GetComponent<UnityEngine.UI.Slider>();
    }
    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                string[] strs = hit.collider.name.Split('_');
                if (strs[0] == "shape") {
                    int idx = int.Parse(strs[1]);
                    for (int i = 0; i < shapes.Count; i++) {
                        if (i != idx) shapes[i].instance.SetActive(false);
                    }
                    shapes[idx].instance.SetActive(true);
                    Cube.SetActive(true);
                    Msg.text = "Pick " + hit.collider.name;
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                string[] strs = hit.collider.name.Split('_');
                if (strs[0] == "shape")
                {
                    int idx = int.Parse(strs[1]);
                    for (int i = 0; i < shapes.Count; i++)
                    {
                        if (i != idx) shapes[i].instance.SetActive(true);
                    }
                    shapes[idx].instance.SetActive(false);
                    Cube.SetActive(true);
                    Msg.text = "unPick " + hit.collider.name;
                }
            }
            else
            {
                for (int i = 0; i < shapes.Count; i++)
                {
                    shapes[i].instance.SetActive(true);
                    Cube.SetActive(false);
                    Cube.SetActive(false);
                }
            }
        }
        if (Input.GetMouseButtonDown(2)) {
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].instance.SetActive(true);
                Cube.SetActive(false);
            }
            GameObject.Find("Canvas/ShowCube").GetComponent<UnityEngine.UI.Toggle>().isOn = true;
            Msg.text = "";
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ThinStructure.scale(2);
            Tool.clearObj_immed();
            ThinStructure.basicPut();
            ThinStructure.setColor(Color.gray);
            scaleval *= 2;
            Msg.text = "scale " + scaleval;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ThinStructure.scale(0.5f);
            Tool.clearObj_immed();
            ThinStructure.basicPut();
            ThinStructure.setColor(Color.gray);
            scaleval /= 2;
            Msg.text = "scale " + scaleval;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Msg.text = "";
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Slider.value -= 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Slider.value += 0.1f;
        }
    }
    void FixedUpdate()
    {
        if (shapes == null) return;
        float slidervalue = Slider.value;
        foreach (Shape shape in shapes)
        {
            if (shape.instance == null) continue;
            Vector3 tarPos = shape.pos * slidervalue;
            Vector3 curpos = shape.instance.transform.position;
            Vector3 vec = (tarPos - curpos) / timediv;
            if ((shape.instance.transform.position - tarPos).magnitude < disthre) shape.instance.transform.position = tarPos;
            else
            {
                if (vec.magnitude < disthre) vec = vec.normalized * disthre;
                shape.instance.transform.position += vec;
            }
        }
    }
    //**************************************
    float timediv = 10;
    float disthre = 0.5f;
    float dismul = 300;
    float scaleval = 1;
    //**************************************
    string path = "";
    bool loaded = false;
    GameObject Cube;
    List<Shape> shapes;
    int shapenum;
    Vector3 max, min;
    bool moveableToggle = false;
    //**************************************
    public void setPath() {
        path = "";
        if (name == "") return;
        path = ".\\data\\" + Dropdown.options[Dropdown.value].text + "\\";
    }
    public void loadTopo() {
        if (path == "") return;
        loaded = true;
        Tool.clearObj_immed();
        ThinStructure.basicRead(path);
        ThinStructure.basicPut();
        ThinStructure.setColor(Color.gray);
        shapes = new List<Shape>();
        scaleval = 1;
    }
    public void writeData() {
        if (!loaded) return;
        ThinStructure.outputThin(path);
        ThinStructure.outputsplitNorms(path);
    }
    public void orient(){
        ThinStructure.moveToCenter(ThinStructure.calMinBounding());
        Tool.clearObj_immed();
        ThinStructure.basicPut();
        ThinStructure.setColor(Color.gray);
    }
    public void showPlane() {
        bool active = GameObject.Find("Canvas/ShowPlane").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (GameObject go in ThinStructure.planeGOs) {
            go.SetActive(active);
        }
    }
    public void showTopo()
    {
        bool active = GameObject.Find("Canvas/ShowTopo").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (GameObject go in ThinStructure.verticeGOs)
        {
            go.SetActive(active);
        }
        foreach (GameObject go in ThinStructure.edgeGOs)
        {
            go.SetActive(active);
        }
    }
    public void showKnife()
    {
        bool active = GameObject.Find("Canvas/ShowKnife").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (GameObject go in knifeGos) {
            go.SetActive(active);
        }
    }
    public void showCube()
    {
        bool active = GameObject.Find("Canvas/ShowCube").GetComponent<UnityEngine.UI.Toggle>().isOn;
        foreach (Shape shape in shapes)
        {
            shape.instance.SetActive(active);
        }
        Cube.SetActive(false);
    }
    public void optimal() {
        try
        {
            writeData();
            Executor.RunCmd("copy /Y " + path + "thinstruct.txt .\\slicer\\");
            Executor.RunCmd("copy /Y " + path + "splitinfo.txt .\\slicer\\");
            Executor.RunCmd("cd slicer&Slicer.exe", true);
            //*********************************************
            System.IO.StreamReader file = new System.IO.StreamReader(".\\slicer\\shapeinfo.txt");
            string line = file.ReadLine(); string[] items = line.Split(' ');
            shapenum = int.Parse(items[0]);
            for (int i = 0; i < shapenum; i++)
            {
                Executor.appendCmdSB("copy /Y " + ".\\slicer\\pool\\" + "shape_" + i + ".obj" + " " + path + "obj\\"); //Executor.RunCmd("copy /Y " + ".\\slicer\\pool\\" + "shape_" + i + ".obj" + " " + path + "obj\\");
            }
            Executor.flushCmdSB("~");
            Executor.RunCmd("copy /Y " + ".\\slicer\\" + "shapeinfo.txt " + path);
            Executor.RunCmd("copy /Y " + ".\\slicer\\" + "knifeinfo.txt " + path);
            //****************************************
        }
        catch {
            print("write error");
        }
    }
    public void visualize()
    {
        //****************************************************************//read shape info
        if (path == "") return;
        if (!loaded) return;
        if (shapes.Count > 0) return;
        shapes = new List<Shape>();
        System.IO.StreamReader file = new System.IO.StreamReader(path+"shapeinfo.txt");
        string line = file.ReadLine(); string[] items = line.Split(' ');
        shapenum = int.Parse(items[0]);
        for (int i = 0; i < shapenum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            Shape shape = new Shape(new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2])));
            shape.setMesh(path + "obj\\" ,"shape_" + i);
            shapes.Add(shape);
            //Tool.setColor(shape.instance, Tool.RandomBrightColor());
            Tool.setColor(shape.instance, Tool.RandomColor());
        }
        //****************************************************************//put Space
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach (Shape shape in shapes) {
            foreach (Vector3 vert in shape.instance.GetComponent<MeshFilter>().mesh.vertices) {
                Tool.cmpVert(vert, ref min, ref max);
            }
        }
        Cube = Instantiate(Resources.Load("transcube") as GameObject);
        MeshFilter mf = Cube.GetComponent<MeshFilter>();
        Cube.transform.localScale = (max - min)*1.0001f;Cube.transform.position = (max + min) / 2;
        Cube.name = "space";
        Cube.transform.parent = GameObject.Find("Collect").transform;
        Destroy(Cube.GetComponent<BoxCollider>());
        Cube.SetActive(false);
        //****************************************************************//genKnifeGo
        knifeGos = new List<GameObject>();
        file = new System.IO.StreamReader(path + "knifeinfo.txt");
        while (file.Peek() >= 0)
        {
            line = file.ReadLine();
            items = line.Split(' ');
            int i =int.Parse(items[0]);
            Edge edge = ThinStructure.edges[i];
            Quaternion fromto = Quaternion.LookRotation(ThinStructure.splitNorms[i], edge.vec);
            ThinStructure.splitNorms[i].Normalize();
            Vector3 cent = Vector3.Dot(edge.cent, ThinStructure.splitNorms[i]) * ThinStructure.splitNorms[i];
            GameObject plane = GameObject.Instantiate(Resources.Load("Plane4"), cent, fromto) as GameObject;
            plane.transform.parent = GameObject.Find("Assist").transform;
            Vector3 v1 = ThinStructure.vertices[edge.idx1];
            Vector3 v2 = ThinStructure.vertices[edge.idx2];
            float tuberadii = ThinStructure.tuberadii;
            plane.transform.localScale = new Vector3(200, 200, 200);
            knifeGos.Add(plane);
            plane.name = "knife";
            plane.SetActive(false);
        }
        //****************************************************************
        move();
    }
    public void showMovable()
    {
        moveableToggle = !moveableToggle;
        int movablenum = 0;
        for (int i = 0; i < shapes.Count; i++) {
            if (shapes[i].vec == Vector3.zero)
            {
                shapes[i].instance.SetActive(!moveableToggle);
                //Tool.setColor(shapes[i].instance, Color.red);
            }
            else
            {
                shapes[i].instance.SetActive(moveableToggle);
                //Tool.setColor(shapes[i].instance, Color.green);
                movablenum++;
            }
        }
        if(moveableToggle) Msg.text = "movable : " + movablenum + " / " + shapes.Count; 
        else Msg.text = "non-movable : " + (shapes.Count - movablenum) + " / " + shapes.Count;
    }
    public void reDraw() {
        for (int i = 0; i < shapes.Count; i++)
        {
            Tool.setColor(shapes[i].instance, Tool.RandomColor());
        }
    }
    public void move() {
        foreach (Shape shape in shapes) {
            if (shape.pos != Vector3.zero) shape.pos = Vector3.zero;
            else if(shape.vec != Vector3.zero){
                //shape.pos = shape.vec.normalized * Tool.getExCenter(shape.instance).magnitude * dismul;
                shape.pos = shape.vec.normalized * dismul;
            }
        }
    }
    public void tuneSplit() {
        ThinStructure.store();
        float[] angles = new float[ThinStructure.edgeNum];
        for (int i = 0; i < ThinStructure.edgeNum; i++) {
            angles[i] = Random.Range(0, 90);
        }
        print(ThinStructure.calTotalAngDif(angles));
        ThinStructure.applyAngles(angles);
        Tool.clearObj_immed();
        ThinStructure.basicPut();
        
    }
}
