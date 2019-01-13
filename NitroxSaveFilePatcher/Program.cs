using System;
using System.Globalization;
using System.Threading;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxServer;

namespace NitroxSaveFilePatcher
{



    class Program
    {

        public static bool FreshWorld = false;
        public static bool UpgradeWorld = false;
        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug | Log.LogLevel.FileLog);
            //NitroxServiceLocator.InitializeDependencyContainer(new ServerAutoFacRegistrar());
            //NitroxServiceLocator.BeginNewLifetimeScope();

            //configureCultureInfo();
            //Server server;
            //try
            //{
            //    server = NitroxServiceLocator.LocateService<Server>();
            //    //server.Start();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //    return;
            //}

            Console.WriteLine("Searching SaveFiles.. ");
            Console.WriteLine("Backup SaveFile... !");
            Patcher patchobject = new Patcher();
            bool Result = patchobject.Patching();
            if(Result == true && FreshWorld == false && UpgradeWorld == true)
            {
                Console.WriteLine("SaveFile found an Upgraded !!");
            }
            else if(FreshWorld == false)
            {
                Console.WriteLine("SaveFiles is Version 5, nothing to Upgrading");
            }
            else
            {
                Console.WriteLine("Creating FreshWolrd... Done!");
            }
            Console.ReadLine();
        }
        /**
 * Internal subnautica files are setup using US english number formats and dates.  To ensure
 * that we parse all of these appropriately, we will set the default cultureInfo to en-US.
 * This must best done for any thread that is spun up and needs to read from files (unless 
 * we were to migrate to 4.5.)  Failure to set the context can result in very strange behaviour
 * throughout the entire application.  This originally manifested itself as a duplicate spawning
 * issue for players in Europe.  This was due to incorrect parsing of probability tables.
 */
        static void configureCultureInfo()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");

            // Although we loaded the en-US cultureInfo, let's make sure to set these incase the 
            // default was overriden by the user.
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = ",";

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }

}
    
