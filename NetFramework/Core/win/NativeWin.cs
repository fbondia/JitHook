using System;

namespace Bugscout.Agent.Core
{
    public class NativeWin: NativeInterface
    {

        public IntPtr getJit()
        {
            return Native.getJitWindows();
        }

        public bool VirtualProtect(IntPtr lpAddress, uint dwSize, Headers.Protection protection, ref uint oldProtection)
        {

            Headers.ProtectionWindows p = Headers.ProtectionWindows.PAGE_NOACCESS;

            switch (protection)
            {
                case Headers.Protection.NONE:
                    p = Headers.ProtectionWindows.PAGE_NOACCESS;
                    break;
                case Headers.Protection.READ:
                    p = Headers.ProtectionWindows.PAGE_NOACCESS;
                    break;
                case Headers.Protection.WRITE:
                    p = Headers.ProtectionWindows.PAGE_READWRITE;
                    break;
                case Headers.Protection.READ_WRITE:
                    p = Headers.ProtectionWindows.PAGE_READWRITE;
                    break;

            }

            return Native.VirtualProtect(lpAddress, dwSize, p, ref oldProtection);

        }

    }
}
