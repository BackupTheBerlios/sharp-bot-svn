using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using IRC_bot;



public class IrcBot
{
   public string server = "irc.homelien.no";
    int port = 6667;
    public string nick = "El|Csharp";
    readonly string user = "USER espenfjo 8 * :A simple IRC-bot using the .NET framework";
    public string channel = "#el-csharp";
    public readonly string owner = "Espenfjo";
    public readonly string ownerhost = "eldiablo@host-81-191-122-211.bluecom.no";

    
    
    public static StreamWriter writer;
    public NetworkStream nStream;
    public TcpClient irc;
    public static string inputLine;
    
    public StreamReader reader;
    public static Regex httpRegex;

    public void connections(int startTime)
    {
        UTF8Encoding utf8 = new UTF8Encoding();
        
        ReadConf conf = new ReadConf();
        conf.init();

        try
        {
            irc = new TcpClient(server, port);
            nStream = irc.GetStream();
            reader = new StreamReader(nStream,Encoding.GetEncoding("iso-8859-15"));
            writer = new StreamWriter(nStream, Encoding.GetEncoding("iso-8859-15"));

            try
            {
                writer.WriteLine(user);
                writer.Flush();
                writer.WriteLine("NICK " + nick);
                writer.Flush();
                

                inputLine = reader.ReadLine();
                
                if (mainLoop(startTime) == "error")
                {
                    Thread.Sleep(10000);
                    string[] argv = { };
                    Program.Main(argv);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Thread.Sleep(10000);
            string[] argv = { };
            Program.Main(argv);

        }
        finally {
            cleanup();
        }

    }

    public string mainLoop(int startTime)
    {
        Information information = new Information();
        ChannelActions ChanActs = new ChannelActions();
        ChanModes mode = new ChanModes();
        Loader load = new Loader();
        CTCP ctcp = new CTCP();
        UserControl users = new UserControl();
        httpRegex = new Regex(@"(?:http://(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?)(?:/(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*))*)(?:\?(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;:@&=])*))?)?)|(?:ftp://(?:(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?&=])*)(?::(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[;?&=])*))?@)?(?:(?:(?:(?:(?:[a-zA-Z\d](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?)\.)*(?:[a-zA-Z](?:(?:[a-zA-Z\d]|-)*[a-zA-Z\d])?))|(?:(?:\d+)(?:\.(?:\d+)){3}))(?::(?:\d+))?))(?:/(?:(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*)(?:/(?:(?:(?:[a-zA-Z\d$\-_.+!*'(),]|(?:%[a-fA-F\d]{2}))|[?:@&=])*))*)))");
        try
        {

            ThreadStart userThreadStart = new ThreadStart(users.userControl);
            Thread userThread = new Thread(userThreadStart);
            userThread.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
      
        while (true)
        {
            while ((inputLine = reader.ReadLine()) != null)
            {
                Console.WriteLine(inputLine);

                if (inputLine.IndexOf("376") >= 0) //End of MOTD
                {
                    writer.WriteLine("JOIN " + channel);
                    writer.Flush();
               
                 

             }
                else
                    break;

                while ((inputLine = reader.ReadLine()) != null)
                {                    
                    Console.WriteLine(inputLine);

                    if (ctcp.isCTCP())
                        ctcp.processCTCPType();

                    else if (inputLine == ("PING :" + server))
                    {
                        writer.WriteLine("PONG " + server);
                        writer.Flush();
                    }


                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": op me") >= 0 && information.sender() == owner)
                    {
                        mode.setMode("+o", owner);
                    }

                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": op") >= 0)
                    {
                        string realSender = information.sender();
                        string realMsg = information.msg();
                        information.sendNamesToSrv();
                        inputLine = reader.ReadLine();
                        if (information.isOped(realSender) || information.sender() == owner && information.senderHost() == ownerhost)
                            mode.setMode("+o", realMsg.Substring(nick.Length + 5));

                    }


                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": voice me") >= 0 || inputLine.ToLower().IndexOf(nick.ToLower() + ": voice ") >= 0)
                    {
                        mode.setMode("+v", information.sender());
                    }

                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": uptime") >= 0)
                    {
                        Uptime uptime = new Uptime();
                        uptime.uptime(startTime);

                    }

                    else if (inputLine.IndexOf("PART " + channel) >= 0)
                    {
                        users.userPart();
                
                    }


                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": die") >= 0 && information.sender() == owner)
                    {
                        writer.WriteLine("QUIT :My master killed me");
                        writer.Flush();
                        cleanup();

                        return "ok";
                    }
                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": topic") >=0  && information.sender() == owner)
                    {
                        mode.setTopic();
                    }
                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": !load") >= 0 && information.sender() == owner)
                    {
                       
                        load.createAD("load");

                    }
                    else if (inputLine.ToLower().IndexOf(nick.ToLower() + ": !unload") >= 0 && information.sender() == owner)
                    {

                        load.createAD("unload");

                    }



                   else if (Regex.IsMatch(inputLine, httpRegex.ToString()) && information.sender() != nick)
                    {

                        HttpHeader hh = new HttpHeader();
                        ThreadStart thStart = new ThreadStart(hh.processHeader);
                        Thread th = new Thread(thStart);
                        th.Start();

                    }


                    if (inputLine.StartsWith("ERROR"))
                    {
                        Console.WriteLine(inputLine);
                        return "error";

                    }

                }
            }
        }
    }

    public void cleanup()
    {
        writer.Close();
        reader.Close();
        irc.Close();
        nStream.Close();
    }

}

