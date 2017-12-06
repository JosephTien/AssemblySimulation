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
    public int coldis;
    public int id;
    //int order;
    public void setMesh(string path, string name){
        instance = GameObject.Instantiate(Resources.Load("Empty") as GameObject);
        Tool.inputMesh(instance, path + name + ".obj");
        Tool.indMesh(instance);
        instance.transform.parent = GameObject.Find("Collect").transform;
        instance.name = name;
        instance.AddComponent<MeshCollider>();
    }
    public void setVoxel(string path, string name)
    {
        instance = new GameObject(name);
        Tool.inputVoxel(instance, path + name + ".voxel");
        instance.transform.parent = GameObject.Find("Collect").transform;
        instance.name = name;
        instance.AddComponent<MeshCollider>();
    }
    public void initInst(string name) {
        instance = GameObject.Instantiate(Resources.Load("Empty") as GameObject);
        instance.transform.parent = GameObject.Find("Collect").transform;
        instance.name = name;
    }
    public void addZipPiece(Vector3 pos, Vector3 size, List<Vector3> point, List<Vector3> knifes) {
        GameObject go = GameObject.Instantiate(Resources.Load("Cube") as GameObject);
        MeshFilter mf = go.GetComponent<MeshFilter>();
        Vector3[] newvertices = new List<Vector3>(mf.mesh.vertices).ToArray();
        for(int i=0;i< mf.mesh.vertices.Length; i++) {
            newvertices[i] = (new Vector3(newvertices[i].x * size.x, newvertices[i].y * size.y, newvertices[i].z * size.z) + pos);
        }
        mf.mesh.vertices = newvertices;
        GameObject toDelete = GameObject.Find("ToDelete");
        for (int i = 0; i < point.Count; i++) {
            GameObject[] newbies = BLINDED_AM_ME.MeshCut.Cut(go, point[i], knifes[i], null);
            go = newbies[1];
            newbies[0].transform.parent = toDelete.transform;
        }
        go.transform.parent = instance.transform;
        GameObject.Destroy(go.GetComponent<BoxCollider>());
        go.AddComponent<MeshCollider>();
        go.name = "part";
    }
}
public class Visualization : MonoBehaviour {

