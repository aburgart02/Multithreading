using System.Diagnostics;
using System.Threading;

namespace Task
{
    class Program
    {
        public static void Payload()
        {
            var sum = 0;
            for (var i = 0; i < 10000; i++)
                sum += 1;
        }

        public static void Main()
        {
            var process = Process.GetCurrentProcess();
            var data = new List<Tuple<int, long>>();
            process.ProcessorAffinity = (IntPtr)Math.Pow(2, (Environment.ProcessorCount - 1));
            process.PriorityClass = ProcessPriorityClass.RealTime;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread thread1 = new Thread(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    lock (data)
                    {
                        Payload();
                        data.Add(Tuple.Create(1, stopwatch.ElapsedMilliseconds));
                    }
                }
            });
            Thread thread2 = new Thread(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    lock (data)
                    {
                        Payload();
                        data.Add(Tuple.Create(2, stopwatch.ElapsedMilliseconds));
                    }
                }
            });
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();
            GetQuantum(data);
        }

        public static void GetQuantum(List<Tuple<int, long>>  data)
        {
            int count = 0;
            long sum = 0;
            long time = 0;
            int threadNumber = data[0].Item1;
            foreach (var item in data)
            {
                if (item.Item1 != threadNumber)
                {
                    count++;
                    sum += (item.Item2 - time);
                    threadNumber = item.Item1;
                    time = item.Item2;
                }
            }
            Console.WriteLine(sum / count);
        }
    }
}