public class Information : IrcBot
{
    public string sender()
    {

        string[] array;
        array = IrcBot.inputLine.Split('!');
        string[] a = array[0].Split(':');

        return a[1].Trim();
    }

    public string senderHost()
    {

        string[] array;
        array = IrcBot.inputLine.Split('!');
        string[] a = array[1].Split(' ');

        return a[0].Trim();
    }

    public string msg()
    {
        string[] rawPieces;
        rawPieces = IrcBot.inputLine.Split(' ');
        string msg ="";

        if (rawPieces.Length > 3)
        {
            msg = rawPieces[3].Substring(1);
            for (int i = 4; i < rawPieces.Length; i++)
                msg = msg + " " + rawPieces[i];
        }
       
        return msg;
    }

    public string origin()
    {
        string[] array;
        array = IrcBot.inputLine.Split(' ');
        string msg = array[2];
        //Console.WriteLine(msg);
        return msg.ToLower();


    }

    public void sendNamesToSrv()
    {
        writer.WriteLine("NAMES {0}", channel);
        writer.Flush();

    }
   
    public ArrayList users()
    {
        ArrayList userList = new ArrayList();            
        string[] rawPieces;
        rawPieces = inputLine.Split(' ');

        if (rawPieces.Length >= 5)        
        {
            userList.Add(rawPieces[5].Substring(1));
            for (int i = 6; i < rawPieces.Length - 1; i++)
            {
                userList.Add(rawPieces[i]);
            }
            return userList;
        }

        else
            userList.Add("");

        return userList;
    }

    private ArrayList opedUsers(ArrayList userList)
    {
        ArrayList oped = new ArrayList();
        foreach(string text in userList)
            if (text.IndexOf("@") >= 0)
                oped.Add(text.Substring(1));

        return oped;
    }

    public bool isOped(string realSender)
    {        
        ArrayList OpedUsers = new ArrayList();
        OpedUsers = opedUsers(users());
        foreach (string text in OpedUsers)
            if (text == realSender)
                return true;

        return false;
    }

}

public class CTCP : IrcBot
{
    Information information = new Information();
    ChannelActions cAction = new ChannelActions();

    public bool isCTCP()
    {

        if (information.msg().IndexOf("\u0001") >= 0)
            return true;
       else
         return false;
       
   }

    public void processCTCPType()
    {

        if (IrcBot.inputLine.IndexOf("VERSION") >= 0)
            ctcpVersion();

        else if (IrcBot.inputLine.IndexOf("PING") >= 0)
            ctcpPing();
        else if (IrcBot.inputLine.IndexOf("TIME") >= 0)
            ctcpTime();      

    }

    private void ctcpVersion()
    {
        
        cAction.say("NOTICE", information.sender(), ("\u0001" + information.msg().Substring(1, 7) + " Espenfjos .NET ircbot:v.01:.NET\u0001"));

    }

    private void ctcpPing()
    {



        cAction.say("NOTICE", information.sender(), information.msg());
        //.say("NOTICE");

    }

    private void ctcpTime()
    {
        DateTime dt = new DateTime();
        cAction.say("NOTICE", information.sender(), ("\u0001" + information.msg().Substring(1, 4) + " " + dt.ToShortTimeString() + "\u0001"));
    }

}