    UnityEngine.UI.Dropdown Dropdown;
    UnityEngine.UI.Text Msg;
    UnityEngine.UI.Slider Slider;
    UnityEngine.UI.Slider Slider2;
    UnityEngine.UI.Slider Slider3;
    List<GameObject> knifeGos;
    GameObject Grid;
    GameObject Assist;
    //**************************************
    void Start() {
        Dropdown = GameObject.Find("Canvas/Dropdown").GetComponent<UnityEngine.UI.Dropdown>();
        foreach (DirectoryInfo file in (new DirectoryInfo(".\\data\\")).GetDirectories()) {
            Dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { text = file.Name });
        }
        ThinStructure.tuberadii = 5.0f;
        Msg = GameObject.Find("Canvas/Msg").GetComponent<UnityEngine.UI.Text>();
        Slider = GameObject.Find("Animation_Slider").GetComponent<UnityEngine.UI.Slider>();
        Slider2 = GameObject.Find("Animation_Slider2").GetComponent<UnityEngine.UI.Slider>();
        Slider3 = GameObject.Find("CalInit/Slider").GetComponent<UnityEngine.UI.Slider>();
        Assist = GameObject.Find("Assist");
    }
    int atar = 0;
    bool moviemode = true;
    public bool assemMode = false;

    Vector3 mouseStart;
    bool mouseDown = false;
    Vector3 xaxis = new Vector3(0, 1, 0);
    Vector3 yaxis = new Vector3(1, 0, 0);
    float moveRate = 0.25f;

    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                print(hit.collider.name);
                string[] strs = hit.collider.name.Split('_');
                if (strs[0] == "part") strs = hit.collider.transform.parent.name.Split('_');
                if (strs[0] == "shape") {
                    int idx = int.Parse(strs[1]);
                    for (int i = 0; i < shapes.Count; i++) {
                        if (i != idx) shapes[i].instance.SetActive(false);
                    }
                    shapes[idx].instance.SetActive(true);
                    Cube.SetActive(true);
                    Msg.text = "Pick " + strs[0] + "_" + strs[1];
                }
                if (strs[0] == "Edge")
                {
                    atar = int.Parse(strs[1]);
                    print(atar);
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
                if (strs[0] == "part") strs = hit.collider.transform.parent.name.Split('_');
                if (strs[0] == "shape")
                {
                    int idx = int.Parse(strs[1]);
                    for (int i = 0; i < shapes.Count; i++)
                    {
                        if (i != idx) shapes[i].instance.SetActive(true);
                    }
                    shapes[idx].instance.SetActive(false);
                    Cube.SetActive(true);
                    Msg.text = "unPick " + strs[0] + "_" + strs[1];
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
        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            ThinStructure.scale(2);
            Tool.clearObj_immed();
            ThinStructure.basicPut();
            ThinStructure.setColor(Color.gray);
            scaleval *= 2;
            Msg.text = "scale " + scaleval;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
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
            if (!moviemode)
            {
                Slider.value -= 0.1f;
                Slider2.value = 0;
            }
            else
            {
                Slider.value = 0;
                Slider2.value -= 1.0f / shapes.Count;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!moviemode)
            {
                Slider.value += 0.1f;
                Slider2.value = 0;
            }
            else
            {
                Slider.value = 0;
                Slider2.value += 1.0f / shapes.Count;
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ThinStructure.restore();
            ThinStructure.angles[atar] += 5;
            print(ThinStructure.calTotalAngDif(ThinStructure.angles));
            ThinStructure.applyAngles(ThinStructure.angles);
            Tool.clearObj_immed();
            ThinStructure.basicPut();
            showPlane();

        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ThinStructure.restore();
            ThinStructure.angles[atar] -= 5;
            print(ThinStructure.calTotalAngDif(ThinStructure.angles));
            ThinStructure.applyAngles(ThinStructure.angles);
            Tool.clearObj_immed();
            ThinStructure.basicPut();
            showPlane();

        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            showTest();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) Ctrl.setView(0);
        /*
        if (Input.GetKeyDown(KeyCode.Alpha8)) Ctrl.setView(7);
        if (Input.GetKeyDown(KeyCode.Alpha1)) Ctrl.setView(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Ctrl.setView(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Ctrl.setView(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Ctrl.setView(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) Ctrl.setView(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) Ctrl.setView(6);
        if (Input.GetKeyDown(KeyCode.I)) { Ctrl.rotateView(0, 90); }
        if (Input.GetKeyDown(KeyCode.J)) { Ctrl.rotateView(90, 0); }
        if (Input.GetKeyDown(KeyCode.K)) { Ctrl.rotateView(0, -90); }
        if (Input.GetKeyDown(KeyCode.L)) { Ctrl.rotateView(-90, 0); }
        */
        if (Input.GetKeyDown(KeyCode.T)) { tunemove.y += l; tuneGrid(); }
        if (Input.GetKeyDown(KeyCode.F)) { tunemove.x -= l; tuneGrid(); }
        if (Input.GetKeyDown(KeyCode.G)) { tunemove.y -= l; tuneGrid(); }
        if (Input.GetKeyDown(KeyCode.H)) { tunemove.x += l; tuneGrid(); }
        if (Input.GetKeyDown(KeyCode.R)) { tunemove.z -= l; tuneGrid(); }
        if (Input.GetKeyDown(KeyCode.Y)) { tunemove.z += l; tuneGrid(); }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Slider3.value++;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Slider3.value--;
        }
        //**************************************
        Tool.clearTodelete();
    }
    void FixedUpdate()
    {
        if (shapes == null) return;
        float slidervalue = Slider.value;
        float slider2value = Slider2.value;
        int sn = shapes.Count;
        if (!assemMode) {
            int i = 0;
            foreach (Shape shape in shapes)
            {
                if (shape.instance == null) continue;
                if (moviemode)
                {
                    slidervalue = (slider2value * sn - i);
                    if (slidervalue < 0) slidervalue = 0;
                    else if (slidervalue > 1) slidervalue = 1;

                }
                Vector3 tarPos = shape.pos * slidervalue;
                Vector3 curpos = shape.instance.transform.position;
                Vector3 vec = (tarPos - curpos) / timediv;
                if ((shape.instance.transform.position - tarPos).magnitude < disthre) shape.instance.transform.position = tarPos;
                else
                {
                    if (vec.magnitude < disthre)vec = vec.normalized * disthre;
                    shape.instance.transform.position += vec;
                    Msg.text = "shape_" + shape.id + ", Dis : " + shape.coldis;
                }
                i++;
            }
        }
        else {
            Msg.text = "";
            for (int i = 0; i < sn; i++) {
                int ii = assemOrder[i];
                if (ii == -1) break;
                Shape shape = shapes[ii];
                if (shape.instance == null) continue;
                if (moviemode)
                {
                    slidervalue = (slider2value * sn - i);
                    if (slidervalue < 0) slidervalue = 0;
                    else if (slidervalue > 1) slidervalue = 1;
                    //else print(i + " " + ii);
                }
                Vector3 tarPos = new Vector3(0, 0, 500) * slidervalue;
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
       
        //******************************************************
        if (isRotateMode) {
            if (Input.GetMouseButton(0))
            {
                mouseDown = true;
            }
            else
            {
                if (mouseDown)
                {
                    ThinStructure.applyRot(Assist.transform.rotation);
                    putCube();
                }
                mouseDown = false;
            }
            if (mouseDown)
            {
                Vector3 mouseDelta = Input.mousePosition - mouseStart;
                Quaternion assistrot = Assist.transform.rotation;
                Quaternion rotx = Quaternion.AngleAxis(-mouseDelta.x * moveRate, xaxis);
                Quaternion roty = Quaternion.AngleAxis(mouseDelta.y * moveRate, yaxis);
                Quaternion rot = rotx * roty;
                Assist.transform.rotation = rot * assistrot;
            }
            mouseStart = Input.mousePosition;
        }
    }
    //**************************************
    float timediv = 10;
    float disthre = 0.5f;
    float dismul = 300;
    float scaleval = 1;
    bool isRotateMode = false;
    //**************************************
    string path = "";
    bool loaded = false;
    GameObject Cube;
    List<Shape> shapes;
    int[] assemOrder;
    int shapenum;
    Vector3 max, min;
    bool moveableToggle = false;
    bool voxelMode = true;
    bool zipMode = true;
    bool grided = false;
    Vector3 tunemove;
    //**************************************
    Vector3 ld = Vector3.zero, ru = Vector3.zero, cent, size;
    Vector3 ld_, ru_;
    float radii = 5;
    float l = 2;
    int nx, ny, nz;
    int stx, sty, stz;
    int ntx, nty, ntz;
    //**************************************
    public void showTest() {
        Tool.clearObj_immed();
        shapes = new List<Shape>();
        List<int> shapeid = new List<int>();
        foreach (FileInfo file in (new DirectoryInfo(".\\data\\Test\\voxel\\")).GetFiles())
        {
            Shape shape = new Shape(new Vector3(0,0,0));
            if (file.Name.Split('.')[1].CompareTo("voxel")!=0) continue;
            shape.setVoxel(".\\data\\Test\\voxel\\", file.Name.Split('.')[0]);
            shapes.Add(shape);
            shapeid.Add(int.Parse(file.Name.Split('.')[0].Split('_')[1]));
            Tool.setColor(shape.instance, Tool.RandomColor());
        }
        int cnt = 0;
        List<Shape> shapes_ = new List<Shape>(shapeid.Count);
        foreach (int sh in shapeid) shapes_.Add(null);
        foreach (int sh in shapeid) {
            print(sh);
            shapes_[sh] = shapes[cnt++];
        }
        shapes = shapes_;
        if (Cube != null) Destroy(Cube);
         Cube = Instantiate(Resources.Load("transcube") as GameObject);
        //**************************************************************
        System.IO.StreamReader info = new System.IO.StreamReader(".\\data\\Test\\" + "shapeinfo.txt");
        string line = info.ReadLine(); string[] items = line.Split(' ');
        shapenum = int.Parse(items[0]);
        for (int i = 0; i < shapenum; i++)
        {
            line = info.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            shapes[i].vec = new Vector3(float.Parse(items[0]), float.Parse(items[1]),/*-1 * */ float.Parse(items[2]));
            print(shapes[i].vec);
        }
        move();
    }
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
        grided = false;
    }
    public void writeData() {
        if (!loaded) return;
        ThinStructure.outputThin(path);
        ThinStructure.outputsplitNorms_ori(path);
        ThinStructure.outputAngles(path);
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
    public void copyInfoTo() {
        Executor.RunCmd("copy /Y " + path + "thinstruct.txt .\\slicer\\");
        Executor.RunCmd("copy /Y " + path + "splitinfo.txt .\\slicer\\");
        Executor.RunCmd("copy /Y " + path + "rotatearg.txt .\\slicer\\");
        Executor.RunCmd("copy /Y " + path + "boundinfo.txt .\\slicer\\");
    }
    public void copyInfoFrom()
    {
        Executor.RunCmd("copy /Y " + ".\\slicer\\" + "shapeinfo.txt " + path);
        Executor.RunCmd("copy /Y " + ".\\slicer\\" + "pieceinfo.txt " + path);
        Executor.RunCmd("copy /Y " + ".\\slicer\\" + "knifeinfo.txt " + path);
        Executor.RunCmd("copy /Y " + ".\\slicer\\" + "rotatearg.txt " + path);
    }
    public void optimal() {
        try
        {
            writeData();
            copyInfoTo();
            //if(voxelMode) Executor.RunCmd("cd slicer&Slicer.exe voxel", true);
            if (voxelMode) Executor.RunCmd("cd slicer&Slicer.exe field", true);
            else Executor.RunCmd("cd slicer&Slicer.exe", true);
            //*********************************************
            if (!zipMode) {
                System.IO.StreamReader file = new System.IO.StreamReader(".\\slicer\\shapeinfo.txt");
                string line = file.ReadLine(); string[] items = line.Split(' ');
                shapenum = int.Parse(items[0]);
                file.Close();
                for (int i = 0; i < shapenum; i++)
                {
                    if (voxelMode) Executor.appendCmdSB("copy /Y " + ".\\slicer\\pool\\" + "shape_" + i + ".voxel" + " " + path + "voxel\\"); //Executor.RunCmd("copy /Y " + ".\\slicer\\pool\\" + "shape_" + i + ".obj" + " " + path + "obj\\");
                    else Executor.appendCmdSB("copy /Y " + ".\\slicer\\pool\\" + "shape_" + i + ".obj" + " " + path + "obj\\"); //Executor.RunCmd("copy /Y " + ".\\slicer\\pool\\" + "shape_" + i + ".obj" + " " + path + "obj\\");

                }
                Executor.flushCmdSB("~");
            }
            //****************************************
            copyInfoFrom();
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
        System.IO.StreamReader file = new System.IO.StreamReader(path + "shapeinfo.txt");
        string line = file.ReadLine(); string[] items = line.Split(' ');
        shapenum = int.Parse(items[0]);
        for (int i = 0; i < shapenum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            Shape shape = new Shape(new Vector3(float.Parse(items[0]), float.Parse(items[1]),/*-1 * */ float.Parse(items[2])));
            shape.coldis = int.Parse(items[3]);
            shape.id = i;
            if (!zipMode) {
                if (!voxelMode) shape.setMesh(path + "obj\\", "shape_" + i);
                else shape.setVoxel(path + "voxel\\", "shape_" + i);
                Tool.setColor(shape.instance, Tool.RandomColor());
            }
            shapes.Add(shape);
            //Tool.setColor(shape.instance, Tool.RandomBrightColor());
        }
        assemOrder = new int[shapenum];
        for (int i = 0; i < shapenum; i++) {
            if (file.EndOfStream)
            {
                assemOrder[i] = -1;
                continue;
            }
            line = file.ReadLine(); items = line.Split(' ');
            assemOrder[i] = int.Parse(items[0]);
            //print(assemOrder[i]);
        }
        file.Close();
        //****************************************************************//zip mode
        if (zipMode) {
            file = new System.IO.StreamReader(path + "pieceinfo.txt");
            line = file.ReadLine(); items = line.Split(' ');
            shapenum = int.Parse(items[0]);
            for (int i = 0; i < shapenum; i++)
            {
                shapes[i].initInst("shape_" + i);
                line = file.ReadLine(); items = line.Split(' ');
                int pieceNum = int.Parse(items[0]);
                for (int j = 0; j < pieceNum; j++) {
                    line = file.ReadLine(); items = line.Split(' ');
                    Vector3 pos = new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
                    Vector3 size = new Vector3(float.Parse(items[3]), float.Parse(items[4]), float.Parse(items[5]));
                    line = file.ReadLine(); items = line.Split(' ');
                    List<Vector3> points = new List<Vector3>();
                    List<Vector3> knifes = new List<Vector3>();
                    if (items.Length >= 6) {
                        for (int k = 0; k < items.Length-1; k += 6)
                        {
                            Vector3 point = new Vector3(float.Parse(items[k]), float.Parse(items[k + 1]), float.Parse(items[k + 2]));
                            Vector3 knife = new Vector3(float.Parse(items[k + 3]), float.Parse(items[k + 4]), float.Parse(items[k + 5]));
                            bool valid = true;
                            for (int a = 0; a < points.Count; a++)
                            {
                                Vector3 point_ = points[a];
                                Vector3 knife_ = knifes[a];
                                if (Vector3.Dot(knife.normalized, knife_.normalized) > Mathf.Cos(5 * Mathf.PI / 180)) {
                                    if (Mathf.Abs(Vector3.Dot((point_ - point), knife.normalized)) < 0.1f) {
                                        valid = false;
                                        break;
                                    }
                                }
                            }
                            if (valid) {
                                points.Add(point);
                                knifes.Add(knife);
                            }
                        }
                    }
                    shapes[i].addZipPiece(pos, size, points, knifes);
                }
                Destroy(shapes[i].instance.GetComponent<MeshRenderer>());
                Destroy(shapes[i].instance.GetComponent<MeshFilter>());
            }
            file.Close();
        }
        //****************************************************************//put Space
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach (Shape shape in shapes) {
            foreach (MeshFilter meshfilter in shape.instance.GetComponentsInChildren<MeshFilter>()) {
                foreach (Vector3 vert in meshfilter.mesh.vertices)
                {
                    Tool.cmpVert(vert, ref min, ref max);
                }
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
        file.Close();
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
    public void toggleMoveMode() {
        moviemode = !moviemode;
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
        writeData();
        copyInfoTo();
        Executor.RunCmd("cd slicer&Slicer.exe tune", true);
        copyInfoFrom();
        loadSplitAngle();
    }
    public void tuneSplit_less()
    {
        writeData();
        copyInfoTo();
        //Executor.RunCmd("cd slicer&Slicer.exe less", true);
        Executor.RunCmd("cd slicer&Slicer.exe less", true);
        copyInfoFrom();
        loadSplitAngle();
    }
    public void tuneSplit_org()
    {
        writeData();
        copyInfoTo();
        //Executor.RunCmd("cd slicer&Slicer.exe less", true);
        Executor.RunCmd("cd slicer&Slicer.exe org", true);
        copyInfoFrom();
        loadSplitAngle();
    }
    public void initSplit()
    {
        ThinStructure.angles = new float[ThinStructure.edgeNum];
        ThinStructure.restore();
        Tool.clearObj_immed();
        ThinStructure.basicPut();
        ThinStructure.setColor(Color.gray);
        showPlane();
    }
    public void loadSplitAngle()
    {
        ThinStructure.basicRead(path);//store, read angles
        print(ThinStructure.calTotalAngDif(ThinStructure.angles));
        ThinStructure.applyAngles(ThinStructure.angles);
        Tool.clearObj_immed();
        ThinStructure.basicPut();
        ThinStructure.setColor(Color.gray);
        showPlane();
    }
    public void tuneGrid() {
        int value = (int)Slider3.value;
        stx = sty = stz = value;
        nx = (int)((ru.x - ld.x + l) / l);
        ny = (int)((ru.y - ld.y + l) / l);
        nz = (int)((ru.z - ld.z + l) / l);
        ntx = (nx + stx - 1) / stx;
        nty = (ny + sty - 1) / sty;
        ntz = (nz + stz - 1) / stz;
        nx = ntx * stx;
        ny = nty * sty;
        nz = ntz * stz;
        //Vector3 tune = new Vector3(ld.x + nx * l, ld.y + ny * l, ld.z + nz * l) - ru;
        //tunemove.x = tunemove.x % (stx * l);
        //tunemove.y = tunemove.y % (stx * l);
        //tunemove.z = tunemove.z % (stx * l);
        //ld_ = ld + tunemove - new Vector3(l * stx, l * sty, l * stz);
        //**************************************************************
        ld_ = ld + tunemove;
        ru_ = new Vector3(ld_.x + nx * l, ld_.y + ny * l, ld_.z + nz * l);
        while (ru_.x < ru.x) ru_.x += stx * l; while (ru_.y < ru.y) ru_.y += sty * l; while (ru_.z < ru.z) ru_.z += stz * l;
        while (ld_.x > ld.x) ld_.x -= stx * l; while (ld_.y > ld.y) ld_.y -= sty * l; while (ld_.z > ld.z) ld_.z -= stz * l;
        while (ru_.x > ru.x + stx * l) ru_.x -= stx * l; while (ru_.y > ru.y + sty * l) ru_.y -= sty * l; while (ru_.z > ru.z + stz * l) ru_.z -= stz * l;
        while (ld_.x < ld.x - stx * l) ld_.x += stx * l; while (ld_.y < ld.y - sty * l) ld_.y += sty * l; while (ld_.z < ld.z - stz * l) ld_.z += stz * l;
        cent = (ld_ + ru_) / 2;
        size = (ru_ - ld_) + new Vector3(l, l, l);
        //**************************************************************
        nx = (int)((ru_.x - ld_.x + 0.1) / l);
        ny = (int)((ru_.y - ld_.y + 0.1) / l);
        nz = (int)((ru_.z - ld_.z + 0.1) / l);
        ntx = (nx + stx - 1) / stx;
        nty = (ny + sty - 1) / sty;
        ntz = (nz + stz - 1) / stz;
        nx = ntx * stx;
        ny = nty * sty;
        nz = ntz * stz;
        //**************************************************************
        if (Grid != null) Destroy(Grid);
        Grid = new GameObject("Grid");
        Grid.transform.parent = GameObject.Find("Collect").transform;
        Color color = Tool.RandomColor(); color.a = 1f;
        for (int i = 0; i < ntx - 1; i++)
        {
            Vector3 p = new Vector3(ld_.x + l * ((i + 1) * stx - 0.5f), cent.y, cent.z);
            Quaternion fromto = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(1, 0, 0));
            GameObject plane = GameObject.Instantiate(Resources.Load("Plane5"), p, fromto) as GameObject;
            plane.transform.parent = Grid.transform;
            plane.transform.localScale = new Vector3(size.y / 2, 1, size.z / 2);
            plane.name = "gridx";
            //Tool.setColor(plane, color);
        }
        color = Tool.RandomColor(); color.a = 1f;
        for (int i = 0; i < nty - 1; i++)
        {
            Vector3 p = new Vector3(cent.x, ld_.y + l * ((i + 1) * sty - 0.5f), cent.z);
            Quaternion fromto = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(0, 1, 0));
            GameObject plane = GameObject.Instantiate(Resources.Load("Plane5"), p, fromto) as GameObject;
            plane.transform.parent = Grid.transform;
            plane.transform.localScale = new Vector3(size.x / 2, 1, size.z / 2);
            plane.name = "gridy";
            //Tool.setColor(plane, color);
        }
        color = Tool.RandomColor(); color.a = 1f;
        for (int i = 0; i < ntz - 1; i++)
        {
            Vector3 p = new Vector3(cent.x, cent.y, ld_.z + l * ((i + 1) * stz - 0.5f));
            Quaternion fromto = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
            GameObject plane = GameObject.Instantiate(Resources.Load("Plane5"), p, fromto) as GameObject;
            plane.transform.parent = Grid.transform;
            plane.transform.localScale = new Vector3(size.x / 2, 1, size.y / 2);
            plane.name = "gridz";
            //Tool.setColor(plane, color);
        }
    }
    public void genGrid() {
        //*********************************************
        if (grided) {
            outoutGrid();
            grided = false;
            Destroy(Grid);
            return;
        }
        grided = true;
        ThinStructure.calBounding(out ru, out ld);
        ld -= new Vector3(radii * 5, radii * 5, radii * 5);
        ru += new Vector3(radii * 5, radii * 5, radii * 5);
        tuneGrid();
    }
    public void outoutGrid() {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Format("{0} {1} {2} {3} {4} {5}\n", ld_.x, ld_.y, ld_.z, ru_.x, ru_.y, ru_.z));
        sb.Append(string.Format("{0} {1} {2}\n", stx, sty, stz));
        string filename = path + "boundinfo.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }
    public void rotateMode() {
        if (!isRotateMode) {
            Ctrl.setView(0);
            Ctrl.valid = false;
            isRotateMode = true;
            ThinStructure.store();
            putCube();
            foreach (GameObject go in ThinStructure.edgeGOs) go.transform.parent = Assist.transform;
            foreach (GameObject go in ThinStructure.verticeGOs) go.transform.parent = Assist.transform;
        }
        else if (isRotateMode)
        {
            Ctrl.setView(0);
            Ctrl.valid = true;
            isRotateMode = false;
            ThinStructure.store();
            Destroy(Cube);
            Assist.transform.rotation = Quaternion.identity;
            Tool.clearObj_immed();
            ThinStructure.basicPut();
        }
    }
    public void putCube() {
        if(Cube!=null)Destroy(Cube);
        Vector3 min, max;
        ThinStructure.calBounding(out min, out max);
        Cube = Instantiate(Resources.Load("transcube") as GameObject);
        Cube.transform.localScale = (max - min) * 1.0001f; Cube.transform.position = (max + min) / 2;
        Cube.name = "space";
        Cube.transform.parent = GameObject.Find("Collect").transform;
        Destroy(Cube.GetComponent<BoxCollider>());
    }
    public void toggleAssemMode()
    {
        assemMode = GameObject.Find("Canvas/AssemMode").GetComponent<UnityEngine.UI.Toggle>().isOn;
    }
}
