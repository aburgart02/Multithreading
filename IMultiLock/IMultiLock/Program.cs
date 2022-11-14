using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;


namespace Task
{
    class Program
    {
        public static void Main()
        {

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