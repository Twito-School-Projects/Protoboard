using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Buzzer : ElectronicComponent
{
    [SerializeField]
    private AudioSource audioData;
    
    [SerializeField]
    private float delayBeforePlaying = 0.5f;
    
    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();
        if (anodeHole && cathodeHole)
            hasCircuitCompleted = anodeHole.wasPropagated && cathodeHole.wasPropagated;


        if (hasCircuitCompleted)
        {
            if (!audioData.isPlaying)
            {
                audioData.PlayScheduled(delayBeforePlaying);
            }
        }
        else
        {
            audioData.Stop();
        }
    }

    protected override Vector3 CalculateMidpoint()
    {
        return new Vector3(
            (cathodeHole.transform.position.x + anodeHole.transform.position.x) / 2,
            transform.position.y,
            cathodeHole.transform.position.z
        );
    }
}