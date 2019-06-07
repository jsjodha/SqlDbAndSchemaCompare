using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SQLDbCompare
{
    public class FileHashKeycompare
    {



        string sourceFilePath = @"C:\Temp\HashKeyMap.src";
        string targetFilePath = @"C:\Temp\HashKeyMap.trg";
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        char splitString = ',';
        int batch_size = 100000;
        int max_diff_to_skip = 100;
        DateTime startTime = DateTime.Now;
        object locker = new object();
        string outputFilePath = @"C:\Temp\HashKeyMap_" + DateTime.Now.ToString("yyMMdd_HHmmsstt") + ".rs";
        StreamWriter outputWriter;
        long totalMS = 0;
        List<object> results = new List<object>();
        private void Run()
        {
            outputWriter = new StreamWriter(outputFilePath, false);
            try
            {

                startTime.ToString().Dump("Started At");




                int batchNumber = 0;
                var sourceLength = GetTotalRecordsCount(sourceFilePath);
                sourceLength.Dump("Source File Records Count");
                var targetLength = GetTotalRecordsCount(targetFilePath);
                targetLength.Dump("Target File Records Count");
                int totalBatchesToRun = sourceLength / batch_size;

                Console.WriteLine("Total Records diff by length {0}", (sourceLength - targetLength));

                Console.WriteLine("Total batch to run.{0}", totalBatchesToRun);




                int index = 0;
                //totalBatchesToRun =1;
                for (int i = 0; i < totalBatchesToRun; i++)
                {
                    sw.Start();

                    var sourceRecords = GetRecords(sourceFilePath, batchNumber, batch_size);
                    var targetRecords = GetRecords(targetFilePath, batchNumber, batch_size);

                    var rs = from s in sourceRecords
                             join t in targetRecords
                             on s.Key equals t.Key
                             where s.Value != t.Value
                             select new
                             {
                                 SourceKey = s.Key,
                                 TargetKey = t.Key,
                                 SourceHash = s.Value,
                                 TargetHash = t.Value,
                             };

                    results.AddRange(rs);
                    sw.Stop();
                    long batchTimeTook = sw.ElapsedMilliseconds;
                    totalMS += batchTimeTook;
                    ("Starting batch code :" + i + " / " + totalBatchesToRun + "  batch starting at " + batchNumber + " in " + sw.ElapsedMilliseconds + " ms").Dump("");
                    sw.Reset();
                    batchNumber += batch_size;
                }


            }
            finally
            {
                //rs.Dump("Diff records in Batch:" + batchNumber);
                foreach (var r in results)
                {
                    //r.Dump("Record");
                    lock (locker)
                    {
                        outputWriter.WriteLine(r);
                    }
                }

                var diffrecords = System.IO.File.ReadLines(outputFilePath).Count();
                Console.WriteLine("Diff records :{0}", diffrecords);
                outputWriter.Close();
                outputWriter.Dispose();

                Console.WriteLine("Time Taken {0}ms", totalMS);
            }
        }
        public int GetTotalRecordsCount(string filepath)
        {
            return System.IO.File.ReadLines(filepath).Count();
        }
        public IEnumerable<KeyValuePair<string, string>> GetRecords(string filepath, int skip, int take)
        {
            return (from r in System.IO.File.ReadLines(filepath).Skip(skip).Take(take)
                    select new KeyValuePair<string, string>(
                    r.Split(splitString).First(),
                    r.Split(splitString).Last()))
                             .AsEnumerable<KeyValuePair<string, string>>();
        }


    }
}

