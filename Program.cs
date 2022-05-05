using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.DirectoryServices;

namespace ADQuery
{
    class Program
    {
        public static DirectoryEntry de = new DirectoryEntry("LDAP://10.0.2.7", "JDoe", "FirstTarget1");
        public static DirectorySearcher ds = new DirectorySearcher(de);
        static async Task Main(string[] args)
        {
            var root = new RootCommand
            {
                new Option<string>(
                    "--command", 
                    description: "Enter the command you want to execute"),
                new Option<string>(
                    "--filter", 
                    description: "Enter the LDAP filter"),
                new Option<string>(
                    "--properties", 
                    description: "Enter the properties you want to query (comma separatared)"),
            };

            root.Handler = CommandHandler.Create<string, string?, string?>((command, filter, properties) =>
            {
                switch (command)
                {
                    case "custom":
                        CustomQuery(filter, properties);
                        break;
                    case "allUsers":
                        ExtractAllUsersInfo(properties);
                        break;
                }
            });

            await root.InvokeAsync(args);
        }            
        static void CustomQuery(string? filter, string? props = "")
        {
            string[] properties = props!.Split(",").ToArray<string>();
            ds.Filter = filter!;
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            } 
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        static void ExtractAllUsersInfo(string? props)
        {
            string[] properties = props!.Split(",").ToArray<string>();
            ds.Filter = "(&(objectCategory=user)(objectClass=user))";
            SearchResultCollection searchResult = ds.FindAll();
            PrintResult(searchResult, properties);
            
        }
        static void PrintResult(SearchResultCollection searchResults, string[] properties)
        {
            try
            {
                foreach (SearchResult result in searchResults)
                {
                    DirectoryEntry resDe = result.GetDirectoryEntry();
                    Console.WriteLine("---------------------------------------------------");
                    ResultPropertyCollection resPropCollection = result.Properties;
                    if (properties.Length > 0)
                    {
                        foreach(string key in properties)
                        {
                            foreach(Object value in resPropCollection[key])
                            {
                                Console.WriteLine("{0} : {1}", key, value);
                            }
                        }
                    }
                    else
                    {
                        foreach (string prop in resPropCollection.PropertyNames)
                        {
                            foreach (Object value in resPropCollection[prop])
                            {
                                Console.WriteLine("{0} : {1}", prop, value);
                            }
                        }
                    }
                    Console.WriteLine("---------------------------------------------------");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Something went wrong");
            }
        }
    }
}