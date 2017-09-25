using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text;

public class Executor : MonoBehaviour
{
    public static StringBuilder CmdSB = new StringBuilder();
    private Thread thread;
    private Thread thread2;
    private Process CSGCommandLineTool = new Process();
    static public  Process CMD = new Process();
    public static Queue<string> CSGCommands = new Queue<string>();
    public static int CSGCommandsCnt = 0;
    static string exeName = @"CSGCommandLineTool.exe";
    static string exePath = @"CSGCommandLineTool\";
    public static Queue<string> ready = new Queue<string>();
    public static bool debug = false;
    StringBuilder log = new StringBuilder();
    public static string curCommand="/ /";//隨便打的
    public static bool allinone = true;
    public static bool manualCopy = true;

    void commandTest()
    {
        csgcommand("LOAD cube");
        csgcommand("COPY cube_1 cube");
        csgcommand("LOAD sphere");
        csgcommand("COPY sphere_1 sphere");
        csgcommand("NEW new_");
        csgcommand("+ new_ sphere_1 cube_1");
        csgcommand("WRITE new_");
    }

    // Use this for initialization
    void Start()
    {
        thread = new Thread(Call);
        thread.IsBackground = true;
        thread2 = new Thread(Call2);
        thread.IsBackground = true;
        startCSGCommandLineTool();
        startCMD();
        thread.Start();
        thread2.Start();
        //commandTest();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void appendCmdSB(string str) {
        CmdSB.Append(str + "\n");
    }

    public static void flushCmdSB(string filename) {
        using (StreamWriter sw = new StreamWriter(filename+".bat"))
        {
            sw.Write(CmdSB.ToString());
        }
        CmdSB = new StringBuilder();
        Executor.RunCmd(filename+".bat", false);
    }

    public static void clearInputSet(int tarSet) {
        //CMD.StandardInput.WriteLine("del ..\\inputSet\\" + tarSet + "\\inputObj\\*.obj");
        Bath.Add("del ..\\inputSet\\" + tarSet + "\\inputObj\\*.obj");
    }

    public static void clearInputSet_Cube(int tarSet)
    {
        //CMD.StandardInput.WriteLine("del ..\\inputSet\\" + tarSet + "\\inputObj\\*.obj");
        Bath.Add("del ..\\inputSet\\" + tarSet + "\\cubeObj\\*.obj");
    }

    public static List<string> Bath = new List<string>();
    public static List<string> CSG = new List<string>();

    public static void mkObjDir(int tarSet) {
        Bath.Add("mkdir ..\\inputSet\\" + tarSet + "\\inputObj\\");
    }
    public static void mkCubeObjDir(int tarSet)
    {
        Bath.Add("mkdir ..\\inputSet\\" + tarSet + "\\cubeObj\\");
    }