public class ChannelActions : IrcBot
{   
    public void say(string inputLine)
    {                 
        try
        {
            writer.WriteLine("PRIVMSG {0} :{1}", channel, inputLine);
            writer.Flush();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void sayn(string inputLine)
    {
        try
        {
            writer.Write("PRIVMSG {0} :{1}", channel, inputLine);
            writer.Flush();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void contSay(string inputLine)
    {
        try
        {
            writer.Write(inputLine);
            writer.Flush();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void say(string format, string to, string inputLine)
    {
       
        writer.Write("{0} {1} :{2}\n", format, to, inputLine);
        writer.Flush();
    }
}

public class ChanModes : IrcBot
{
    public void setMode(string mode, string target)
    {
        IrcBot bot = new IrcBot();
        IrcBot.writer.WriteLine("MODE {0} {1} {2}", channel, mode, target);
        writer.Flush();
    }

    public void setTopic()
    {
        Information info = new Information();
        writer.WriteLine("TOPIC {0} :{1}", channel, info.msg().Substring(nick.Length + 8));
        writer.Flush();
    }


}

public class Uptime
{
    static public int startTimer()
    {
        DateTime now = DateTime.Now.ToUniversalTime();
        
        System.TimeSpan span = new System.TimeSpan(System.DateTime.Parse("1/1/1970").Ticks);
        System.DateTime time = now.Subtract(span);

        int startTime = (int)(time.Ticks / 10000000);
        return startTime;

    }

    public void uptime(int startTime)
    {
        ChannelActions chanact = new ChannelActions();
        DateTime now = DateTime.Now.ToUniversalTime();
        System.TimeSpan span = new System.TimeSpan(System.DateTime.Parse("1/1/1970").Ticks);
        System.DateTime time = now.Subtract(span);
        
        long upTimesec = (int)(time.Ticks / 10000000) - startTime;
        TimeSpan ts = new TimeSpan(0, 0, (int) upTimesec);

        int days = (int) ts.TotalDays;
        int hours = (int) ts.TotalHours;
        int minutes = (int)ts.TotalMinutes;
        int seconds = (int)ts.TotalSeconds;
        


        chanact.sayn("I have been up for ");
        if (days >= 1)
            chanact.contSay(days + " days, ");

        if (hours >= 1 && hours <= 60)
            chanact.contSay(hours + " hours, ");
        else if(hours > 60)
            chanact.contSay(hours - days * 60 + " hour and ");


        if (minutes >= 1 && minutes <= 60)
              chanact.contSay(minutes + " minutes and ");
        else if(minutes > 60)
              chanact.contSay(minutes - hours*60 + " minutes and ");

          if(seconds <= 60)
            chanact.contSay(seconds + " seconds\n");
         else if(seconds > 60)
            chanact.contSay(seconds - minutes*60 + " seconds\n");


    }

}

public class ReadConf
{
    public void init()
    {
        string file = "ircbot.conf";
        try
        {
            FileStream freader = new FileStream(file, FileMode.Open, FileAccess.Read);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }
}

public class HttpHeader : IrcBot
{
    public void processHeader()
    {
        IrcBot bot = new IrcBot();
        Information info = new Information();
        try
        {
            string[] httpInput = Regex.Split(inputLine, httpRegex.ToString(), RegexOptions.IgnoreCase);
            string[] input = info.msg().Split(' ');
            foreach (string text in input)
                if (Regex.IsMatch(text, httpRegex.ToString()))
                {

                    WebClient web = new WebClient();
                    byte[] bInHttpStream = web.DownloadData(text);
                    string inHttpString = Encoding.Default.GetString(bInHttpStream).Trim();
                    string[] data1 = Regex.Split(inHttpString.Trim(), "<title>|<TITLE>");
                    string[] titleContent = Regex.Split(data1[1].Trim(), "</title>|</TITLE>");

                    ChannelActions chanAct = new ChannelActions();
                    chanAct.say("[" + titleContent[0].Replace('\n',' ') + "]");
                }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        

    }

}

public class UserControl : IrcBot
{
    Information info = new Information();
    static ArrayList users = new ArrayList();

    public void userControl()
    {
        users = getUsers();
        if (inputLine.Trim().IndexOf("QUIT") >= 0 || inputLine.Trim().IndexOf("PART") >= 0)
            userPart();
        if (inputLine.Trim().IndexOf("JOIN") >= 0)
            userJoin();

    }

    public ArrayList getUsers()
    {
        users.Add(info.users());
        return users;
    }
   
    public void userPart()
    {     
        int i = 0;
        foreach (string text in UserControl.users)
            {
                i++;
                Console.WriteLine(text);
                if (text == info.sender())
                {

                    UserControl.users.RemoveAt(i);
                    Console.WriteLine(text);
                }
                else
                    Console.WriteLine("NEI");
            }

    }
    public void userJoin()
    {
        UserControl.users.Add(info.sender());
    }
}

