using Mono.Options;

namespace SQLDbCompare
{
    class Program
    {
        static void Main(string[] args)
        {
            //FileHashKeycompare p = new FileHashKeycompare();
            //p.Run();


            //"--username=ssjod",
            //"--password=Windows98"
            args = new string[] {
                "--scs=Data Source=JSHOME;Integrated Security=true;Initial Catalog=New_IcareMvcMaster",
                "--tcs=Data Source=JSHOME;Integrated Security=true;Initial Catalog=IcareMvcMaster",
                "--appName=TestApp"
            };


            var compare = new SqlDbSchemaCompare();
            compare.Run(args);
        }
    }
}
