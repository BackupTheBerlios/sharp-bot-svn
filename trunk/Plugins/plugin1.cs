#region Using directives

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Drawing;

#endregion

namespace Plugins
{
    public class plugin1 : MarshalByRefObject
    {
      
        public plugin1(StreamWriter writer, string inputLine)
        {
            Console.WriteLine( "[{0}] Event{1}", System.AppDomain.CurrentDomain.FriendlyName, "Shape constructor");
            Console.WriteLine(inputLine + " FISK");
            
           
        }

        public void sayHello(StreamWriter writer)
        {
            writer.WriteLine("PRIVMSG #el-csharp :HEI fra modul1");
            writer.Flush();


        }
    }
}
