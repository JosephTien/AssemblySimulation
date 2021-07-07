using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IComponent{

    public Vector3 position;
    public int index;

    public Node(int index_, Vector3 position_)
    {
        index = index_;
        position = position_;
    }

    public override string Output()
    {
        return position.x + " " + position.y + " " + position.z;
    }

    public override int GetIndex()
    {
        return index;
    }
}
