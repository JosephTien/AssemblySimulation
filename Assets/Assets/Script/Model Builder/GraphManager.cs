using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager {

    public List<Node> nodeList;
    public List<Line> lineList;
    public int currentIndex;

    public GraphManager()
    {
        nodeList = new List<Node>();
        lineList = new List<Line>();
        currentIndex = 0;
    }

    public string Output()
    {
        string text = "";
        text += nodeList.Count + " " + lineList.Count + "\n";
        foreach(Node node in nodeList)
        {
            text += node.Output() + "\n";
        }
        foreach(Line line in lineList)
        {
            text += line.Output() + "\n";
        }
        return text;
    }
}
