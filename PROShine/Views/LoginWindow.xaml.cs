using PROBot;
using PROProtocol;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PROShine
{
    public partial class LoginWindow : Window
    {
        private BotClient _bot;
        public bool ShowAccounts { get; set; }

        public string Username
        {
            get { return UsernameTextBox.Text.Trim(); }
        }

        public string Password
        {
            get { return PasswordTextBox.Password; }
        }

        public string Server
        {
            get { return ServerComboBox.Text.Trim().ToUpperInvariant().Split(' ')[0]; }
        }

        public string MacAddress
        {
            get { return !MacRandomCheckBox.IsChecked.Value ? MacAddressTextBox.Text.Trim() : null; }
        }

        public bool HasProxy
        {
            get { return ProxyCheckBox.IsChecked.Value; }
        }

        public int ProxyVersion
        {
            set { if (value == 4)
                {
                    Socks4RadioButton.IsChecked = true;
                }
                else if (value == 5)
                {
                    Socks5RadioButton.IsChecked = true;
                }
            }
            get { return Socks4RadioButton.IsChecked.Value ? 4 : 5; }
        }

        public string ProxyHost
        {
            get { return ProxyHostTextBox.Text.Trim(); }
        }

        public int ProxyPort { get; private set; }

        public string ProxyUsername
        {
            get { return ProxyUsernameTextBox.Text.Trim(); }
        }

        public string ProxyPassword
        {
            get { return ProxyPasswordTextBox.Password; }
        }

        private Regex macAddressRegex = new Regex("^[0-9A-F]{12}([0-9A-F]{12})?$");

        public LoginWindow(BotClient bot)
        {
            InitializeComponent();
            MacUseRandom_Checked(null, null);
            ProxyCheckBox_Checked(null, null);

            _bot = bot;

            Title = App.Name + " - " + Title;
            UsernameTextBox.Focus();

            ServerComboBox.ItemsSource = new List<string>() { "Red Server", "Blue Server", "Yellow Server" };
            ServerComboBox.SelectedIndex = 0;
            RefreshAccountList();
        }

        public void RefreshAccountList()
        {
            IEnumerable<Account> accountList;
            lock (_bot)
            {
                accountList = _bot.AccountManager.Accounts.Values.OrderBy(e => e.Name);
            }
            List<string> accountListView = new List<string>();
            foreach (Account account in accountList)
            {
                accountListView.Add(account.FileName);
            }

            AccountListView.ItemsSource = accountListView;
            AccountListView.Items.Refresh();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (Username.Length == 0)
            {
                UsernameTextBox.Focus();
                return;
            }
            if (Password.Length == 0)
            {
                PasswordTextBox.Focus();
                return;
            }

            string macAddress = MacAddress;
            if (macAddress != null && macAddress != HardwareHash.Empty && !macAddressRegex.IsMatch(macAddress))
            {
                MacAddressTextBox.Focus();
                return;
            }

            if (HasProxy)
            {
                int port;
                if (int.TryParse(ProxyPortTextBox.Text.Trim(), out port) && port >= 0 && port <= 65535)
                {
                    ProxyPort = port;
                    DialogResult = true;
                }
            }
            else
            {
                DialogResult = true;
            }
        }

        private void MacUseRandom_Checked(object sender, RoutedEventArgs e)
        {
            if (MacRandomCheckBox == null || MacAddressLabel == null || MacAddressTextBox == null || MacAddressPanel == null) return;

            Visibility macVisibility = MacRandomCheckBox.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible;
            MacAddressLabel.Visibility = macVisibility;
            MacAddressTextBox.Visibility = macVisibility;
            MacAddressPanel.Visibility = macVisibility;
        }

        private void MacRandomButton_Click(object sender, RoutedEventArgs e)
        {
            MacAddressTextBox.Text = HardwareHash.GenerateRandom();
        }

        private void MacRealButton_Click(object sender, RoutedEventArgs e)
        {
            MacAddressTextBox.Text = HardwareHash.RetrieveMAC();
        }

        private void MacEmptyButton_Click(object sender, RoutedEventArgs e)
        {
            MacAddressTextBox.Text = HardwareHash.Empty;
        }

        private void ProxyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            Visibility hasProxy = ProxyCheckBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            Visibility isSocks5 = ProxyCheckBox.IsChecked.Value && Socks5RadioButton.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            Visibility hasAuth = ProxyCheckBox.IsChecked.Value && Socks5RadioButton.IsChecked.Value && !AnonymousCheckBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            if (ProxyTypePanel != null)
            {
                ProxyTypePanel.Visibility = hasProxy;
            }
            if (ProxyHostLabel != null)
            {
                ProxyHostLabel.Visibility = hasProxy;
            }
            if (ProxyHostTextBox != null)
            {
                ProxyHostTextBox.Visibility = hasProxy;
            }
            if (ProxyPortLabel != null)
            {
                ProxyPortLabel.Visibility = hasProxy;
            }
            if (ProxyPortTextBox != null)
            {
                ProxyPortTextBox.Visibility = hasProxy;
            }
            if (AnonymousCheckBox != null)
            {
                AnonymousCheckBox.Visibility = isSocks5;
            }
            if (ProxyUsernameLabel != null)
            {
                ProxyUsernameLabel.Visibility = hasAuth;
            }
            if (ProxyPasswordLabel != null)
            {
                ProxyPasswordLabel.Visibility = hasAuth;
            }
            if (ProxyUsernameTextBox != null)
            {
                ProxyUsernameTextBox.Visibility = hasAuth;
            }
            if (ProxyPasswordTextBox != null)
            {
                ProxyPasswordTextBox.Visibility = hasAuth;
            }
        }

        private void ShowAccounts_Click(object sender, RoutedEventArgs e)
        {
            ShowAccounts = !ShowAccounts;
            if (ShowAccounts)
            {
                AccountList.Visibility = Visibility.Visible;
                AccountList.Width = 150;
                ShowAccountsButton.Content = "<";
            }
            else
            {
                AccountList.Width = 0;
                AccountList.Visibility = Visibility.Hidden;
                ShowAccountsButton.Content = ">";
            }
        }

        private void AccountListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountListView.SelectedItem == null)
            {
                return;
            }
            string accountName = AccountListView.SelectedItem.ToString();
            lock (_bot)
            {
                if (_bot.AccountManager.Accounts.ContainsKey(accountName))
                {
                    Account account = _bot.AccountManager.Accounts[accountName];
                    UsernameTextBox.Text = account.Name;
                    if (account.Password == null)
                    {
                        PasswordTextBox.Clear();
                    }
                    else
                    {
                        PasswordTextBox.Password = account.Password;
                    }
                    if (account.Server == "YELLOW")
                    {
                        ServerComboBox.SelectedIndex = 2;
                    }
                    else if (account.Server == "BLUE")
                    {
                        ServerComboBox.SelectedIndex = 1;
                    }
                    else
                    {
                        ServerComboBox.SelectedIndex = 0;
                    }
                    if (account.MacAddress != null)
                    {
                        MacRandomCheckBox.IsChecked = false;
                        MacAddressTextBox.Text = account.MacAddress;
                    }
                    else
                    {
                        MacRandomCheckBox.IsChecked = true;
                    }
                    if (account.Socks.Version != SocksVersion.None || account.Socks.Username != null || account.Socks.Password != null
                        || account.Socks.Host != null || account.Socks.Port != -1)
                    {
                        ProxyCheckBox.IsChecked = true;
                    }
                    else
                    {
                        ProxyCheckBox.IsChecked = false;
                    }
                    if (account.Socks.Version == SocksVersion.Socks4)
                    {
                        ProxyVersion = 4;
                    }
                    else if (account.Socks.Version == SocksVersion.Socks5)
                    {
                        ProxyVersion = 5;
                    }
                    if (account.Socks.Host == null)
                    {
                        ProxyHostTextBox.Clear();
                    }
                    else
                    {
                        ProxyHostTextBox.Text = account.Socks.Host;
                    }
                    if (account.Socks.Port == -1)
                    {
                        ProxyPortTextBox.Clear();
                    }
                    else
                    {
                        ProxyPortTextBox.Text = account.Socks.Port.ToString();
                    }
                    if (account.Socks.Username != null || account.Socks.Password != null)
                    {
                        AnonymousCheckBox.IsChecked = false;
                    }
                    else
                    {
                        AnonymousCheckBox.IsChecked = true;
                    }
                    if (account.Socks.Username == null)
                    {
                        ProxyUsernameTextBox.Clear();
                    }
                    else
                    {
                        ProxyUsernameTextBox.Text = account.Socks.Username;
                    }
                    if (account.Socks.Password == null)
                    {
                        ProxyPasswordTextBox.Clear();
                    }
                    else
                    {
                        ProxyPasswordTextBox.Password = account.Socks.Password;
                    }
                }
            }
        }

        private void SaveAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowAccounts)
            {
                ShowAccounts_Click(null, null);
            }
            if (UsernameTextBox.Text == null || UsernameTextBox.Text.Trim() == "")
            {
                return;
            }
            Account account = new Account(UsernameTextBox.Text.Trim());

            if (PasswordTextBox.Password != "" && PasswordTextBox.Password != null)
            {
                account.Password = PasswordTextBox.Password;
            }
            account.MacAddress = MacAddress;

            account.Server = Server;
            if (HasProxy)
            {
                SocksVersion socksVersion = SocksVersion.None;
                if (ProxyVersion == 4)
                {
                    socksVersion = SocksVersion.Socks4;
                }
                else if (ProxyVersion == 5)
                {
                    socksVersion = SocksVersion.Socks5;
                }
                account.Socks.Version = socksVersion;
                if (ProxyHostTextBox.Text != null && ProxyHostTextBox.Text.Trim() != "")
                {
                    account.Socks.Host = ProxyHostTextBox.Text.Trim();
                }
                if (ProxyPortTextBox.Text != null && ProxyPortTextBox.Text.Trim() != "")
                {
                    int port;
                    if (int.TryParse(ProxyPortTextBox.Text.Trim(), out port))
                    {
                        account.Socks.Port = port;
                    }
                }
                if (ProxyUsernameTextBox.Text != null && ProxyUsernameTextBox.Text.Trim() != "")
                {
                    account.Socks.Username = ProxyUsernameTextBox.Text.Trim();
                }
                if (ProxyPasswordTextBox.Password != null && ProxyPasswordTextBox.Password != "")
                {
                    account.Socks.Password = ProxyPasswordTextBox.Password;
                }
            }
            lock (_bot)
            {
                _bot.AccountManager.Accounts[account.Name] = account;
                _bot.AccountManager.SaveAccount(account.Name);
            }
            RefreshAccountList();
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem == null)
            {
                return;
            }
            string name = AccountListView.SelectedItem.ToString();
            lock (_bot)
            {
                if (_bot.AccountManager.Accounts.ContainsKey(name))
                {
                    _bot.AccountManager.DeleteAccount(name);
                    RefreshAccountList();
                }
            }
        }
    }
}
