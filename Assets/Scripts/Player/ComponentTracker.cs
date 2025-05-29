using System.Collections;
using UnityEngine;

public class ComponentTracker : MonoBehaviour
{
    public static ComponentTracker Instance { get; private set; }

    public Battery battery = null;
    public Breadboard breadboard = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }
}
