using System;
using System.Runtime.InteropServices;

namespace PROShine.Utils
{
    public class FlashWindowHelper
    {
        /// <summary>
        ///     Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FlashwStop = 0;

        /// <summary>
        ///     Flash the window caption.
        /// </summary>
        public const uint FlashwCaption = 1;

        /// <summary>
        ///     Flash the taskbar button.
        /// </summary>
        public const uint FlashwTray = 2;

        /// <summary>
        ///     Flash both the window caption and taskbar button.
        ///     This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FlashwAll = 3;

        /// <summary>
        ///     Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FlashwTimer = 4;

        /// <summary>
        ///     Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FlashwTimernofg = 12;

        /// <summary>
        ///     A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater => Environment.OSVersion.Version.Major >= 5;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref Flashwinfo pwfi);

        /// <summary>
        ///     Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                var fi = CreateFlashInfoStruct(hwnd, FlashwAll | FlashwTimernofg, uint.MaxValue, 0);

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static Flashwinfo CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
        {
            var fi = new Flashwinfo();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        ///     Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="hwnd">The handle of the Window to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd, uint count)
        {
            if (Win2000OrLater)
            {
                var fi = CreateFlashInfoStruct(hwnd, FlashwAll | FlashwTimernofg, count, 0);

                return FlashWindowEx(ref fi);
            }

            return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Flashwinfo
        {
            /// <summary>
            ///     The size of the structure in bytes.
            /// </summary>
            public uint cbSize;

            /// <summary>
            ///     A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;

            /// <summary>
            ///     The Flash Status.
            /// </summary>
            public uint dwFlags;

            /// <summary>
            ///     The number of times to Flash the window.
            /// </summary>
            public uint uCount;

            /// <summary>
            ///     The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink
            ///     rate.
            /// </summary>
            public uint dwTimeout;
        }
    }
}