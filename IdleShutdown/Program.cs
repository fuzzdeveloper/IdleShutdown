using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace IdleShutdown
{
    class Program
    {
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception("Call to GetLastInputInfo failed");
            }
            return lastInPut.dwTime;
        }

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(int informationLevel, IntPtr lpInputBuffer, uint nInputBufferSize, IntPtr lpOutputBuffer, uint nOutputBufferSize);

        private static uint GetSystemExecutionState()
        {
            int size = Marshal.SizeOf(typeof(ulong));
            IntPtr status = Marshal.AllocCoTaskMem(size);
            if (0 != CallNtPowerInformation(16, (IntPtr)null, 0, status, (uint)size))
            {
                Marshal.FreeCoTaskMem(status);
                throw new Exception("Call to CallNtPowerInformation failed");
            }
            uint statusVal = (uint)Marshal.ReadInt32(status);
            Marshal.FreeCoTaskMem(status);
            return statusVal;
        }

        static void Main(string[] args)
        {
            long msIdleBeforeShutdown = -1;
            int secondsShutdownTimer = -1;
            if (args.Length > 0)
            {
                try
                {
                    msIdleBeforeShutdown = Convert.ToInt32(args[0]) * 1000L;
                    secondsShutdownTimer = Convert.ToInt32(args[1]);
                }
                catch (FormatException)
                {
                    msIdleBeforeShutdown = -1;
                    secondsShutdownTimer = -1;
                }
            }
            if (msIdleBeforeShutdown < 0 || secondsShutdownTimer < 0)
            {
                System.Windows.Forms.MessageBox.Show("Usage:\n  IdleShutdown [idle-time] [shutdown-time]\n\nNote: Time specified in seconds", "Invalid Parameters Used");
                Environment.ExitCode = 1;
                return;
            }
			//run the idle detection and call to shutdown.exe in an endless loop - that way if user
			//aborts shutdown (via shutdown /a) we loop back to detecting next idle condition
            while (true)
            {
                while (true)
                {
					//GetSystemExecutionState() will return value other than 0 if movie player running, network share in use, etc
                    if (0 != GetSystemExecutionState())
                    {
                        Thread.Sleep((int)(int.MaxValue & msIdleBeforeShutdown));
                        continue;
                    }
                    long lastInputTime = Environment.TickCount - GetLastInputTime();
                    if (msIdleBeforeShutdown <= lastInputTime)
                        break;
                    Thread.Sleep((int)(int.MaxValue & (msIdleBeforeShutdown - lastInputTime)));
                }
                System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
                pProcess.StartInfo.FileName = @"shutdown.exe";
                pProcess.StartInfo.Arguments = "/s /t " + secondsShutdownTimer;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.Start();
                pProcess.WaitForExit();
                pProcess.Dispose();
                Thread.Sleep(secondsShutdownTimer * 1000 + 16000);
            }

        }
    }
}
