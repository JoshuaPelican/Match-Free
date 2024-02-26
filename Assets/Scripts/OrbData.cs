public struct OrbData
{
    public OrbType Type;
    public bool IsEmpty => Type == OrbType.None;

    public OrbData(OrbType type)
    {
        this.Type = type;
    }

    public override string ToString()
    {
        return $"{Type.ToString()[0]}";
    }

    public static readonly OrbData Empty = new OrbData(OrbType.None);
    public static OrbData Random => new OrbData((OrbType)UnityEngine.Random.Range(1, 7));
}