    public static void cpoyObjFileInPoolToInputSet(string name, int tarSet, string rename)
    {
        //CMD.StandardInput.WriteLine("copy pool\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\");
        //CMD.StandardInput.WriteLine("move ..\\inputSet\\" + tarSet + "\\inputObj\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\" + rename + ".obj");
        Bath.Add("copy pool\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\");
        Bath.Add("move ..\\inputSet\\" + tarSet + "\\inputObj\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\" + rename + ".obj");
    }

    public static void cpoyObjFileInPoolToInputSet_Cube(string name, int tarSet, string rename)
    {
        //CMD.StandardInput.WriteLine("copy pool\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\");
        //CMD.StandardInput.WriteLine("move ..\\inputSet\\" + tarSet + "\\inputObj\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\" + rename + ".obj");
        Bath.Add("copy pool\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\cubeObj\\");
        Bath.Add("move ..\\inputSet\\" + tarSet + "\\cubeObj\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\cubeObj\\" + rename + ".obj");
    }

    public static void deleteObjFileInPool(string name) {
        //CMD.StandardInput.WriteLine("del pool\\"+name+".obj");
        Bath.Add("del pool\\" + name + ".obj");
    }

    
    public static void flushBath() {
        StringBuilder sb = new StringBuilder();
        foreach (string str in Bath) {
            sb.Append(str + "\n");
        }
        string filename = exePath+"_.bat";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }

    public static void flushCSG_advance()
    {
        StringBuilder sb = new StringBuilder();
        foreach (GameObject go in Generator.Samples)
        {
            sb.Append("LOAD " + go.name + "\n");
        }
        foreach (MeshComponent mc in Pool.list)
        {
            if (mc.tfa != null)
            {
                string ori = mc.Name.Split('_')[0];
                sb.Append("COPY " + mc.Name + " " + ori + "\n");
                /*
                sb.Append("FORM-VEC " + mc.Name + " " + mc.Name + " " + mc.tfa.scale.x + " " + mc.tfa.scale.y + " " + mc.tfa.scale.z + " "
                                                                  + mc.tfa.trans.x + " " + mc.tfa.trans.y + " " + mc.tfa.trans.z + " "
                                                                  + mc.tfa.up.x + " " + mc.tfa.up.y + " " + mc.tfa.up.z + " "
                                                                  + mc.tfa.forw.x + " " + mc.tfa.forw.y + " " + mc.tfa.forw.z + " " + "\n");
                */
                sb.Append("FORM " + mc.Name + " " + mc.Name + " " + mc.tfa.scale.x + " " + mc.tfa.scale.y + " " + mc.tfa.scale.z + " "
                                                                  + mc.tfa.trans.x + " " + mc.tfa.trans.y + " " + mc.tfa.trans.z + " "
                                                                  + mc.tfa.rot.x + " " + mc.tfa.rot.y + " " + mc.tfa.rot.z + " " + "\n");
            }
        }
        foreach (string str in CSG)
        {
            string cs = str.Split(' ')[0];
            if (cs != "LOAD") sb.Append(str + "\n");
        }
        string filename = exePath + "script2.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }

    public static void flushCSG()
    {
        StringBuilder sb = new StringBuilder();
        foreach (string str in CSG)
        {
            sb.Append(str + "\n");
        }
        string filename = exePath + "script.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(sb.ToString());
        }
    }
    public void excuteBath() {
        /*
        Process TEMP = new Process();
        TEMP.StartInfo.FileName = exePath+"_.bat";
        TEMP.StartInfo.WorkingDirectory = exePath;
        TEMP.StartInfo.UseShellExecute = false;
        TEMP.StartInfo.CreateNoWindow = false;
        TEMP.StartInfo.RedirectStandardInput = true;
        TEMP.StartInfo.RedirectStandardOutput = true;
        TEMP.Start();
        TEMP.WaitForExit();
        TEMP.Close();
        */
        //CMD.StandardInput.WriteLine("_.bat");
    }
    void startCMD() {
        if (!manualCopy) {
            CMD.StartInfo.FileName = "cmd.exe";
            CMD.StartInfo.WorkingDirectory = exePath;
            CMD.StartInfo.UseShellExecute = false;
            CMD.StartInfo.CreateNoWindow = false;//to check if still there
            //CMD.StartInfo.CreateNoWindow = true;
            CMD.StartInfo.RedirectStandardInput = true;
            CMD.StartInfo.RedirectStandardOutput = true;
            CMD.Start();
            CMD.StandardInput.WriteLine(" ");
        }
    }
    void startCSGCommandLineTool() {
        if (!allinone) {
            //auto restart?
            CSGCommandLineTool.StartInfo.FileName = exePath + exeName;
            CSGCommandLineTool.StartInfo.WorkingDirectory = exePath;
            //myProcess.StartInfo.Arguments = "script.txt";
            CSGCommandLineTool.StartInfo.UseShellExecute = false;
            CSGCommandLineTool.StartInfo.CreateNoWindow = false;//to check if still there
                                                                //CSGCommandLineTool.StartInfo.CreateNoWindow = true;
            CSGCommandLineTool.StartInfo.RedirectStandardOutput = true;
            CSGCommandLineTool.StartInfo.RedirectStandardInput = true;
            CSGCommandLineTool.Start();
            CSGCommandLineTool.StandardInput.WriteLine(" ");
        }
    }
    static public void csgcommand(string line)
    {
        CSGCommands.Enqueue(line);
    }
    private void Call()
    {
        while (true)
        {
            Thread.Sleep(5);
            bool isempty = true;
            foreach (string x in CSGCommands)
            {
                isempty = false;
                break;
            }
            if (!isempty)
            {
                string line = CSGCommands.Dequeue();
                if (line.Split(' ')[0] != "WRITE" && line.Split(' ')[0] != "LOAD")CSGCommandsCnt++;
                log.Append(line + "\n");
                curCommand = line;
                if (!allinone) CSGCommandLineTool.StandardInput.WriteLine(line);
                else {//使用手動模式，強制回傳
                    CSG.Add(line);
                    if (line.Split(' ')[0] == "WRITE")
                    {
                        ready.Enqueue(line.Split(' ')[1]);
                    }
                }
            }
        }
    }
    private void Call2()
    {
        while (!CSGCommandLineTool.HasExited) {
            string output = CSGCommandLineTool.StandardOutput.ReadLine();
            string[] result = output.Split(' ');
            if (result[0] == "WRITE") {
                ready.Enqueue(result[1]);
            }
            if(debug)print("CMD : " + output);
        }
    }

    void writeLog()
    {
        string filename = exePath + "log.txt";
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(log.ToString());
        }
    }

    private void OnDisable()
    {
        try
        {
            writeLog();
            CSGCommandLineTool.Kill();
            CMD.Kill();
            thread.Abort();
            thread2.Abort();
        }
        catch{
        }
    }
    /*
    private void OnApplicationQuit()
    {
        thread.Abort();//強制中斷當前執行緒
        thread.Abort();
    }
    */
    public static bool RunCmd(string cmdStr) {
        return RunCmd(cmdStr, false);
    }
    public static bool RunCmd(string cmdStr, bool show)
    {
        bool result = false;
        try
        {
            using (Process myPro = new Process())
            {
                myPro.StartInfo.FileName = "cmd.exe";
                myPro.StartInfo.UseShellExecute = false;
                myPro.StartInfo.RedirectStandardInput = true;
                myPro.StartInfo.RedirectStandardOutput = !show;
                myPro.StartInfo.RedirectStandardError = true;
                myPro.StartInfo.CreateNoWindow = !show;
                //myPro.StartInfo.CreateNoWindow = false;
                myPro.Start();
                //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                string str = string.Format("{0} {1}", cmdStr, "&exit");

                myPro.StandardInput.WriteLine(" ");
                myPro.StandardInput.WriteLine(str);
                myPro.StandardInput.WriteLine("\n");
                myPro.StandardInput.AutoFlush = true;
                myPro.WaitForExit();

                result = true;
            }
        }
        catch
        {

        }
        return result;
    }

}
