using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Data;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace IRC_bot
{
    class Program
    {


        public static void Main(string[] args)
        {
            
            Uptime uptime = new Uptime();
            int startTime = Uptime.startTimer();

            IrcBot bot = new IrcBot();

            bot.connections(startTime);


        }
    }

    public class Loader
    {
        public void createAD(string action)
        {
              ObjectHandle oh;
              if (action == "load")
              {
                  try
                  {
                      AppDomain ad = AppDomain.CreateDomain("Plugins");
                      IrcBot bot = new IrcBot();

                      oh = ad.CreateInstance(
                           "Plugins",                                   // the assembly name
                          "Plugins.plugin1",                             // the type name with namespace
                          false,                                          // ignore case
                          System.Reflection.BindingFlags.CreateInstance,  // flag
                          null,                                           // binder
                          new object[] { IrcBot.writer, IrcBot.inputLine },                            // args
                          null,                                           // culture
                          null,                                           // activation attributes
                          null);                                         // security attributes

                      

                  }
                  catch (Exception e)
                  {
                      Console.WriteLine(e.ToString());
                      Environment.Exit(1);
                  }
              }
              if (action == "unload")
                oh = null;
          


        }


    }
}

