using System.Collections.Generic;
using Unity.VisualScripting;

public class BreadboardSection
{
    public List<Hole> holes = new List<Hole>();
    public List<Hole> powerSources = new List<Hole>();
    public bool Powered { get; set; }

    public void Update()
    {
        Powered = powerSources.Count > 0;
        foreach (var hole in holes)
        {
            hole.powered = Powered;
        }
    }
    public void RemovePowerSource(Hole holeToRemove)
    {
        powerSources.Remove(holeToRemove);
    }

    public void AddPowerSource(Hole holeToAdd)
    {
        powerSources.Add(holeToAdd);
    }
}