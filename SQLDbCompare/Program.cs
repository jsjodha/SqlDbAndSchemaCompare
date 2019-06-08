using System;
using System.Collections.Generic;
using System.Threading;
using Mono.Options;

namespace SQLDbCompare
{
    class Program
    {
        public static readonly CancellationTokenSource Ctc = new CancellationTokenSource();

        public static Dictionary<string, object> CommandArgs = new Dictionary<string, object>();

        public static  OptionSet _options;
        static void Main(string[] args)
        {
            //"--username=ssjod",
            //"--password=Windows98"
            args = new string[] {
                "--scs=Data Source=JSHOME;Integrated Security=true;Initial Catalog=IcareMvcMaster",
                "--tcs=Data Source=JSHOME;Integrated Security=true;Initial Catalog=New_IcareMvcMaster",
                "--appName=SchemaCompareApp",
                "--appVersion=2019.06.08.1200",
                "--excludedTypes=PartitionFunctions;PartitionSchemes;DatabaseOptions"
            };

            SetupCmdArgs(args);


            if (CommandArgs.ContainsKey("scs") && CommandArgs.ContainsKey("tcs"))
            {
                var scs = CommandArgs["scs"].ToString();
                var tcs = CommandArgs["tcs"].ToString();
                var comparor = new ScmFileBasedComparor(scs, tcs);
                comparor.Compare();
            }
            else
            {
                Console.Error.WriteLine("Invalid command arguments");
                ShowHelp();
            }
            
        }

        private static void SetupCmdArgs(string[] args)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Ctc.Cancel();
            };
            _options = new OptionSet
                        {
                            {
                                "scs=|SourceConnectionString=", "source connection string",
                                s => CommandArgs["scs"] = s
                            },
                             {
                                "tcs=|TargetConnectionString=", "Target connection string",
                                t => CommandArgs["tcs"] = t
                            },
                              {
                                "u=|username=", "UserName to use to execute this program. e.g. domain\\user",
                                s => CommandArgs["username"] = s
                            },
                            {
                                "p=|password=", "user Password to be used for authentication.",
                                s => CommandArgs["password"] = s.ToString().Trim()
                            },
                             {
                                "apn=|appName=", "Application Name which is used to log into Db for activities.",
                                s => CommandArgs["appName"] = s
                            },
                              {
                                "apv=|appVersion=", "Application Version which is used to log into Db for activities.",
                                s => CommandArgs["appVersion"] = s
                            },
                                  {
                                "et=|excludedTypes=", "object to be excluded while comparing. options are: " +
                                string.Join(";", Enum.GetNames(typeof(Microsoft.SqlServer.Dac.ObjectType)))
                               ,
                                s => CommandArgs["excludedTypes"] = s
                            },
                            {
                                "h|help", "show this message and exit",
                                s => CommandArgs["help"] = s
                            }
                        };

            _options.Parse(args);

            if (CommandArgs.ContainsKey("help") && CommandArgs["help"] != null)
            {
                ShowHelp();                
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("SQL Db Schema Compare Tool");
            Console.WriteLine();

            _options.WriteOptionDescriptions(Console.Out);
        }
    }
}
