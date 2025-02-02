using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;

namespace Brickline
{


    public partial class Form1 : Form
    {
        
        BricklineInterface bricklineInterface = new BricklineInterface();



    public Form1()
        {



            
            InitializeComponent();
        }
        public ListView GetListView1()
        {
            return listView1;
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void JoinServer(object sender, MouseEventArgs e)
        {
            bricklineInterface.Servers[listView1.SelectedItems[0].Index].JoinServer();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            StatusIndicator.Text = "Creating log file...";
            FileStream LogFile = File.Create(Environment.CurrentDirectory + "\\Latest.log");
            LogFile.Close();
            
            Log("CurrentDirectory :: " + Environment.CurrentDirectory);
            listView1.MouseDoubleClick += JoinServer;
            LoadList();
            Console.SetOut(new ConsoleToTextBoxWriter(textBox2));

        }
        
        public async Task LoadList()
        {
            
            StatusIndicator.Text = "Loading servers...";
            Log("LoadList :: Clearing bricklineInterface.Servers...");
            bricklineInterface.Servers.Clear();
            Log("LoadList :: Waiting for bricklineInterface to give me the server info...");
            await bricklineInterface.WriteServerInfo();
            foreach (BricklineInterface.ServerInfo server in bricklineInterface.Servers) {
                ListViewItem serverItem = new ListViewItem();
                serverItem.Text = (server.Name + " | " + server.Map + " | " + server.Players);
                
                
                listView1.Items.Add(serverItem);

            }
            StatusIndicator.Text = "Servers";
            
        }
        
        public void Log(string msg)
        {
            Console.WriteLine(msg);
            StreamWriter LogStream = File.AppendText(Environment.CurrentDirectory + "\\Latest.log");
            LogStream.WriteLine(msg);
            LogStream.Close();
          
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            listView1.Items.Clear();
            LoadList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(bricklineInterface.Servers[listView1.SelectedItems[0].Index].FormattedInfo().ToString(), "Advanced Info"); ;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Click on a server, first.", "Hey!");
            }
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.tocat.xyz/quick-links/brickline-official-download.html");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
    public class ConsoleToTextBoxWriter : TextWriter
    {
        private TextBox _textBox;
        private delegate void AppendTextDelegate(string text);

        public ConsoleToTextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        public override async void Write(char value)
        {
            StreamWriter LogStream = File.AppendText(Environment.CurrentDirectory + "\\Latest.log");
            LogStream.Write(value);
            LogStream.Close();
            _textBox.Invoke(new AppendTextDelegate(_textBox.AppendText), value.ToString());
        }

        public override async void Write(string value)
        {
            StreamWriter LogStream = File.AppendText(Environment.CurrentDirectory + "\\Latest.log");
            LogStream.WriteLine(value);
            LogStream.Close();
            _textBox.Invoke(new AppendTextDelegate(_textBox.AppendText), value);
        }

        public override Encoding Encoding => System.Text.Encoding.UTF8;
    }

    public class BricklineInterface
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
            public void JoinServer()
            {
                Console.WriteLine("BrickLineInterface::ServerInfo::JoinServer() :: Attempting to join a server.");
                Console.WriteLine("==Server Info==" + Environment.NewLine + this.FormattedInfo());
                Console.WriteLine("BrickLineInterface::ServerInfo::JoinServer() :: Checking for existing RobloxApp_client processes...");
                if (Process.GetProcessesByName("RobloxApp_client").Length > 0)
                {
                    Console.WriteLine("BrickLineInterface::ServerInfo::JoinServer() :: Found " + Process.GetProcessesByName("RobloxApp_client").Length + " instances of RobloxApp_client. Prompting user to close them.");
                    DialogResult ClosePrompt = MessageBox.Show("Would you like to exit it?", "You already have a client running.", MessageBoxButtons.YesNo);
                    if (ClosePrompt == DialogResult.Yes)
                    {

                        foreach (Process process in Process.GetProcessesByName("RobloxApp_client"))
                        {
                            Console.WriteLine("BrickLineInterface::ServerInfo::JoinServer() :: Killing Roblox client with PID: " + process.Id);
                            process.Kill();
                            
                        }
                        
                    
                    }if (ClosePrompt == DialogResult.No)
                    {
                        return;
                    }
                }
                Console.WriteLine("BrickLineInterface::ServerInfo::JoinServer() :: No clients running, loading the URI...");
                Process.Start(URI);
            }
        }

        public List<ServerInfo> Servers = new List<ServerInfo>();
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
        public async Task WriteServerInfo()
        {
            //Console.Write("Loading servers | ");
            //ProgressBar downloadbar = new ProgressBar();
            Console.WriteLine("BrickLineInterface::WriteServerInfo() :: Fetching data...");
            string url = "http://brickline.blackspace.lol:45689/";
            HttpClient client = new HttpClient();
            string html = "";
            try
            {



                html = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error: Failed to fetch brickline! Either it's down, or you've started the launcher too many times.\nException ID: " + ex.ToString());
                return;
            }

            Console.WriteLine("BrickLineInterface::WriteServerInfo() :: Retrieved the HTML for Brickline, load into HtmlAgilityPack for scraping...");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            //(int CursorLeft, int CursorTop) CurrentCursor = (//Console.CursorLeft, //Console.CursorTop);
           

            var serverList = doc.DocumentNode.SelectSingleNode("//div[@class='server-list']");
            if (serverList != null)
            {

                var servers = serverList.SelectNodes(".//div[@class='server']");
                if (servers != null)
                {

                    Console.WriteLine("BrickLineInterface::WriteServerInfo() :: Found " + servers.Count + " servers.");
                    foreach (var server in servers)
                    {



                        ServerInfo CurrentServer = new ServerInfo();

                        var serverTitle = server.SelectSingleNode(".//div[@class='server-title']/span");
                        if (serverTitle != null)
                        {

                            CurrentServer.Name = WebUtility.HtmlDecode(serverTitle.InnerText);
                        }
                        if (serverTitle == null || serverTitle.InnerText == "")
                        {
                            //Console.ForegroundColor = ConsoleColor.Yellow;
                            //Console.WriteLine("Writing server info for a blank server?? WTF??");
                            //Console.ResetColor();
                        }
                        var serverDetails = server.SelectSingleNode(".//div[@class='server-details']");

                        if (serverDetails != null)
                        {

                            foreach (var p in serverDetails.SelectNodes("p"))
                            {
                                //Not formatting... Very dirty hack.
                                //Makes some servers look very weird... Too bad!
                                Console.WriteLine("BrickLineInterface::WriteServerInfo() :: Got server detail: " + p.InnerText + ", resolve it and write it ServerInfo constructor");
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
                                    }
                                    catch (Exception ex)
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
                                //Console.ForegroundColor = ConsoleColor.Red;
                                //Console.WriteLine("ERROR: Server has no URI???");
                                Console.WriteLine("BrickLineInterface::WriteServerInfo() :: WTF? Server has no URI? Crash!");
                                throw new UriFormatException();
                            }
                            else
                            {
                                Console.WriteLine("BrickLineInterface::WriteServerInfo() :: Got URI: " + uri + ", write to ServerInfo constructor");
                                CurrentServer.URI = uri;


                            }

                        }

                        Console.WriteLine("BrickLineInterface::WriteServerInfo() :: DONE! Add to the server browser...");
                        Servers.Add(CurrentServer);


                    }
                    //downloadbar.Dispose();
                    //Console.Write("DONE!\n");

                    
                }
            }
        }
    }
}
