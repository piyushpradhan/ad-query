using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.DirectoryServices;

namespace ADQuery
{
    class Program
    {
        public static DirectoryEntry de = new DirectoryEntry();
        public static DirectorySearcher ds = new DirectorySearcher(de);
        static async Task Main(string[] args)
        {
            var root = new RootCommand
            {
                new Option<string>(
                    "--connect", 
                    description: "IP of the LDAP server"),
                new Option<string>(
                    "--username", 
                    description: "Enter the username of any valid domain user"),
                new Option<string>(
                    "--password", 
                    description: "Enter the password"),
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

            root.Handler = CommandHandler.Create<string, string?, string?, string?, string?, string?>((command, ip, username, password, filter, properties) =>
            {
                switch (command)
                {
                    case "connect":
                        try
                        {
                            de = new DirectoryEntry($"LDAP://{ip}", username, password);
                            ds = new DirectorySearcher(de);
                        } catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("[-] Please ensure you have entered the right set of credentials and IP");
                        }
                        break;
                    case "custom":
                        CustomQuery(filter, properties);
                        break;
                    case "user":
                        Console.WriteLine("Enter the username: ");
                        string name = Console.ReadLine();
                        ExtractSingleUserInfo(name, properties);
                        break;
                    case "computer":
                        Console.WriteLine("Enter the computer name: ");
                        string computerName = Console.ReadLine();
                        ExtractComputerInfo(computerName, properties);
                        break;
                    case "allComputers":
                        ExtractAllComputersInfo(properties);
                        break;
                    case "allUsers":
                        ExtractAllUsersInfo(properties);
                        break;
                }
            });

            await root.InvokeAsync(args);
        }            
        static void CustomQuery(string? filter, string? props)
        {
            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
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
        static void ExtractSingleUserInfo(string name, string? props)
        {
            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
            ds.Filter = $"(&(&(objectCategory=user)(objectClass=user))(|(cn={name})(displayname={name})(name={name})(samaccountname={name})))";
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Oops something went wrong");
                Console.WriteLine(e.Message);
            }
        }
        static void ExtractComputerInfo(string name, string? props)
        {
            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
            ds.Filter = $"(&(objectclass=computer)(|(cn={name})(name={name})(samaccountname={name})))";
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Oops something went wrong");
                Console.WriteLine(e.Message);
            }
        }
        static void ExtractAllComputersInfo(string? props)
        {
            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
            ds.Filter = "(objectclass=computer)";
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Oops something went wrong");
                Console.WriteLine(e.Message);
            }
        }
        static void ExtractAllUsersInfo(string? props)
        {

            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
            ds.Filter = "(&(objectCategory=user)(objectClass=user))";
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            } 
            catch (Exception e)
            {
                Console.WriteLine("[!] Oops something went wrong");
                Console.WriteLine(e.Message);
            }

        }
        static void ExtractDomainPolicy(string? props)
        {
            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
            ds.Filter = "(objectclass=domain)";
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Oops something went wrong");
                Console.WriteLine(e.Message);
            }
        }
        static void ExtractDomainTrust(string? props)
        {
            string[] properties = props == null ? Array.Empty<string>() : props!.Split(",").ToArray<string>();
            ds.Filter = "(objectclass=trustedDomain)";
            try
            {
                SearchResultCollection searchResult = ds.FindAll();
                PrintResult(searchResult, properties);
            }
            catch (Exception e)
            {
                Console.WriteLine("[!] Oops something went wrong");
                Console.WriteLine(e.Message);
            }
        }
        static void PrintResult(SearchResultCollection searchResults, string[] properties)
        {
            try
            {
                foreach (SearchResult result in searchResults)
                {
                    DirectoryEntry resDe = result.GetDirectoryEntry();
                    ResultPropertyCollection resPropCollection = result.Properties;
                    if (properties.Length > 0)
                    {
                        foreach(string key in properties)
                        {
                            foreach(Object value in resPropCollection[key.Trim()])
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