using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Reflection;
using System.Collections.Generic;

namespace TrayPcInfo
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                LogBuffer.Add("APP", "Eccezione UI non gestita.", e.Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogBuffer.Add("APP", "Eccezione non gestita nel dominio applicativo.", ex);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var form = new InfoForm())
            {
                form.WindowState = FormWindowState.Minimized;
                form.Load += (s, e) => { form.Hide(); form.ShowInTaskbar = false; };
                Application.Run(form);
            }
        }
    }

    internal static class LogBuffer
    {
        private static readonly object _sync = new object();
        private static readonly List<string> _lines = new List<string>();

        public static void Add(string category, string message, Exception ex = null)
        {
            try
            {
                lock (_sync)
                {
                    var line = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}{3}",
                        DateTime.Now,
                        category ?? "GEN",
                        message ?? "",
                        ex != null ? " - " + ex.Message : ""
                    );
                    _lines.Add(line);
                    if (_lines.Count > 200)
                        _lines.RemoveRange(0, _lines.Count - 50);
                }
            }
            catch
            {
            }
        }

        public static string Dump()
        {
            lock (_sync)
            {
                if (_lines.Count == 0)
                    return "(Nessun errore registrato)";
                return string.Join(Environment.NewLine, _lines.ToArray());
            }
        }
    }

    public class InfoForm : Form
    {
        private readonly NotifyIcon _tray;
        private readonly ToolStrip _toolbar;
        private readonly ToolStripButton _btnQuickAssist;
        private readonly Label _lblHeader;
        private readonly Label _lblVersion;
        private readonly ComboBox _cmbPrinters;
        private readonly LinkLabel _lnkSetDefault;
        private readonly TextBox _txtInfo;
        private readonly StatusStrip _status;
        private readonly ToolStripStatusLabel _lblStatus;
        private readonly Timer _refreshTimer;

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDefaultPrinter(string pszPrinter);

        private sealed class PrinterItem
        {
            public string Name { get; private set; }
            public bool IsDefault { get; private set; }

            public PrinterItem(string name, bool isDefault)
            {
                Name = name;
                IsDefault = isDefault;
            }

            public override string ToString()
            {
                return IsDefault ? Name + " (predefinita)" : Name;
            }
        }

        public InfoForm()
        {
            Text = "TrayPcInfo";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(820, 560);
            Icon = SystemIcons.Information;

            var cms = new ContextMenuStrip();
            var miApri = new ToolStripMenuItem("Apri", null, (s, e) => ShowFromTray());
            var miEsci = new ToolStripMenuItem("Esci", null, (s, e) => ExitApp());
            cms.Items.Add(miApri);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(miEsci);

            _tray = new NotifyIcon
            {
                Icon = Icon,
                Visible = true,
                ContextMenuStrip = cms
            };
            _tray.Text = BuildTrayText();
            _tray.DoubleClick += (s, e) => ShowFromTray();

            _lblHeader = new Label
            {
                Text = "System Information Monitor",
                AutoSize = true,
                Dock = DockStyle.Left,
                Padding = new Padding(6, 8, 6, 8)
            };
            var infoVer = GetInformationalVersion();
            _lblVersion = new Label
            {
                Text = "v" + infoVer,
                AutoSize = true,
                Dock = DockStyle.Right,
                Padding = new Padding(6, 8, 6, 8),
                TextAlign = ContentAlignment.MiddleRight
            };

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 34, BorderStyle = BorderStyle.FixedSingle };
            headerPanel.Controls.Add(_lblVersion);
            headerPanel.Controls.Add(_lblHeader);

            _toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            var assistBitmap = SystemIcons.Shield.ToBitmap();

            _btnQuickAssist = new ToolStripButton("Assistenza rapida")
            {
                Image = assistBitmap,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            _btnQuickAssist.Click += (s, e) => SafeExec("QUICKASSIST", LaunchQuickAssist);

            _toolbar.Items.Add(_btnQuickAssist);
            _toolbar.Padding = new Padding(2);

            var toolbarPanel = new Panel { Dock = DockStyle.Top, Height = 34, BorderStyle = BorderStyle.FixedSingle };
            toolbarPanel.Controls.Add(_toolbar);

            _cmbPrinters = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            _lnkSetDefault = new LinkLabel { Text = "Imposta predefinita", AutoSize = true, Padding = new Padding(8, 6, 8, 6) };
            _lnkSetDefault.LinkClicked += (s, e) => SafeExec("STAMPANTI", DoSetDefaultPrinter);

            var printersPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(6, 4, 6, 4),
                BorderStyle = BorderStyle.FixedSingle
            };
            printersPanel.Controls.Add(new Label { Text = "Stampanti:", AutoSize = true, Padding = new Padding(0, 8, 8, 0) });
            printersPanel.Controls.Add(_cmbPrinters);
            printersPanel.Controls.Add(_lnkSetDefault);

            _txtInfo = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericMonospace, 9f),
                TabStop = false,
                ShortcutsEnabled = true,
                HideSelection = false
            };

            var infoPanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            infoPanel.Controls.Add(_txtInfo);

            _status = new StatusStrip();
            _lblStatus = new ToolStripStatusLabel("Pronto");
            _status.Items.Add(_lblStatus);

            Controls.Add(infoPanel);
            Controls.Add(printersPanel);
            Controls.Add(toolbarPanel);
            Controls.Add(headerPanel);
            Controls.Add(_status);

            _refreshTimer = new Timer { Interval = 15000 };
            _refreshTimer.Tick += (s, e) => _tray.Text = BuildTrayText();
            _refreshTimer.Start();

            Load += (s, e) => { SafeExec("INIT", PopulatePrinters); SafeExec("INIT", RefreshAll); };
            Activated += (s, e) => { SafeExec("INIT", RefreshAll); };
            FormClosing += OnFormClosingHideToTray;
        }

        private void SafeExec(string category, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogBuffer.Add(category, "Errore in azione UI.", ex);
                _lblStatus.Text = "Errore: " + ex.Message;
                RefreshAll();
            }
        }

        private static string GetInformationalVersion()
        {
            try
            {
                var asm = typeof(InfoForm).Assembly;
                var attrs = asm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    var attr = attrs[0] as AssemblyInformationalVersionAttribute;
                    if (attr != null) return attr.InformationalVersion;
                }
            }
            catch (Exception ex)
            {
                LogBuffer.Add("SISTEMA", "Errore lettura versione informativa.", ex);
            }
            return Application.ProductVersion;
        }

        private void ExitApp()
        {
            try
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
            catch
            {
            }

            try { _tray.Visible = false; _tray.Dispose(); } catch { }
            Application.Exit();
        }

        private void OnFormClosingHideToTray(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
            }
        }

        private void ShowFromTray()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private string BuildTrayText()
        {
            string host = Environment.MachineName;
            string ip = GetPrimaryIPv4() ?? "N/A";
            string tip = host + " - " + ip;
            return Truncate(tip, 63);
        }

        private void PopulatePrinters()
        {
            try
            {
                _cmbPrinters.Items.Clear();
                var settings = new PrinterSettings();
                string defaultPrinter = settings.PrinterName;

                foreach (string p in PrinterSettings.InstalledPrinters)
                {
                    bool isDefault = string.Equals(p, defaultPrinter, StringComparison.OrdinalIgnoreCase);
                    _cmbPrinters.Items.Add(new PrinterItem(p, isDefault));
                }

                if (_cmbPrinters.Items.Count > 0)
                {
                    for (int i = 0; i < _cmbPrinters.Items.Count; i++)
                    {
                        var pi = _cmbPrinters.Items[i] as PrinterItem;
                        if (pi != null && pi.IsDefault)
                        {
                            _cmbPrinters.SelectedIndex = i;
                            _lblStatus.Text = "Stampante predefinita: " + pi.Name;
                            return;
                        }
                    }

                    _cmbPrinters.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LogBuffer.Add("STAMPANTI", "Errore in PopulatePrinters.", ex);
            }
        }

        private void DoSetDefaultPrinter()
        {
            var selected = _cmbPrinters.SelectedItem as PrinterItem;
            if (selected == null)
            {
                LogBuffer.Add("STAMPANTI", "Tentativo di impostare una stampante senza selezione.");
                _lblStatus.Text = "Seleziona una stampante.";
                RefreshAll();
                return;
            }

            string target = selected.Name;
            bool ok = false;
            try
            {
                ok = SetDefaultPrinter(target);
            }
            catch (Exception ex)
            {
                LogBuffer.Add("STAMPANTI", "Eccezione in SetDefaultPrinter.", ex);
            }

            if (ok)
            {
                PopulatePrinters();
                RefreshAll();
                _lblStatus.Text = "Stampante predefinita impostata: " + target;
                ShowPopup("Stampante predefinita impostata:\n" + target, 2500);
            }
            else
            {
                int err = Marshal.GetLastWin32Error();
                LogBuffer.Add("STAMPANTI",
                    "Impossibile impostare la stampante predefinita '" + target + "'. Codice errore: " + err,
                    null);
                _lblStatus.Text = "Errore impostando la stampante predefinita.";
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            try
            {
                _txtInfo.Text = BuildInfo();
                _lblStatus.Text = "Aggiornato: " + DateTime.Now.ToString("HH:mm:ss");
                _tray.Text = BuildTrayText();
                _tray.Icon = Icon;
            }
            catch (Exception ex)
            {
                LogBuffer.Add("SISTEMA", "Errore in RefreshAll.", ex);
            }
        }

        private static string BuildInfo()
        {
            var sb = new StringBuilder();
            string machine = Environment.MachineName;
            string domainUser;
            try
            {
                string user = Environment.UserName;
                string dom = Environment.UserDomainName;
                domainUser = string.IsNullOrWhiteSpace(dom) ? user : dom + "\\" + user;
            }
            catch (Exception ex)
            {
                LogBuffer.Add("SISTEMA", "Errore lettura utente.", ex);
                domainUser = Environment.UserName;
            }

            string dnsDomain = string.Empty;
            try
            {
                dnsDomain = IPGlobalProperties.GetIPGlobalProperties().DomainName ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogBuffer.Add("RETE", "Errore lettura DNS domain.", ex);
            }

            sb.AppendLine("=== SISTEMA ===");
            sb.AppendLine("Hostname: " + machine);
            sb.AppendLine("Utente: " + domainUser);
            sb.AppendLine("DNS domain: " + dnsDomain);
            sb.AppendLine();
            sb.AppendLine("=== RETE ===");

            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                foreach (var nic in nics)
                {
                    try
                    {
                        var ipProps = nic.GetIPProperties();
                        string nicName = nic.Name;
                        string desc = nic.Description;
                        string status = nic.OperationalStatus.ToString();
                        string speed = HumanSpeed(nic.Speed);
                        string suffix = ipProps.DnsSuffix ?? "";

                        var dhcp4Addrs = ipProps.DhcpServerAddresses != null
                            ? ipProps.DhcpServerAddresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                                .Select(a => a.ToString()).ToArray()
                            : new string[0];
                        var dhcp6Addrs = ipProps.DhcpServerAddresses != null
                            ? ipProps.DhcpServerAddresses.Where(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                                .Select(a => a.ToString()).ToArray()
                            : new string[0];

                        string mac = string.Join(":", nic.GetPhysicalAddress().GetAddressBytes()
                            .Select(b => b.ToString("X2")).ToArray());

                        var uni4 = ipProps.UnicastAddresses
                            .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork).ToList();
                        string ipv4 = uni4.Count > 0
                            ? string.Join(", ",
                                uni4.Select(u =>
                                {
                                    int? prefixLength = GetPrefixLengthSafe(u);
                                    return prefixLength.HasValue
                                        ? u.Address + "/" + PrefixToMask(prefixLength.Value)
                                        : u.Address.ToString();
                                }).ToArray())
                            : "N/D";

                        var gw4Addrs = ipProps.GatewayAddresses != null
                            ? ipProps.GatewayAddresses.Select(g => g.Address)
                                .Where(a => a != null && a.AddressFamily == AddressFamily.InterNetwork)
                                .Select(a => a.ToString()).ToArray()
                            : new string[0];
                        var dns4Addrs = ipProps.DnsAddresses != null
                            ? ipProps.DnsAddresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                                .Select(a => a.ToString()).ToArray()
                            : new string[0];

                        sb.AppendLine("NIC: " + nicName + " - " + desc);
                        sb.AppendLine("  Stato: " + status);
                        sb.AppendLine("  Velocità: " + speed);
                        sb.AppendLine("  DNS suffix: " + suffix);
                        sb.AppendLine("  DHCP v4: " + (dhcp4Addrs.Length > 0 ? string.Join(", ", dhcp4Addrs) : "N/D"));
                        sb.AppendLine("  DHCP v6: " + (dhcp6Addrs.Length > 0 ? string.Join(", ", dhcp6Addrs) : "N/D"));
                        sb.AppendLine("  MAC: " + mac);
                        sb.AppendLine("  IPv4: " + ipv4);
                        sb.AppendLine("  Gateway v4: " + (gw4Addrs.Length > 0 ? string.Join(", ", gw4Addrs) : "N/D"));
                        sb.AppendLine("  DNS v4: " + (dns4Addrs.Length > 0 ? string.Join(", ", dns4Addrs) : "N/D"));
                        sb.AppendLine();
                    }
                    catch (Exception exNic)
                    {
                        LogBuffer.Add("RETE", "Errore lettura NIC '" + nic.Name + "'.", exNic);
                    }
                }
            }
            catch (Exception ex)
            {
                LogBuffer.Add("RETE", "Errore enumerazione interfacce di rete.", ex);
            }

            sb.AppendLine("=== STAMPANTI ===");
            try
            {
                var printers = PrinterSettings.InstalledPrinters;
                var settings = new PrinterSettings();
                string def = settings.PrinterName;
                if (printers == null || printers.Count == 0)
                {
                    sb.AppendLine("(Nessuna stampante installata)");
                }
                else
                {
                    foreach (string p in printers)
                    {
                        string tag = string.Equals(p, def, StringComparison.OrdinalIgnoreCase)
                            ? " (predefinita)"
                            : "";
                        sb.AppendLine("- " + p + tag);
                    }
                }
            }
            catch (Exception ex)
            {
                LogBuffer.Add("STAMPANTI", "Errore enumerazione stampanti.", ex);
                sb.AppendLine("(Errore nel leggere le stampanti. Vedi log errori in fondo.)");
            }

            sb.AppendLine();
            sb.AppendLine("=== LOG ERRORI ===");
            sb.AppendLine(LogBuffer.Dump());

            return sb.ToString();
        }

        private static string HumanSpeed(long bps)
        {
            try
            {
                if (bps < 1000) return bps + " bps";
                double kb = bps / 1000.0;
                if (kb < 1000) return kb.ToString("0.##") + " Kbps";
                double mb = kb / 1000.0;
                if (mb < 1000) return mb.ToString("0.##") + " Mbps";
                double gb = mb / 1000.0;
                return gb.ToString("0.##") + " Gbps";
            }
            catch (Exception ex)
            {
                LogBuffer.Add("RETE", "Errore calcolo HumanSpeed.", ex);
                return bps + " bps";
            }
        }

        private static string PrefixToMask(int prefixLength)
        {
            try
            {
                if (prefixLength < 0 || prefixLength > 32)
                {
                    LogBuffer.Add("RETE", "Prefix length non valido: " + prefixLength + ".");
                    prefixLength = Math.Max(0, Math.Min(32, prefixLength));
                }
                uint mask = prefixLength == 0 ? 0u : uint.MaxValue << (32 - prefixLength);
                var bytes = BitConverter.GetBytes(mask).Reverse().ToArray();
                return string.Join(".", bytes.Select(b => b.ToString()).ToArray());
            }
            catch (Exception ex)
            {
                LogBuffer.Add("RETE", "Errore in PrefixToMask.", ex);
                return "0.0.0.0";
            }
        }

        private static int? GetPrefixLengthSafe(UnicastIPAddressInformation info)
        {
            try
            {
                return info.PrefixLength;
            }
            catch (Exception ex)
            {
                LogBuffer.Add("RETE", "Errore lettura prefix length.", ex);
                return null;
            }
        }

        private static string GetPrimaryIPv4()
        {
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                foreach (var nic in nics)
                {
                    var ipProps = nic.GetIPProperties();
                    var addr = ipProps.UnicastAddresses
                        .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork);
                    if (addr != null)
                        return addr.Address.ToString();
                }
            }
            catch (Exception ex)
            {
                LogBuffer.Add("RETE", "Errore in GetPrimaryIPv4.", ex);
            }
            return null;
        }

        private void LaunchQuickAssist()
        {
            ShowPopup("Avvio di Assistenza rapida...", 2000);
            bool started = false;
            try
            {
                var psi = new ProcessStartInfo("ms-quick-assist:") { UseShellExecute = true };
                Process.Start(psi);
                started = true;
            }
            catch (Exception ex)
            {
                LogBuffer.Add("QUICKASSIST", "Errore avvio tramite protocollo ms-quick-assist.", ex);
            }

            if (!started)
            {
                try
                {
                    string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    string sys32 = Path.Combine(windowsDir, "System32", "quickassist.exe");
                    string syswow = Path.Combine(windowsDir, "SysWOW64", "quickassist.exe");
                    string pathExe = null;
                    if (File.Exists(sys32)) pathExe = sys32;
                    else if (File.Exists(syswow)) pathExe = syswow;
                    else
                    {
                        var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator);
                        pathExe = paths.Select(p => Path.Combine(p, "quickassist.exe")).FirstOrDefault(File.Exists);
                    }

                    if (pathExe != null)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(pathExe) { UseShellExecute = true });
                            ShowPopup("Assistenza rapida avviata.", 2000);
                        }
                        catch (Exception ex)
                        {
                            LogBuffer.Add("QUICKASSIST", "Errore avvio quickassist.exe.", ex);
                        }
                    }
                    else
                    {
                        LogBuffer.Add("QUICKASSIST", "Quick Assist non trovato sul sistema.", null);
                    }
                }
                catch (Exception ex)
                {
                    LogBuffer.Add("QUICKASSIST", "Errore ricerca quickassist.exe.", ex);
                }
            }
            else
            {
                ShowPopup("Assistenza rapida avviata.", 2000);
            }
        }

        private void ShowPopup(string text, int durationMs)
        {
            try
            {
                _tray.BalloonTipTitle = "TrayPcInfo";
                _tray.BalloonTipText = text;
                _tray.BalloonTipIcon = ToolTipIcon.Info;
                _tray.ShowBalloonTip(durationMs);
            }
            catch (Exception ex)
            {
                LogBuffer.Add("SISTEMA", "Errore visualizzazione popup tray.", ex);
            }
        }
    }
}
