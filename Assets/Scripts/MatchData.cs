public struct MatchData
{
    public Shape Shape;
    public int X, Y;
    public OrbType Type;

    public MatchData(int x, int y, Shape shape, OrbType attribute)
    {
        X = x;
        Y = y;
        Shape = shape;
        Type = attribute;
    }

    public override string ToString()
    {
        return Shape.ToString();
    }
}
