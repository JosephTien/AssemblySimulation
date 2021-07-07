using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : IComponent {

    public int fromNode;
    public int toNode;

    public Line(int from, int to)
    {
        fromNode = from;
        toNode = to;
    }

    public override int GetIndex()
    {
        return -1;
    }

    public override string Output()
    {
        return fromNode + " " + toNode;
    }
}
