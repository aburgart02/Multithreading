using LogParsing.LogParsers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace DataParallelismTask.LogParsers
{
    public class ThreadLogParser : ILogParser
    {
        private readonly FileInfo file;
        private readonly Func<string, string> tryGetIdFromLine;

        public ThreadLogParser(FileInfo file, Func<string, string> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }

        public string[] GetRequestedIdsFromLogFile()
        {
            var stack = new ConcurrentStack<string>(File.ReadLines(file.FullName));
            var answer = new ConcurrentBag<string>();
            var threads = new Thread[Environment.ProcessorCount * 3];

            for (var i = 0; i < threads.Length; i++)
            {
                var thread = new Thread(() =>
                {
                    while (stack.TryPop(out var result))
                    {
                        var id = tryGetIdFromLine(result);
                        if (id != null)
                            answer.Add(id);
                    }
                });

                thread.Start();
                threads[i] = thread;
            }

            foreach (var thread in threads)
                thread.Join();

            return answer.ToArray();
        }
    }
}
