using System;
using System.Linq;

public enum Charge
{ Positive, Negative, None }

[Serializable]
public class Rail : BreadboardSection
{
    public Charge charge = Charge.None;

    public Rail(Charge charge = Charge.None)
    {
        this.charge = charge;
    }
}