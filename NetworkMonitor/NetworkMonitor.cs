using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkMonitor
{
    public partial class NetworkMonitor : Form
    {
        private List<Tuple<string, bool>> IpList = new List<Tuple<string, bool>>();
        private Dictionary<string, bool> PreviousStatus = new Dictionary<string, bool>();

        public NetworkMonitor()
        {
            InitializeComponent();

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Çıkış");
            exitItem.Click += ExitItem_Click;

            contextMenu.Items.Add(exitItem);

            notifyIcon1.ContextMenuStrip = contextMenu;
        }

        private void NetworkMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private async void NetworkMonitoring_Tick(object sender, EventArgs e)
        {
            await ScanNetwork();
        }

        private async Task ScanNetwork()
        {
            List<Task> pingTasks = new List<Task>();

            for (int i = 1; i <= 254; i++)
            {
                string ip = $"192.168.1.{i}";
                pingTasks.Add(NetworkPing(ip));
            }

            await Task.WhenAll(pingTasks);

            NetworkList.Items.Clear();

            foreach (var ip in IpList)
            {
                ListViewItem item = new ListViewItem("");

                item.SubItems.Add(ip.Item1);

                if (ip.Item2)
                    item.ForeColor = Color.Green;
                else
                    item.ForeColor = Color.Red;

                NetworkList.Items.Add(item);
            }
        }

        private async Task NetworkPing(string ip)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ip, 2000);

                var isConnected = reply.Status == IPStatus.Success;
                var existingItem = IpList.FirstOrDefault(x => x.Item1 == ip);

                if (existingItem == null)
                {
                    if (isConnected)
                    {
                        IpList.Add(new Tuple<string, bool>(ip, isConnected));
                        Notification("Device Joined", $"{ip} joined the network.");
                    }
                }
                else
                {
                    int index = IpList.IndexOf(existingItem);
                    IpList[index] = new Tuple<string, bool>(ip, isConnected);

                    if (PreviousStatus.ContainsKey(ip))
                    {
                        if (PreviousStatus[ip] && !isConnected)
                        {
                            Notification("Device Left", $"{ip} left the network.");
                        }
                        else if (!PreviousStatus[ip] && isConnected)
                        {
                            Notification("Device Joined", $"{ip} rejoined the network.");
                        }
                    }
                }

                PreviousStatus[ip] = isConnected;
            }
            catch { }
        }

        private void Notification(string title, string content)
        {
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.BalloonTipText = content;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(3000);
        }
    }
}
