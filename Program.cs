using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NeoSmart.Utils;

namespace Brickline_Launcher
{
    public class ServerInfo
    {
        public string Name;
        public string IP;
        public string Port;
        public string Players;
        public string Version;
        public string Client;
        public string URI;
        public string Map;
        public string Uptime;
        public string FormattedInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Name);
            sb.AppendLine(IP);
            sb.AppendLine(Port);
            sb.AppendLine(Players);
            sb.AppendLine(Version);
            sb.AppendLine(Client);
            sb.AppendLine(Map);
            sb.AppendLine(Uptime);
            return sb.ToString();
        }
    }
    internal class Program
    {

        static List<string> URIs = new List<string>();
        static List<ServerInfo> Servers = new List<ServerInfo>();
        static string GetFormattedServerList()
        {
            string ServerInfoFileRaw = new WebClient().DownloadString("http://brickline.blackspace.lol:45689/serverlist.txt");
            string Output = "";

            
            string[] ServerInfoList = ServerInfoFileRaw.Split('|', '\n');
            foreach (string ServerInfo in ServerInfoList)
            {

                byte[] Decoded = Convert.FromBase64String(ServerInfo);
                Console.WriteLine("DEBUG :: Raw decoded: " + Decoded);
                 Output += "--------\n" + Encoding.UTF8.GetString(Decoded) + "\n---------\n";
            }
            return Output;
        }
        static async Task Main(string[] args)
        {
            Console.Title = "BRICKLINE LAUNCHER";
            if (!args.Contains("--hidesplash"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(@"@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@%*=--=*###+--=#@@@@@%*=---=+#@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%=-=%@@@@%=--#@@@@@@@=--*@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=%@@@@@=--+@@@@@@@+--#@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=%@@@@%=-=%@@@@@@@+-=#@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=*###*=+*@@@@@@@@@+--#@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=*###*+==*%@@@@@@@+-=#@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=%@@@@@%=--+@@@@@@+--#@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=%@@@@@@#=--%@@@@@+-=#@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@%+-=#@@@@@@*---@@@@@@+--#@@@@@@@#=+%@@@@@@@@@@@
@@@@@@@@@@@@@@@%=--#@@@@@*=-=%@@@@@@=--*@@@@@@%+-*@@@@@@@@@@@@
@@@@@@@@@@@@@*=-----=====+#%@@@@@*==----======--=#@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                Console.ResetColor();
                Console.WriteLine("==BRICKLINE MASTERSERVER LAUNCHER==");
                Console.WriteLine("Launcher made by tocatwastaken");
                Console.WriteLine("Brickline made by ashlyn");
                Console.WriteLine("Join the Brickline discord server here: https://discord.gg/Vw6BAuJ4qZ");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

            Console.Clear();
            
            
            await WriteServerInfo();
            
            Console.Write("Please enter the ID of the server you want to connect to, type reload to refresh, or exit to quit: "); string input = Console.ReadLine();
            if (input != "exit" && input != "reload")
            {
                Console.Clear();
                //Console.WriteLine("URI index has " + Servers.Count + " entries.");
                //Console.ReadKey();
                Console.WriteLine("Is this the server you wanna join?\n");
                if (Servers.Count < int.Parse(input))
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Join Server - Error                                     \n\n\n\n");
                    Console.WriteLine("That ID is invalid!");
                    Console.WriteLine("Restarting in 2 seconds.");
                    Thread.Sleep(2500);

                    Process.Start(Process.GetCurrentProcess().ProcessName, "--hidesplash");
                }
                else
                {
                    
                    Console.WriteLine(Servers[int.Parse(input) - 1].FormattedInfo());
                    Console.Write("Do you wanna connect? [Y/N]"); var Input = Console.ReadLine();
                    if (Input.ToLower() == "y")
                    {
                        if (Process.GetProcessesByName("RobloxApp_client").Length > 0){
                            Console.WriteLine("You have another client open. Close it and try again.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            Process.Start(Process.GetCurrentProcess().ProcessName, "--hidesplash");
                            return;
                        }
                        Process.Start(Servers[int.Parse(input) - 1].URI);
                    
                    }
                    
                        Process.Start(Process.GetCurrentProcess().ProcessName, "--hidesplash");
                }
            }else if (input == "exit")
            {
                Environment.Exit(0);
            }else if (input == "reload")
            {
                Process.Start(Process.GetCurrentProcess().ProcessName, "--hidesplash");
            }else
            {
                Process.Start(Process.GetCurrentProcess().ProcessName, "--hidesplash");
            }
            //If you're wondering why we're using a sameline statement there (see above line), its to better show where the ReadLine is going to be at

        }
        static string ExtractUri(string onClickValue)
        {
            int start = onClickValue.IndexOf("`novetus://") + 1;
            int end = onClickValue.LastIndexOf("`;");
            if (start > 0 && end > start)
            {
                return onClickValue.Substring(start, end - start);
            }
            return "N/A";
        }
        static async System.Threading.Tasks.Task WriteServerInfo()
        {
            Console.Write("Loading servers | ");
            ProgressBar downloadbar = new ProgressBar();
            string url = "http://brickline.blackspace.lol:45689/";
            HttpClient client = new HttpClient();
            string html = "";
            try
            {
                
                
                
                html = await client.GetStringAsync(url);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: Failed to fetch brickline! Either it's down, or you've started the launcher too many times.\nException ID: " + ex.ToString());
                return;
            }
            

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            (int CursorLeft, int CursorTop) CurrentCursor = (Console.CursorLeft, Console.CursorTop);
            
            
            var serverList = doc.DocumentNode.SelectSingleNode("//div[@class='server-list']");
            if (serverList != null)
            {

                var servers = serverList.SelectNodes(".//div[@class='server']");
                if (servers != null)
                {
                    
                   
                    foreach (var server in servers)
                    {


                        downloadbar.Report((double)servers[server] / servers.Count);
                        ServerInfo CurrentServer = new ServerInfo();
                        
                        var serverTitle = server.SelectSingleNode(".//div[@class='server-title']/span");
                        if (serverTitle != null)
                        {

                            CurrentServer.Name = WebUtility.HtmlDecode(serverTitle.InnerText);
                        }
                        if (serverTitle == null || serverTitle.InnerText == ""){
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Writing server info for a blank server?? WTF??");
                            Console.ResetColor();
                        }
                        var serverDetails = server.SelectSingleNode(".//div[@class='server-details']");
                        
                        if (serverDetails != null)
                        {

                            foreach (var p in serverDetails.SelectNodes("p"))
                            {
                                //Not formatting... Very dirty hack.
                                //Makes some servers look very weird... Too bad!
                                
                                if (p.InnerText.StartsWith("Players: "))
                                {
                                    CurrentServer.Players = p.InnerText;
                                }
                                if (p.InnerText.StartsWith("IP: "))
                                {
                                    try
                                    {

                                        var IPInfo = p.InnerText.Replace("IP: ", "").Split(':');
                                        CurrentServer.IP = "IP: " + IPInfo[0];
                                        CurrentServer.Port = "Join Port: " + IPInfo[1];
                                    } catch(Exception ex)
                                    {
                                        CurrentServer.IP = "[FAILED TO RETRIEVE IP: " + ex.Message + "]";
                                        CurrentServer.Port = "[FAILED TO RETRIEVE PORT: " + ex.Message + "]";
                                    }
                                    
                                }
                                if (p.InnerText.StartsWith("Map: "))
                                {
                                    CurrentServer.Map = p.InnerText;
                                }
                                if (p.InnerText.StartsWith("Client: "))
                                {
                                    CurrentServer.Client = p.InnerText;
                                }
                                if (p.InnerText.StartsWith("Novetus version: "))
                                {
                                    CurrentServer.Version = p.InnerText;
                                }
                                if (p.InnerText.StartsWith("Uptime: "))
                                {
                                    CurrentServer.Uptime = p.InnerText;
                                }
                            }
                           
                        }
                        var joinButton = server.SelectSingleNode(".//button[contains(@class, 'join-button')]");
                        if (joinButton != null && joinButton.Attributes["onclick"] != null)
                        {
                            string onClickValue = joinButton.Attributes["onclick"].Value;
                            string uri = ExtractUri(onClickValue);
                            if (uri == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ERROR: Server has no URI???");
                                throw new UriFormatException();
                            }
                            else
                            {
                                CurrentServer.URI = uri;
                                
                                
                            }
                            
                        }
                        
                       
                        Servers.Add(CurrentServer);
                        

                    }
                    downloadbar.Dispose();
                    Console.Write("DONE!\n");
                    
                    foreach (ServerInfo server in Servers)
                    { 
                        Console.WriteLine(String.Format("===[ID: {0}]===", Servers.IndexOf(server)+1));
                        Console.WriteLine(server.FormattedInfo());
                        //Console.WriteLine(String.Format("===[END SERVER INFO FOR ID: {0}]===\n", Servers.IndexOf(server)));
                    }
                }
            }
        }
    }


}

  
