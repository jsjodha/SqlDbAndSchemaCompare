using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Compare;
using Microsoft.SqlServer.Dac.Model;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SQLDbCompare
{
    public class SqlDbSchemaCompare
    {
        private readonly CancellationTokenSource Ctc = new CancellationTokenSource();

        Dictionary<string, object> cmd = new Dictionary<string, object>();

        private readonly OptionSet _options;
        public SqlDbSchemaCompare()
        {
            _options = new OptionSet
                        {
                            {
                                "scs=|SourceConnectionString=", "source connection string",
                                s => cmd["scs"] = s
                            },
                             {
                                "tcs=|TargetConnectionString=", "Target connection string",
                                t => cmd["tcs"] = t
                            },
                              {
                                "u=|username=", "UserName to use to execute this program. e.g. domain\\user",
                                s => cmd["username"] = s
                            },
                            {
                                "p=|password=", "user Password to be used for authentication.",
                                s => cmd["password"] = s.ToString().Trim()
                            },
                             {
                                "apn=|appName=", "Application Name which is used to log into Db for activities.",
                                s => cmd["appName"] = s
                            },
                            {
                                "h|help", "show this message and exit",
                                s => cmd["help"] = s
                            }
                        };
        }
        public string Run(string[] args)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Ctc.Cancel();
            };


            _options.Parse(args);

            if (cmd.ContainsKey("help") && cmd["help"] != null)
            {
                ShowHelp(_options);
                return "0";
            }

            //System.Diagnostics.Trace.Listeners.Clear();            
            var consoleListner = new ConsoleTraceListener();            
            System.Diagnostics.Trace.Listeners.Add(consoleListner);

            if (cmd.ContainsKey("scs")
                && cmd.ContainsKey("tcs")
                && cmd.ContainsKey("username")
                && cmd.ContainsKey("password"))
            {

                var scs = cmd["scs"].ToString();
                var tcs = cmd["tcs"].ToString();
                var user = cmd["username"].ToString();
                var pass = cmd["password"].ToString();

                var rs = MainExec(scs, tcs, user, pass, C_Token: Ctc);

                return rs;
            }
            else if (cmd.ContainsKey("scs")
               && cmd.ContainsKey("tcs"))
            {

                var scs = cmd["scs"].ToString();
                var tcs = cmd["tcs"].ToString();

                var rs = MainExec(scs, tcs);
                rs = MainExec(tcs, scs);

                return rs;
            }
            else
                ShowHelp(_options);

            return "0";
        }

        private static string MainExec(string sourceConnectionString, string targerConnectionString, string username = null, string password = null, DacDeployOptions options = null, CancellationTokenSource C_Token = null)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                using (var impersonator = new ImpersonateIt())
                {
                    impersonator.Impersonate(username, password);

                    return Process(sourceConnectionString, targerConnectionString, ref options);
                }
            }
            else
                return Process(sourceConnectionString, targerConnectionString, ref options);
        }

        public void ReadModel(string srcConString)
        {
            var decFilePath = GetDacFileName(srcConString) + ".bacpac";
            using (TSqlModel model = new TSqlModel(decFilePath))
            {
                // This will get all tables. Note the use of Table.TypeClass!
                var tables = model.GetObjects(DacQueryScopes.Default, Table.TypeClass).ToList();

                // Look up a specific table by ID. Note that if no schema is defined when creating 
                // an element the default "dbo" schema is used
                var t1 = model.GetObjects(Table.TypeClass,new ObjectIdentifier("dbo", "t1"), DacQueryScopes.Default).FirstOrDefault();
                
                // Get a the column referenced by this table, and query its length 
                TSqlObject column = t1.GetReferenced(Table.Columns).First(col => col.Name.Parts[2].Equals("c1"));
                int columnLength = column.GetProperty<int>(Column.Length);
                Console.WriteLine("Column c1 has length {0}", columnLength);

                // Verify the ColumnType of this column. This can help indicate which 
                // properties will return meaningful values.
                // For instance since Column.Collation is only available on a simple column,
                // and Column.Persisted is only on computed columns
                ColumnType columnType = column.GetMetadata<ColumnType>(Column.ColumnType);
                Console.WriteLine("Column c1 is of type '{0}'", columnType);
            }
        }
        private static string Process(string srcConString, string targetConString, ref DacDeployOptions options)
        {
            var srcCon = new SqlConnectionStringBuilder(srcConString);
            var decFilePath = GetDacFileName(srcConString)+".bacpac";
            Export(srcConString, decFilePath);
            //Export(targerConnectionString, @"C:\Temp\Target_dacFile.dacpac");

            var TargetCon = new SqlConnectionStringBuilder(targetConString);
            var TargetdacServices = new DacServices(TargetCon.ConnectionString);

            TargetdacServices.Message += ((s, e) => { Console.WriteLine(e?.Message.ToString()); });
            TargetdacServices.ProgressChanged += ((s, e) => { Console.WriteLine("Status:{0}, Message:{1}", e?.Status, e?.Message.ToString()); });

            if (options == null)
            {
                options = new DacDeployOptions()
                {
                    CommentOutSetVarDeclarations = true,
                    DoNotAlterChangeDataCaptureObjects = true,
                    DropDmlTriggersNotInSource = true,
                    PopulateFilesOnFileGroups = true,
                    RegisterDataTierApplication = true,

                };
            }


            
            using (DacPackage dacpac = DacPackage.Load(decFilePath, DacSchemaModelStorageType.Memory))
            {
                // Script then deploy, to support debugging of the generated plan
                // string script = dacServices.GenerateDeployScript(dacpac, dbName, options);
               var dt =  DateTime.Now.ToString("yyyy.MM.dd.HHmmss");

                //TargetdacServices.Register(TargetCon.InitialCatalog, DacSchemaModelStorageType.File, "DacService", Version.Parse(dt));
                //CreateXmlReport(srcConString, TargetCon, TargetdacServices, dacpac);
                //CreateSqlChangeScript(srcConString, TargetCon, TargetdacServices, dacpac);
                //CreateXmlDiffReport(srcConString, TargetCon, TargetdacServices);


                
                var pubOptions = new PublishOptions()
                {
                    DeployOptions = options,
                    GenerateDeploymentReport  =true,
                    GenerateDeploymentScript =true,
                    DatabaseScriptPath= GetDacFileName(srcConString) ,
                    MasterDbScriptPath= GetDacFileName(srcConString)+"Master"
                    

                };

                
                var rs = TargetdacServices.Script(dacpac, TargetCon.InitialCatalog, pubOptions);


               
                string scriptPath = GetDacFileName(srcConString) + "_" + srcCon.DataSource+ ".sql";
                string reportPath = GetDacFileName(srcConString) + "_" + srcCon.DataSource+ ".xml";
                System.IO.File.WriteAllText(scriptPath, rs.DatabaseScript);
                System.IO.File.WriteAllText(reportPath, rs.DeploymentReport);
                return "Done.";
            }
        }

        private static void CreateXmlDiffReport(string srcConString, SqlConnectionStringBuilder TargetCon, DacServices TargetdacServices)
        {
            var DiffReport = TargetdacServices.GenerateDriftReport(TargetCon.InitialCatalog);
            var outDiffReport = GetDacFileName(srcConString) + "_DeployDiff.xml";
            System.IO.File.WriteAllText(outDiffReport, DiffReport);
            Console.WriteLine("DiffReport.{0}", DiffReport);
        }

        private static void CreateSqlChangeScript(string srcConString, SqlConnectionStringBuilder TargetCon, DacServices TargetdacServices, DacPackage dacpac)
        {
            var deployScript = TargetdacServices.GenerateDeployScript(dacpac, TargetCon.InitialCatalog);
            var outScriptPath = GetDacFileName(srcConString) + "_DeployScript.sql";
            System.IO.File.WriteAllText(outScriptPath, deployScript);
            Console.WriteLine("DeployScript.{0}", deployScript);
        }

        private static void CreateXmlReport(string srcConString, SqlConnectionStringBuilder TargetCon, DacServices TargetdacServices, DacPackage dacpac)
        {
            var deployReport = TargetdacServices.GenerateDeployReport(dacpac, TargetCon.InitialCatalog);
            var outReportPath = GetDacFileName(srcConString) + "_DeployReport.xml";
            System.IO.File.WriteAllText(outReportPath, deployReport);
            Console.WriteLine("DeployReport.{0}", deployReport);
        }

        private static string GetDacFileName(string sourceConnectionString)
        {
            var sBuilder = new SqlConnectionStringBuilder(sourceConnectionString);

            var baseFolder = @"C:\Temp\SchemaCompare\" + DateTime.Today.ToString("yy_MMM_dd") + "_" + sBuilder.DataSource;

            if (!System.IO.Directory.Exists(baseFolder))
                System.IO.Directory.CreateDirectory(baseFolder);
            
            return System.IO.Path.Combine(baseFolder, sBuilder.InitialCatalog);

        }

        //private static string MainExec(string sourceConnectionString, string targerConnectionString, string username, string password, DacDeployOptions options = null, CancellationTokenSource C_Token = null)
        //{
        //    using (var impersonator = new ImpersonateIt())
        //    {
        //        impersonator.Impersonate(username, password);

        //        var decFilePath = @"C:\Temp\Source_dacFile.dacpac";
        //        Export(sourceConnectionString, decFilePath);
        //        //Export(targerConnectionString, @"C:\Temp\Target_dacFile.dacpac");

        //        var TargetCon = new SqlConnectionStringBuilder(targerConnectionString);
        //        var TargetdacServices = new DacServices(TargetCon.ConnectionString);

        //        TargetdacServices.Message += ((s, e) => { Console.WriteLine(e?.Message.ToString()); });
        //        TargetdacServices.ProgressChanged += ((s, e) => { Console.WriteLine("Status:{0}, Message:{1}", e?.Status, e?.Message.ToString()); });

        //        if (options == null)
        //        {
        //            options = new DacDeployOptions();
        //        }

        //        using (DacPackage dacpac = DacPackage.Load(decFilePath, DacSchemaModelStorageType.Memory))
        //        {

        //            // Script then deploy, to support debugging of the generated plan
        //            // string script = dacServices.GenerateDeployScript(dacpac, dbName, options);
        //            var deployReport = TargetdacServices.GenerateDeployReport(dacpac, TargetCon.InitialCatalog);
        //            var outReportPath = Path.Combine(@"C:\Temp\", "DeployReport_" + DateTime.Now.ToString("yyyyMMMdd HHmmsstt") + ".sql");
        //            System.IO.File.WriteAllText(outReportPath, deployReport);
        //            Console.WriteLine("DeployReport.{0}", deployReport);

        //            var deployScript = TargetdacServices.GenerateDeployScript(dacpac, TargetCon.InitialCatalog);
        //            var outScriptPath = Path.Combine(@"C:\Temp\", "DeployScript_" + DateTime.Now.ToString("yyyyMMMdd HHmmsstt") + ".sql");
        //            System.IO.File.WriteAllText(outScriptPath, deployScript);
        //            Console.WriteLine("DeployScript.{0}", deployScript);

        //            var DiffReport = TargetdacServices.GenerateDriftReport(TargetCon.InitialCatalog);
        //            var outDiffReport = Path.Combine(@"C:\Temp\", "DeployDiff_" + DateTime.Now.ToString("yyyyMMMdd HHmmsstt") + ".sql");
        //            System.IO.File.WriteAllText(outDiffReport, DiffReport);
        //            Console.WriteLine("DiffReport.{0}", DiffReport);

        //            return "Done.";
        //        }
        //    }
        //    return "";
        //}

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("SQL Db Schema Compare Tool");
            Console.WriteLine("Export SQL databases as bacpac.");
            Console.WriteLine();

            p.WriteOptionDescriptions(Console.Out);
        }

        public static bool Export(string sourceDbConString, string outDacFilePath)
        {
            bool result = false;
            var sourceConBuilder = new SqlConnectionStringBuilder(sourceDbConString);
            var services = new DacServices(sourceConBuilder.ConnectionString);

            services.ProgressChanged += ((s, e) => { Console.WriteLine("Ëxporting Dacpack Status:{0} , Message:{1}.", e.Status, e.Message); });

            string blobName;

            if (System.IO.File.Exists(outDacFilePath))
            {
                System.IO.File.Delete(outDacFilePath);
            }

            using (FileStream stream = File.Open(outDacFilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                Console.WriteLine("starting bacpac export");

                DacExportOptions opts = new DacExportOptions()
                {
                    TargetEngineVersion = EngineVersion.Default,
                    Storage = DacSchemaModelStorageType.Memory,
                    VerifyFullTextDocumentTypesSupported = false

                };
                var extOptions = new DacExtractOptions()
                {
                     ExtractAllTableData =false,
                     ExtractApplicationScopedObjectsOnly= true,
                     ExtractReferencedServerScopedElements=true
                };

                services.Extract(packageStream: stream,
                    databaseName: sourceConBuilder.InitialCatalog, 
                    applicationName: "Schema_Exporter",
                    extractOptions:extOptions,                    
                    applicationVersion: Version.Parse("1.0.0.0"));
                

                stream.Flush();

                return true;
            }
            return result;

        }
        public static void Deploy(Stream dacpac, string connectionString, string databaseName)
        {
            var options = new DacDeployOptions()
            {
                BlockOnPossibleDataLoss = true,
                IncludeTransactionalScripts = true,
                DropConstraintsNotInSource = false,
                DropIndexesNotInSource = false,
                DropDmlTriggersNotInSource = false,
                DropObjectsNotInSource = false,
                DropExtendedPropertiesNotInSource = false,
                DropPermissionsNotInSource = false,
                DropStatisticsNotInSource = false,
                DropRoleMembersNotInSource = false,
            };

            var service = new DacServices(connectionString);
            service.Message += (x, y) =>
            {
                Console.WriteLine(y.Message.Message);
            };
            try
            {
                using (var package = DacPackage.Load(dacpac))
                {
                    service.Deploy(package, databaseName, true, options);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, true);
            }
        }

        public void ScmFileBasedCompare(string srcConString, string targetConString)
        {
            var src = new SqlConnectionStringBuilder(srcConString);

            var bacPacFile = GetDacFileName(srcConString)+".dacpac";
            var dtVersion = DateTime.Now.ToString("yyyy.MM.dd.HHmmss");

            //if Bac file not exists create new 
            var dacService = new Microsoft.SqlServer.Dac.DacServices(srcConString);
            dacService.Extract(bacPacFile, src.InitialCatalog, "Test Application", Version.Parse(dtVersion));

            var sourceDacpac = new SchemaCompareDacpacEndpoint(bacPacFile);

            var target = new SqlConnectionStringBuilder(targetConString);
            var targetDatabase = new SchemaCompareDatabaseEndpoint(targetConString);

            var comparison = new SchemaComparison(sourceDacpac, targetDatabase);
            // Persist comparison file to disk in Schema Compare (.scmp) format
            comparison.SaveToFile(@"C:\temp\mycomparison.scmp");

            // Load comparison from Schema Compare (.scmp) file
            comparison = new SchemaComparison(@"C:\temp\mycomparison.scmp");
            SchemaComparisonResult comparisonResult = comparison.Compare();
             foreach (var d in comparisonResult.Differences)
            {
                Console.WriteLine(d.SourceObject.GetScript());

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
            var publishResult = comparisonResult.GenerateScript(".");

            
            Console.WriteLine(publishResult.MasterScript);
            Console.WriteLine(publishResult.Script);
            Console.WriteLine(publishResult.Success ? "Publish succeeded." : "Publish failed.");
        }


    }
}

