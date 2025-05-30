using System.Collections;
using UnityEngine;

public class ComponentTracker : Singleton<ComponentTracker>
{
    public Battery battery = null;
    public Breadboard breadboard = null;
}
