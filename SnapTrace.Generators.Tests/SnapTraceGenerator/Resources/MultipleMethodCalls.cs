using SnapTrace;

namespace TestApp
{
    public class MyService
    {
        [SnapTrace]
        public void DoWork(string input)
        {
            Console.WriteLine(input);
        }
    }

    public class Program
    {
        public static void Main()
        {
            var service = new MyService();
            service.DoWork("Hello");
            service.DoWork("World");
        }
    }
}
