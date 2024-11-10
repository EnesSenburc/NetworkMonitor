using System;
using System.Threading;
using System.Windows.Forms;

namespace NetworkMonitor
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(true, "{c2c2ba98-1953-4d7e-aa47-4448ccc1f19b}", out bool createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("This application is already working.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new NetworkMonitor());
            }
        }
    }
}
