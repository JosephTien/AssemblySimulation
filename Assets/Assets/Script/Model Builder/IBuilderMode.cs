using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBuilderMode {

    public IBuilderMode(GameObject prefab)
    {

    }

    public abstract void ClickLeftButton();
    public abstract void ClickRightButton();

    public abstract void MouseOnThePosition();

}
