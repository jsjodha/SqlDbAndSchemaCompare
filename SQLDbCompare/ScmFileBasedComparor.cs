using Microsoft.SqlServer.Dac.Compare;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SQLDbCompare
{
    internal class ScmFileBasedComparor : BaseComparor
    {
        public ScmFileBasedComparor(string sourceConString, string targetConString)
            : base(sourceConString, targetConString)
        { }

        public override void Compare()
        {
            if (string.IsNullOrWhiteSpace(this.SourceConString) || string.IsNullOrWhiteSpace(this.TargetConString))
                throw new ArgumentNullException("Source and Target connection string are empty");

            if (Program.CommandArgs.ContainsKey("username") && Program.CommandArgs.ContainsKey("password"))
            {
                using (ImpersonateIt impersonator = new ImpersonateIt())
                {
                    impersonator.Impersonate(
                        Program.CommandArgs["username"].ToString(),
                        Program.CommandArgs["password"].ToString());
                    Start();
                }
            }
            else
                Start();

        }

        private void Start()
        {
            var src = new SqlConnectionStringBuilder(this.SourceConString);
            var target = new SqlConnectionStringBuilder(this.TargetConString);


            //if Bac file not exists create new 
            //var bacPacFile = Path.Combine(GetWorkingFolder(), src.InitialCatalog + ".dacpac");
            //var dacService = new Microsoft.SqlServer.Dac.DacServices(this.SourceConString);
            //dacService.Extract(bacPacFile, src.InitialCatalog, "Test Application", Version.Parse(AppVersion));

            //var sourceDacpac = new SchemaCompareDacpacEndpoint(bacPacFile);
            Console.WriteLine("Creating Database end point connections.");
            var sourceEndPoint = new SchemaCompareDatabaseEndpoint(this.SourceConString);

            var targetEndPoint = new SchemaCompareDatabaseEndpoint(this.TargetConString);


            Console.WriteLine("Initializing Schema Comparision with given endpoints");
            var comparison = new SchemaComparison(sourceEndPoint, targetEndPoint);

            //// Persist comparison file to disk in Schema Compare (.scmp) format
            //var scmpFile = GetDacFileName(src.ConnectionString) + ".scmp";
            //comparison.SaveToFile(scmpFile);            
            //// Load comparison from Schema Compare (.scmp) file
            //comparison = new SchemaComparison(scmpFile);
            Console.WriteLine("Schema Comparision Setting Options...");
            comparison.Options = new Microsoft.SqlServer.Dac.DacDeployOptions()
            {
            };
            if (Program.CommandArgs.ContainsKey("excludedTypes"))
            {
                var stringSplitter = new string[] { ",", ";", "|" };
                var excludedTypes = new List<Microsoft.SqlServer.Dac.ObjectType>();
                foreach (var type in Program.CommandArgs["excludedTypes"].ToString().Split(stringSplitter, StringSplitOptions.RemoveEmptyEntries))
                {
                    Microsoft.SqlServer.Dac.ObjectType outType;
                    if (Enum.TryParse(type, out outType))
                    {
                        Console.WriteLine("   Adding excluded type {0}", outType.ToString());
                        excludedTypes.Add(outType);
                    }
                }
                comparison.Options.ExcludeObjectTypes = excludedTypes.ToArray();
            }
            Console.WriteLine("Schema Comparision Starting...");
            //Set Options 

            SchemaComparisonResult comparisonResult = comparison.Compare();
            Console.WriteLine("Schema Comparision Compated.");
            var sourceFolder = GetSourceWorkingFolder(this.SourceConString);
            var targetFolder = GetSourceWorkingFolder(this.TargetConString);
            Console.WriteLine("Creating diff object scripts...");
            foreach (var d in comparisonResult.Differences)
            {
                string objectName = string.Join(".", d.SourceObject?.Name.Parts ?? d.TargetObject.Name.Parts);
                string objType = d.SourceObject?.ObjectType?.Name ?? d.TargetObject?.ObjectType?.Name;
                if (string.IsNullOrWhiteSpace(objectName))
                {
                    Console.WriteLine("Object Name is empty {0}", objectName);
                    continue;
                }
                WriteToFile(sourceFolder, objType, objectName, d.SourceObject?.ToString());

                WriteToFile(targetFolder, objType, objectName, d.TargetObject?.GetScript());
            }

            // Find the change to table1 and exclude it.
            //foreach (SchemaDifference difference in comparisonResult.Differences)
            //{
            //    if (difference.TargetObject.Name != null &&
            //        difference.TargetObject.Name.HasName &&
            //        difference.TargetObject.Name.Parts[1] == "DbConnections")
            //    {
            //        comparisonResult.Exclude(difference);
            //        break;
            //    }
            //}


            // Publish the changes to the target database
            //SchemaComparePublishResult publishResult = comparisonResult.PublishChangesToTarget();
            var publishResult = comparisonResult.GenerateScript(target.InitialCatalog);
            WriteToFile(GetWorkingFolder(), "", "MasterChangeScript.sql", publishResult.MasterScript);
            WriteToFile(GetWorkingFolder(), "", "ChangeScript.sql", publishResult.Script);

            Console.WriteLine(publishResult.Success ? "Publish succeeded." : "Publish failed.");

            System.IO.Compression.ZipFile.CreateFromDirectory(GetWorkingFolder(), Path.Combine(_basePath, "Artifact" + DateTime.Now.ToString("ddMMMyyHHmmss") + ".zip"));
        }
    }
}