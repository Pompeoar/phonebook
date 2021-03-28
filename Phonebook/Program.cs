using BenchmarkDotNet.Running;

namespace Phonebook
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<PhoneBookBenchmarks>();
        }
    }
}
