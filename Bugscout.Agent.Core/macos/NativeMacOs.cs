using System;

namespace Bugscout.Agent.Core
{
    public class NativeMacOs: NativeInterface
    {

        public IntPtr getJit()
        {
            return Native.getJitMacOs();
        }

        public bool VirtualProtect(IntPtr lpAddress, uint dwSize, Headers.Protection protection, ref uint oldProtection)
        {

            Headers.ProtectionPosix p = Headers.ProtectionPosix.PROT_NONE;

            switch(protection)
            {
                case Headers.Protection.NONE:
                    p = Headers.ProtectionPosix.PROT_NONE;
                    break;
                case Headers.Protection.READ:
                    p = Headers.ProtectionPosix.PROT_READ;
                    break;
                case Headers.Protection.WRITE:
                    p = Headers.ProtectionPosix.PROT_WRITE;
                    break;
                case Headers.Protection.READ_WRITE:
                    p = Headers.ProtectionPosix.PROT_WRITE;
                    break;

            }

            /*
            int main(void)
            {
                //  New value to write into foo+5.
                int NewValue = 23;

                //  Find page size for this system.
                size_t pagesize = sysconf(_SC_PAGESIZE);

                //  Calculate start and end addresses for the write.
                uintptr_t start = (uintptr_t) &foo + 5;
                uintptr_t end = start + sizeof NewValue;

                //  Calculate start of page for mprotect.
                uintptr_t pagestart = start & -pagesize;

                //  Change memory protection.
                if (mprotect((void *) pagestart, end - pagestart,
                        PROT_READ | PROT_WRITE | PROT_EXEC))
                {
                    perror("mprotect");
                    exit(EXIT_FAILURE);
                }
            }

            https://books.google.com.br/books?id=zgwqDwAAQBAJ&pg=PA99&lpg=PA99&dq=mono+unix+mprotect&source=bl&ots=fkE7IFeTIg&sig=ACfU3U0h5fsYGG7dQUzIe0xSoET2ZT_kXA&hl=pt-BR&sa=X&ved=2ahUKEwi7hrWEmcDnAhVKILkGHe2EAnYQ6AEwB3oECAoQAQ#v=onepage&q=mono%20unix%20mprotect&f=false
             */

            
            long pageSize = Mono.Unix.Native.Syscall.sysconf(Mono.Unix.Native.SysconfName._SC_PAGESIZE);
            long ps = ~(pageSize - 1);

            long lpAddL = lpAddress.ToInt64();

            IntPtr alignPtr = (IntPtr)(lpAddL & ps);

            //int resultsx = Mono.Unix.Native.Syscall.mprotect(alignPtr, dwSize, Mono.Unix.Native.MmapProts.PROT_WRITE);
            //
            int results = Native.mprotect(alignPtr, dwSize, 0x04 | 0x02 | 0x01);

            if (results!=0)
            {
                // Mono.Unix.UnixMarshal.ThrowExceptionForLastError();
                return false;
            }
            //
            

            return true;

        }

    }
}
