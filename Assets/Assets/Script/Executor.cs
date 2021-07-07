using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

public class Executor : MonoBehaviour
{
    private Thread thread;
    private Process CSGCommandLineTool = new Process();
    static public  Process CMD = new Process();
    public static Queue<string> commands=new Queue<string>();
    string exeName = @"CSGCommandLineTool.exe";
    string exePath = @"CSGCommandLineTool\";

    void commandTest()
    {
        command("LOAD cube");
        command("COPY cube1 cube");
        command("WRITE cube1");
    }

    // Use this for initialization
    void Start()
    {
        thread = new Thread(Call);
        thread.IsBackground = true;
        startCSGCommandLineTool();
        startCMD();
        thread.Start();
        //commandTest();
    }
    // Update is called once per frame
    void Update()
    {
    }


    public static void deleteObjFileInPool(string name) {
        CMD.StandardInput.WriteLine("del pool\\"+name+".obj");
    }
    void startCMD() {
        CMD.StartInfo.FileName = "cmd.exe";
        CMD.StartInfo.WorkingDirectory = exePath;
        CMD.StartInfo.UseShellExecute = false;
        CMD.StartInfo.CreateNoWindow = true;
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
        //myProcess.StartInfo.RedirectStandardOutput = true;
        CSGCommandLineTool.StartInfo.RedirectStandardInput = true;
        CSGCommandLineTool.Start();
        CSGCommandLineTool.StandardInput.WriteLine(" ");
    }
    static public void command(string line)
    {
        commands.Enqueue(line);
    }
    private void Call()
    {
        while (true)
        {
            Thread.Sleep(333);
            bool isempty=true;
            foreach (string x in commands)
            {
                isempty = false;
                break;
            }
            if (!isempty)
            {
                string line = commands.Dequeue();
                print(line);
                CSGCommandLineTool.StandardInput.WriteLine( line );
            }
        }
    }
    private void OnDisable()
    {
        try
        {
            CSGCommandLineTool.Kill();
            CMD.Kill();
        }
        catch{
        }
        thread.Abort();//強制中斷當前執行緒
    }
    /*
    private void OnApplicationQuit()
    {
        thread.Abort();//強制中斷當前執行緒
        thread.Abort();
    }
    */
}
