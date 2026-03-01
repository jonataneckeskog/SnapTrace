using SnapTrace;

SnapTraceObserver.Initialize(new SnapOptions
{
    BufferSize = 10,
    RecordTimestamp = false,
    Output = message => Console.WriteLine(message)
});

var service = new BankService();
service.Deposit("1234-SECRET", 500.00m);

throw new ArgumentException("Something went wrong!");

[SnapTrace]
public class BankService
{
    [SnapTraceContext]
    private decimal _currentBalance = 1000.00m;

    public void Deposit([SnapTraceIgnore] string pin, decimal amount)
    {
        _currentBalance += amount;
    }
}
