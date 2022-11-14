using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Task
{
    class Program
    {
        public static void Main()
        {
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

            for (var i = 0; i < 10; i++)
            {
                Test1();
                Test2();
                Test3();
            }
        }
    }

    public class Disposable : IDisposable
    {
        private readonly object[] blockedObjects;

        public Disposable(object[] blockedObjects) =>
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
        private readonly static ConcurrentDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>();

        public IDisposable AcquireLock(params string[] keys)
        {
            object syncObj = new object();
            lock (syncObj)
            {
                lock (dictionary)
                {
                    foreach (var key in keys)
                        dictionary.GetOrAdd(key, syncObj);
                }

                var disposable = new Disposable(dictionary.Keys.ToArray());

                foreach (var blockedObject in dictionary.Keys.ToArray())
                    Monitor.Enter(blockedObject);
                Console.WriteLine("Объект заблокирован");
                return disposable;
            }
        }
    }
}