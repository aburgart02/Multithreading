using System.Diagnostics;

namespace Task
{
    class Program
    {
        private static Stopwatch stopwatch;

        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)Math.Pow(2, (Environment.ProcessorCount - 1));
            var data = new List<long>(10);
            for (var i = 0; i < 10; i++)
            {
                stopwatch = new Stopwatch();
                var thread1 = new Thread(Thread1StartStopwatch)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal,
                };
                thread1.Start();
                var thread2 = new Thread(Thread2StopWatch)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest,
                };
                thread2.Start();
                thread2.Join();
                thread1.Join();
                data.Add(stopwatch.ElapsedMilliseconds);
            }
            for (var i = 0; i < data.Count; i++)
                Console.WriteLine($"{i + 1}. {data[i]}");
            Console.WriteLine($"Average: {data.Average()}");
        }

        private static void Thread1StartStopwatch()
        {
            stopwatch.Start();
            while (stopwatch.IsRunning) { }
        }

        private static void Thread2StopWatch()
        {
            stopwatch.Stop();
        }
    }
}