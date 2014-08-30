using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GlobalHotkeys;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlobalHotkeys.Tests
{
    [TestClass()]
    public class HotKeyManagerTests
    {

        #region Native Functions to simulate the key presses:
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Post Message will not wait for the messageloop to process the send message, checks the return value to make sure it was successfull
        void PostMessageSafe(HandleRef hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            bool returnValue = PostMessage(hWnd, msg, wParam, lParam);
            if (!returnValue) { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        }
        #endregion


        // Unit Test Initalization, creates a fresh hotkey manager before every test
        private HotKeyManager manager;

        [TestInitialize]
        public void TestInitialize() { manager = new HotKeyManager(); }

        [TestCleanup]
        public void TestCleanup() { manager.Dispose(); }

        [TestMethod()]
        public void HotKeyTest()
        {
            // Register Hotkeys
            manager.RegisterHotKey(ModifierKeys.Control & ModifierKeys.Alt, Keys.P);
            manager.RegisterHotKey(ModifierKeys.Alt, Keys.O);

            // Event call counts
            int pCount = 0, oCount = 0;

            // Subscribe to Event
            manager.HotKeyPressed += delegate(object sender, HotKeyPressedEventArgs e)
            {
                if (e.Modifier == (ModifierKeys.Control & ModifierKeys.Alt) && e.Key == Keys.P)
                {
                    pCount += 1;
                }
                else if (e.Modifier == ModifierKeys.Alt && e.Key == Keys.O)
                {
                    oCount += 1;
                }
            };

            // TODO: Trigger the event Key Codes: SHIFT = + // CTRL = ^ // ALT = % 
            // Cannot use SendKeys.Send since it requires that the caller processes Windows Messages, which the testing library does not


            // Verify that the event was called atleast once.
            Assert.IsTrue(pCount > 0, "pCount did not increase, assume the event was not called");
            Assert.IsTrue(oCount > 0, "oCount did not increase, assume the event was not called");
        }

        [TestMethod()]
        public void MediaKeyTest()
        {
            // TODO: Add Unit test for MediaKey testing
            Assert.Fail();
        }


    }
}
