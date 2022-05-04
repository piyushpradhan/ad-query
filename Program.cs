using System;
using System.CommandLine;
using System.CommandLine.DragonFruit;
using System.CommandLine.NamingConventionBinder;
using System.DirectoryServices;

namespace ADQuery
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var root = new RootCommand
            {
                new Option<string>(
                    "--command", 
                    description: "Enter the command you want to execute"),
                new Option<string>(
                    "--properties", 
                    description: "Enter the properties that you want information for"),
            };

            root.Handler = CommandHandler.Create<string, string>((command, properties) =>
            {
                Console.WriteLine($"Command: {command}");
                Console.WriteLine($"Properties: {properties}");
            });

            await root.InvokeAsync(args);

            Console.ReadLine();

            return;
        }            
        static void ExtractAllUsersInfo()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://10.0.2.7", "JDoe", "FirstTarget1");
            DirectorySearcher ds = new DirectorySearcher(de);
            String filter = "(&(objectCategory=user)(objectClass=user))";
            ds.Filter = filter;
            try
            {
                foreach (SearchResult result in ds.FindAll())
                {
                    DirectoryEntry resDe = result.GetDirectoryEntry();

                    ResultPropertyCollection resPropCollection = result.Properties;
                    foreach (string prop in resPropCollection.PropertyNames)
                    {
                        Console.WriteLine(prop);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Something went wrong");
            }
        }
    }
}