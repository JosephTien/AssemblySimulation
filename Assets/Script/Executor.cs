using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

public class Executor : MonoBehaviour
{
    private Thread thread;
    private Thread thread2;
    private Process CSGCommandLineTool = new Process();
    static public  Process CMD = new Process();
    public static Queue<string> CSGCommands = new Queue<string>();
    string exeName = @"CSGCommandLineTool.exe";
    string exePath = @"CSGCommandLineTool\";
    public static Queue<string> ready = new Queue<string>();
    public static bool debug = false;

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

    public static void cpoyObjFileInPoolToInputSet(string name, int tarSet, string rename)
    {
        CMD.StandardInput.WriteLine("copy pool\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\");
        CMD.StandardInput.WriteLine("move ..\\inputSet\\" + tarSet + "\\inputObj\\" + name + ".obj ..\\inputSet\\" + tarSet + "\\inputObj\\" + rename + ".obj");
    }
    public static void deleteObjFileInPool(string name) {
        CMD.StandardInput.WriteLine("del pool\\"+name+".obj");
    }
    void startCMD() {
        CMD.StartInfo.FileName = "cmd.exe";
        CMD.StartInfo.WorkingDirectory = exePath;
        CMD.StartInfo.UseShellExecute = false;
        CMD.StartInfo.CreateNoWindow = false;
        CMD.StartInfo.RedirectStandardInput = true;
        CMD.StartInfo.RedirectStandardOutput = true;
        CMD.Start();
        CMD.StandardInput.WriteLine(" ");
    }
    void startCSGCommandLineTool() {
        //auto restart?
        CSGCommandLineTool.StartInfo.FileName = exePath + exeName;
        CSGCommandLineTool.StartInfo.WorkingDirectory = exePath;
        //myProcess.StartInfo.Arguments = "script.txt";
        CSGCommandLineTool.StartInfo.UseShellExecute = false;
        CSGCommandLineTool.StartInfo.CreateNoWindow = false;
        CSGCommandLineTool.StartInfo.RedirectStandardOutput = true;
        CSGCommandLineTool.StartInfo.RedirectStandardInput = true;
        CSGCommandLineTool.Start();
        CSGCommandLineTool.StandardInput.WriteLine(" ");
    }
    static public void csgcommand(string line)
    {
        CSGCommands.Enqueue(line);
    }
    private void Call()
    {
        while (true)
        {
            Thread.Sleep(333);
            bool isempty = true;
            foreach (string x in CSGCommands)
            {
                isempty = false;
                break;
            }
            if (!isempty)
            {
                string line = CSGCommands.Dequeue();
                CSGCommandLineTool.StandardInput.WriteLine(line);
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
    private void OnDisable()
    {
        try
        {
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
}
