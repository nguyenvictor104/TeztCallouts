using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace TeztCallouts
{
    public class Main : Plugin
    {
        string pluginName = "TeztCallouts";


        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChanged;
            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
            Game.LogTrivial(string.Format("Plugin:{0} has been intialized.", pluginName));
            Game.LogTrivial(string.Format("Go on duty to fully load {0}", pluginName));
            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
        }

        public override void Finally()
        {
            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
            Game.LogTrivial(pluginName + " has been cleaned up");
            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
        }

        void OnOnDutyStateChanged(bool onDuty)
        {

            if (onDuty)
            {
                RegisterCallouts();

                Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
                Game.LogTrivial(string.Format("{0} has been successfully loaded.", pluginName));
                Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));

                Game.DisplayNotification(string.Format("{0} has been successfully loaded.", pluginName));
            }

        }
        private static void RegisterCallouts()
        {
            //Functions.RegisterCallout(typeof(Callouts.TeztSuspiciousVehicle));
            Functions.RegisterCallout(typeof(Callouts.TeztSuspiciousPerson));
        }

        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()))
                {
                    return assembly;
                }
            }

            return null;
        }

        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
