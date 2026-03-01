using SnapTrace;

SnapTraceObserver.Initialize(new SnapOptions
{
    BufferSize = 10,
    RecordTimestamp = false,
    Output = message => Console.WriteLine(message)
});

var calc = new Calculator();
calc.Add(3, 5);

throw new InvalidOperationException("Test crash");

[SnapTrace]
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
