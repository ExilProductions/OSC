using MelonLoader;
using System.IO;

[assembly: MelonInfo(typeof(OSC.Core), "OSC", "1.0.0", "Exil_S", null)]

namespace OSC
{
    public class Core : MelonPlugin
    {
        public static OSCManager OSCManager { get; private set; }
        public static MelonLogger.Instance Logger { get; private set; }

        public override void OnPreInitialization()
        {
            Logger = LoggerInstance;
        }

        public override void OnInitializeMelon()
        {
            Logger.Msg("Initialized.");
            
            try
            {
                OSCManager = new OSCManager();
                
                OSCManager.Initialize();
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed to initialize OSC Manager: {ex.Message}");
            }
        }

        public override void OnApplicationQuit()
        {
            Logger.Msg("Shutting down.");
            OSCManager?.Shutdown();
        }

        public override void OnUpdate()
        {
            OSCManager?.Update();
        }
    }
}