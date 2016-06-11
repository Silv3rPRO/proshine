using System;
using System.Runtime.InteropServices;

namespace PROShine
{
    public class FlashWindowHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;

        /// <summary>
        /// Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        /// Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="hwnd">The handle of the Window to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, count, 0);

                return FlashWindowEx(ref fi);
            }

            return false;
        }

        /// <summary>
        /// A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater
        {
            get { return Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
