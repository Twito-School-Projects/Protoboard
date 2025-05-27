using System.Collections.Generic;

public class BreadboardSection
{
    public List<Hole> holes = new List<Hole>();
    public List<Hole> powerSources = new List<Hole>();

    public void RemovePowerSource(Hole holeToRemove)
    {
        powerSources.Remove(holeToRemove);
        if (powerSources.Count == 0)
        {
            foreach (var hole in holes)
            {
                hole.powered = false;
                hole.Debug_SetPoweredOffColour();
            }
        }
    }

    public void AddPowerSource(Hole holeToAdd)
    {
        powerSources.Add(holeToAdd);
        foreach (var hole in holes)
        {
            hole.powered = true;
            hole.Debug_SetPoweredOnColour();
        }
    }
}