using UnityEngine;

public class BarObject
{
    public int barID;
    public GameObject barObjectPrimitive;
    public MaterialPropertyBlock mpb;
    public BarData barData;

    public BarObject(int iD, GameObject barObject, BarData rawData, MaterialPropertyBlock matPropertyBlock)
    {
        this.barID = iD;
        this.barObjectPrimitive = barObject;
        this.barData = rawData;
        this.mpb = matPropertyBlock;
    }
}
