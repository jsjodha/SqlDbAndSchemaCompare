using System;
using System.Data.SqlClient;
using System.IO;

namespace SQLDbCompare
{
    public abstract class BaseComparor : IObjectComparor
    {
        public string SourceConString { get; }
        public string TargetConString { get; }
        internal string _basePath = @"C:\Temp";
        internal string _workingFolder;
        internal string _targetWorkingPath;
        internal string _sourceWorkingPath;
        protected string AppVersion = DateTime.Now.ToString("yyyy.MM.dd.HHmmss");
        public BaseComparor(string sourceConString, string targetConString)
        {
            this.SourceConString = sourceConString;
            this.TargetConString = targetConString;
        }
        internal void WriteToFile(string baseFolder, string objectType, string objectName, string scriptText)
        {
            string folderPath = Path.Combine(baseFolder, objectType);
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, objectName + ".sql");
            Console.WriteLine("Writing Object Script to file {0}", filePath);
            scriptText = string.IsNullOrWhiteSpace(scriptText) ? "Object does not exists" : scriptText;
            System.IO.File.WriteAllText(filePath, scriptText);

        }

        internal string GetWorkingFolder()
        {
            if (!string.IsNullOrWhiteSpace(_workingFolder))
                return _workingFolder;

            var folderPath = System.IO.Path.Combine(_basePath, "SchemaCompare_" + DateTime.Today.ToString("dd_MMM_yyyy"));
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        internal string GetSourceWorkingFolder(string SourceConString)
        {
            if (!string.IsNullOrWhiteSpace(_sourceWorkingPath))
                return _sourceWorkingPath;

            var basePath = GetWorkingFolder();
            var cb = new SqlConnectionStringBuilder(SourceConString);
            var sourcePath = System.IO.Path.Combine(basePath, cb.DataSource, cb.InitialCatalog);

            if (!System.IO.Directory.Exists(sourcePath))
                System.IO.Directory.CreateDirectory(sourcePath);

            return sourcePath;
        }
        internal string GetTargetWorkingFolder(string TargetConString)
        {
            if (!string.IsNullOrWhiteSpace(_targetWorkingPath))
                return _targetWorkingPath;
            var basePath = GetWorkingFolder();
            var cb = new SqlConnectionStringBuilder(TargetConString);
            var TargetworkingPath = System.IO.Path.Combine(basePath, cb.DataSource, cb.InitialCatalog);

            if (!System.IO.Directory.Exists(TargetworkingPath))
                System.IO.Directory.CreateDirectory(TargetworkingPath);

            return TargetworkingPath;
        }
        public abstract void Compare();
    }
}