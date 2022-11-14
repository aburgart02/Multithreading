using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;


namespace Task
{
    class Program
    {
        private static IMultiLock _multiLock2 = new MultiLock();

        public static void Main(string[] args)
        {
            void Test0()
            {
                var t1 = new Thread(x => UseResource(TimeSpan.FromSeconds(10), "1", "3"));
                var t2 = new Thread(x => UseResource(TimeSpan.FromSeconds(2), "4", "3"));
                var t3 = new Thread(x => UseResource(TimeSpan.FromSeconds(1), "4", "5"));
                t1.Name = "1";
                t2.Name = "2";
                t3.Name = "3";
                t1.Start();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                t2.Start();
                t3.Start();
                t1.Join();
                t2.Join();
                t2.Join();
            }

            void Test1()
            {
                var testVariable = new[] { "1", "2", "3" };
                var testVariable2 = new[] { "1" };
                var testVariable3 = new[] { "2", "3" };
                var multilock = new MultiLock();
                var thread1 = new Thread(() =>
                {
                    using (multilock.AcquireLock(testVariable))
                    {
                        Thread.Sleep(1000);
                    }

                });
                var thread2 = new Thread(() =>
                {
                    Thread.Sleep(500);
                    using (multilock.AcquireLock(testVariable2))
                    {
                        Thread.Sleep(500);
                        using (multilock.AcquireLock(testVariable3))
                        {

                        }
                    }
                });
                thread1.Start();
                thread2.Start();
            }

            void Test2()
            {
                var testVariable = new[] { "b", "c" };
                var testVariable2 = new[] { "c", "b" };
                var multilock = new MultiLock();
                var thread1 = new Thread(() =>
                {
                    using (multilock.AcquireLock(testVariable))
                    {

                    }

                });
                var thread2 = new Thread(() =>
                {
                    using (multilock.AcquireLock(testVariable2))
                    {

                    }
                });
                thread1.Start();
                thread2.Start();
            }

            void Test3()
            {
                var testVariable = new[] { "a" };
                var testVariable2 = new[] { "b" };
                var testVariable3 = new[] { "c" };
                var multilock = new MultiLock();
                var thread1 = new Thread(() =>
                {
                    using (multilock.AcquireLock(testVariable2))
                    {

                    }

                });
                var thread2 = new Thread(() =>
                {
                    Thread.Sleep(500);
                    using (multilock.AcquireLock(testVariable3))
                    {

                    }
                });
                var thread3 = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    using (multilock.AcquireLock(testVariable2))
                    {
                        using (multilock.AcquireLock(testVariable3))
                        {

                        }
                    }
                });
                var thread4 = new Thread(() =>
                {
                    Thread.Sleep(3000);
                    using (multilock.AcquireLock(testVariable2))
                    {

                    }
                });
                thread1.Start();
                thread2.Start();
                thread3.Start();
                thread4.Start();
            }

            Test0();
            for (var i = 0; i < 10; i++)
            {
                Test1();
                Test2();
                Test3();
            }
        }


        private static void UseResource(TimeSpan fromSeconds, params string[] keys)
        {
            // Wait until it is safe to enter.
            Console.WriteLine("{0} is requesting the mutex",
                Thread.CurrentThread.Name);
            var _lock = _multiLock2.AcquireLock(keys);
            //var m = new Mutex(false, "1");
            //m.WaitOne();
            Console.WriteLine("{0} has entered the protected area",
                Thread.CurrentThread.Name);

            // Place code to access non-reentrant resources here.

            // Simulate some work.
            Thread.Sleep(fromSeconds);

            Console.WriteLine("{0} is leaving the protected area",
                Thread.CurrentThread.Name);

            // Release the Mutex.
            _lock.Dispose();
            //m.ReleaseMutex();

            Console.WriteLine("{0} has released the mutex",
                Thread.CurrentThread.Name);
        }
    }

    public class Disposable : IDisposable
    {
        private readonly StringWrapper[] blockedObjects;

        public Disposable(IEnumerable<StringWrapper> blockedObjects) =>
            this.blockedObjects = blockedObjects.ToArray();

        public void Dispose()
        {
            foreach (var blockedObject in blockedObjects)
            {
                if (Monitor.IsEntered(blockedObject))
                    Monitor.Exit(blockedObject);
            }
            Console.WriteLine("Объект разблокирован");
        }
    }

    public interface IMultiLock
    {
        public IDisposable AcquireLock(params string[] keys);
    }

    public class MultiLock : IMultiLock
    {
        private readonly Dictionary<string, StringWrapper> stringWrappers = new Dictionary<string, StringWrapper>();

        public IDisposable AcquireLock(params string[] keys)
        {
            lock (stringWrappers)
            {
                foreach (var key in keys)
                    if (!stringWrappers.ContainsKey(key))
                        stringWrappers[key] = new StringWrapper(key);
            }

            var blockedObjects = keys
                .Select(i => stringWrappers[i])
                .OrderBy(i => i.String)
                .ToArray();

            var disposable = new Disposable(blockedObjects);

            var isError = true;
            try
            {
                foreach (var blockedObject in blockedObjects)
                    Monitor.Enter(blockedObject);
                Console.WriteLine("Объект заблокирован");
                isError = false;
            }
            finally
            {
                if (isError)
                    disposable.Dispose();
            }
            return disposable;
        }
    }

    public class StringWrapper
    {
        public string String { get; }
        public StringWrapper(string @string) => String = @string;
    }
}