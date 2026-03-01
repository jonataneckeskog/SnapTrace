using SnapTrace;

SnapTraceObserver.Initialize(new SnapOptions
{
    BufferSize = 10,
    RecordTimestamp = false,
    Output = message => Console.WriteLine(message)
});

var svc = new ReturnService();
svc.GetSecret();
svc.GetCoords();
svc.GetLabel();

throw new Exception("Done");

[SnapTrace]
public class ReturnService
{
    [return: SnapTraceIgnore]
    public string GetSecret()
    {
        return "top-secret-value";
    }

    [return: SnapTraceDeep]
    public Coords GetCoords()
    {
        return new Coords { X = 1, Y = 2 };
    }

    public Coords GetLabel()
    {
        return new Coords { X = 3, Y = 4 };
    }
}

public struct Coords
{
    public int X { get; set; }
    public int Y { get; set; }

    public override string ToString() => $"({X}, {Y})";
}
