using LogParsing.LogParsers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace DataParallelismTask.LogParsers
{
    public class ParallelLogParser : ILogParser
    {
        private readonly FileInfo file;
        private readonly Func<string, string> tryGetIdFromLine;

        public ParallelLogParser(FileInfo file, Func<string, string> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }

        public string[] GetRequestedIdsFromLogFile()
        {
            var data = File.ReadLines(file.FullName);
            var answer = new ConcurrentBag<string>();
            Parallel.ForEach(data, n =>
            {
                var id = tryGetIdFromLine(n);
                if (id != null)
                    answer.Add(id);
            });
            return answer.ToArray();
        }
    }
}
