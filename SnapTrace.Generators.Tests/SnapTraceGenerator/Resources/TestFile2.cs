using SnapTrace;

namespace TestApp
{
    public class UserService
    {
        [SnapTraceContext]
        private string _userId = "12345";

        [SnapTrace]
        public void UpdateUser(string name) { }
    }

    public class Program
    {
        public static void Main()
        {
            new UserService().UpdateUser("Alice");
        }
    }
}