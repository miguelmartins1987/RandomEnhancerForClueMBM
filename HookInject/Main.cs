using ClueLauncher;
using CSharpFunctionalExtensions;
using EasyHook;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HookInject
{
    public class Main : IEntryPoint
    {
        private const int DICE_ROLL_RETURN_ADDRESS = 0x425849;
        private readonly ClueInterface Interface;
        private LocalHook RandHook;
        private LocalHook SrandHook;
        private LocalHook EGuiWindowManagerHook;

        public Main(
            RemoteHooking.IContext _1,
            string InChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<ClueInterface>(InChannelName);
            Interface.Ping();
        }

        public void Run(
            RemoteHooking.IContext _1,
            string _2)
        {
            try
            {
                RandHook = LocalHook.Create(
                    LocalHook.GetProcAddress("msvcrt.dll", "rand"),
                    new Drand(RandHooked),
                    this);

                RandHook.ThreadACL.SetExclusiveACL(new int[] { 0 });

                SrandHook = LocalHook.Create(
                    LocalHook.GetProcAddress("msvcrt.dll", "srand"),
                    new Dsrand(SrandHooked),
                    this);

                SrandHook.ThreadACL.SetExclusiveACL(new int[] { 0 });

                EGuiWindowManagerHook = LocalHook.Create(
                    LocalHook.GetProcAddress("!EagleM.dll", "??0EGuiWindowManager@@AAE@XZ"),
                    new DEGuiWindowManager(EGuiWindowManagerHooked),
                    this);

                EGuiWindowManagerHook.ThreadACL.SetExclusiveACL(new int[] { 0 });
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);

                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            RemoteHooking.WakeUpProcess();

            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    Interface.Ping();
                }
            }
            catch
            {
                // Ping() will raise an exception if host is unreachable
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr Drand();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void Dsrand(UIntPtr seed);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void DEGuiWindowManager(IntPtr obj);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr rand();

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void srand(UIntPtr seed);

        [DllImport("!EagleM.dll", CallingConvention = CallingConvention.ThisCall, EntryPoint = "??0EGuiWindowManager@@AAE@XZ")]
        static extern void EGuiWindowManager(IntPtr obj);

        static IntPtr RandHooked()
        {
            Main This = (Main)HookRuntimeInfo.Callback;
            IntPtr returnAddress = HookRuntimeInfo.ReturnAddress;
            if (returnAddress.ToInt32() == DICE_ROLL_RETURN_ADDRESS)
            {
                if (DiceRollController.Data.IsEmpty())
                {
                    This.Interface.LogInformation("Ran out of true random values! Fetching some more...");
                    DiceRollController.Initialize(This.Interface.GetSetting("ApiKey"));
                }

                long randomValue = DiceRollController.Data.PopFirstElement();
                This.Interface.LogInformation($"Got random value: {randomValue}");
                return (IntPtr)randomValue;
            }
            return rand();
        }

        static void SrandHooked(UIntPtr seed)
        {
            Main This = (Main)HookRuntimeInfo.Callback;
            IntPtr returnAddress = HookRuntimeInfo.ReturnAddress;
            This.Interface.LogInformation($"srand called! Return address: 0x{returnAddress:X4}");
            srand(seed);
        }

        static void EGuiWindowManagerHooked(IntPtr obj)
        {
            Main This = (Main)HookRuntimeInfo.Callback;
            This.Interface.LogInformation($"private: __thiscall EGuiWindowManager::EGuiWindowManager(void) called!");
            Task.Run(() => FetchTrueRandomValues(This));
            EGuiWindowManager(obj);
        }

        private static void FetchTrueRandomValues(Main This)
        {
            try
            {
                This.Interface.LogInformation("Fetching true random values...");
                Result result = DiceRollController.Initialize(This.Interface.GetSetting("ApiKey"));
                if (result.IsSuccess)
                {
                    This.Interface.LogInformation($"Fetched {DiceRollController.Data.Count} random values!");
                }
                else
                {
                    This.Interface.LogWarning($"Unable to fetch true random values! Reason: {result.Error}");
                }
            }
            catch (Exception ex)
            {

                This.Interface.ReportException(ex);
            }
        }
    }
}
