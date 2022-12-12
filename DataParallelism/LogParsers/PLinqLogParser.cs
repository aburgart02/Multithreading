using LogParsing.LogParsers;
using System;
using System.IO;
using System.Linq;

namespace DataParallelismTask.LogParsers
{
    public class PLinqLogParser : ILogParser
    {
        private readonly FileInfo file;
        private readonly Func<string, string> tryGetIdFromLine;

        public PLinqLogParser(FileInfo file, Func<string, string> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }

        public string[] GetRequestedIdsFromLogFile()
        {
            var data = File.ReadLines(file.FullName);
            return data
                .AsParallel()
                .Select(tryGetIdFromLine)
                .Where(id => id != null)
                .ToArray();
        }
    }
}
