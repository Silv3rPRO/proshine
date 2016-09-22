using PROBot;
using PROProtocol;
using PROShine.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace PROShine
{
    public partial class ChatView : UserControl
    {
        private Dictionary<string, ButtonTab> _channelTabs;
        private Dictionary<string, ButtonTab> _pmTabs;
        private Dictionary<string, ButtonTab> _channelPmTabs; // fuck that
        private TabItem _localChatTab;
        private BotClient _bot;

        public ChatView(BotClient bot)
        {
            InitializeComponent();
            _bot = bot;
            _localChatTab = new TabItem();
            _localChatTab.Header = "Local";
            _localChatTab.Content = new ChatPanel();
            TabControl.Items.Add(_localChatTab);
            _channelTabs = new Dictionary<string, ButtonTab>();
            AddChannelTab("All");
            AddChannelTab("Trade");
            AddChannelTab("Battle");
            AddChannelTab("Other");
            AddChannelTab("Help");
            _pmTabs = new Dictionary<string, ButtonTab>();
            _channelPmTabs = new Dictionary<string, ButtonTab>();
        }

        public void Client_RefreshChannelList()
        {
            Dispatcher.InvokeAsync(delegate
            {
                IList<ChatChannel> channelList;
                lock (_bot)
                {
                    channelList = _bot.Game.Channels.ToArray();
                }
                foreach (ChatChannel channel in channelList)
                {
                    if (!_channelTabs.ContainsKey(channel.Name))
                    {
                        AddChannelTab(channel.Name);
                    }
                }
                foreach (string key in _channelTabs.Keys.ToArray())
                {
                    if (!(channelList.Any(e => e.Name == key)))
                    {
                        RemoveChannelTab(key);
                    }
                }
            });
        }

        public void Client_LeavePrivateMessage(string conversation, string mode, string leaver)
        {
            Dispatcher.InvokeAsync(delegate
            {
                if (leaver == _bot.Game.PlayerName)
                {
                    return;
                }
                AddPrivateSystemMessage(conversation, mode, leaver, "has closed the PM window");
            });
        }

        public void Client_ChatMessage(string mode, string author, string message)
        {
            Dispatcher.InvokeAsync(delegate
            {
                AddChatMessage(mode, author, message);
            });
        }

        public void Client_ChannelMessage(string channelName, string mod, string author, string message)
        {
            Dispatcher.InvokeAsync(delegate
            {
                AddChannelMessage(channelName, mod, author, message);
            });
        }

        public void Client_ChannelSystemMessage(string channelName, string message)
        {
            Dispatcher.InvokeAsync(delegate
            {
                AddChannelSystemMessage(channelName, message);
            });
        }

        public void Client_ChannelPrivateMessage(string conversation, string mode, string author, string message)
        {
            Dispatcher.InvokeAsync(delegate
            {
                AddChannelPrivateMessage(conversation, mode, author, message);
            });
        }

        public void Client_PrivateMessage(string conversation, string mode, string author, string message)
        {
            Dispatcher.InvokeAsync(delegate
            {
                PlayNotification();
                AddPrivateMessage(conversation, mode, author, message);
            });
        }

        public void Client_EmoteMessage(string mode, string author, int emoteId)
        {
            Dispatcher.InvokeAsync(delegate
            {
                AddEmoteMessage(mode, author, emoteId);
            });
        }

        private void AddChannelTab(string tabName)
        {
            ButtonTab tab = new ButtonTab();
            (tab.Header as ButtonTabHeader).TabName.Content = '#' + tabName;
            (tab.Header as ButtonTabHeader).CloseButton += () => CloseChannelTab(tabName);
            tab.Tag = tabName;
            tab.Content = new ChatPanel();
            _channelTabs[tabName] = tab;
            TabControl.Items.Add(tab);
        }

        private void CloseChannelTab(string channelName)
        {
            if (!_channelTabs.ContainsKey(channelName))
            {
                return;
            }
            if (_bot.Game != null && _bot.Game != null && _bot.Game.IsMapLoaded && _bot.Game.Channels.Any(e => e.Name == channelName))
            {
                _bot.Game.CloseChannel(channelName);
            }
            else
            {
                RemoveChannelTab(channelName);
            }
        }

        private void RemoveChannelTab(string tabName)
        {
            TabControl.Items.Remove(_channelTabs[tabName]);
            _channelTabs.Remove(tabName);
        }

        private void AddChannelPmTab(string tabName)
        {
            ButtonTab tab = new ButtonTab();
            (tab.Header as ButtonTabHeader).TabName.Content = "*" + tabName;
            (tab.Header as ButtonTabHeader).CloseButton += () => CloseChannelPmTab(tabName);
            tab.Tag = tabName;
            tab.Content = new ChatPanel();
            _channelPmTabs[tabName] = tab;
            TabControl.Items.Add(tab);
        }

        private void CloseChannelPmTab(string channelName)
        {
            if (!_channelPmTabs.ContainsKey(channelName))
            {
                return;
            }
            RemoveChannelPmTab(channelName);
        }

        private void RemoveChannelPmTab(string tabName)
        {
            TabControl.Items.Remove(_channelPmTabs[tabName]);
            _channelPmTabs.Remove(tabName);
        }
        private void AddPmTab(string tabName)
        {
            ButtonTab tab = new ButtonTab();
            (tab.Header as ButtonTabHeader).TabName.Content = tabName;
            (tab.Header as ButtonTabHeader).CloseButton += () => ClosePmTab(tabName);

            tab.Tag = tabName;
            tab.Content = new ChatPanel();
            _pmTabs[tabName] = tab;
            TabControl.Items.Add(tab);
        }

        private void ClosePmTab(string pmName)
        {
            if (!_pmTabs.ContainsKey(pmName))
            {
                return;
            }
            if (_bot.Game != null && _bot.Game != null && _bot.Game.IsMapLoaded && _bot.Game.Conversations.Contains(pmName))
            {
                _bot.Game.CloseConversation(pmName);
            }
            RemovePmTab(pmName);
        }

        private void RemovePmTab(string tabName)
        {
            TabControl.Items.Remove(_pmTabs[tabName]);
            _pmTabs.Remove(tabName);
        }

        private void AddChannelMessage(string channelName, string mode, string author, string message)
        {
            message = Regex.Replace(message, @"\[.+?\]", "");
            if (mode != null)
            {
                author = "[" + mode + "]" + author;
            }
            if (!_channelTabs.ContainsKey(channelName))
            {
                AddChannelTab(channelName);
            }
            MainWindow.AppendLineToTextBox((_channelTabs[channelName].Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] " + author + ": " + message);
        }

        private void AddChannelSystemMessage(string channelName, string message)
        {
            message = Regex.Replace(message, @"\[.+?\]", "");
            if (!_channelTabs.ContainsKey(channelName))
            {
                AddChannelTab(channelName);
            }
            MainWindow.AppendLineToTextBox((_channelTabs[channelName].Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] SYSTEM: " + message);
        }

        private void AddChannelPrivateMessage(string conversation, string mode, string author, string message)
        {
            message = Regex.Replace(message, @"\[.+?\]", "");
            if (mode != null)
            {
                author = "[" + mode + "]" + author;
            }
            if (!_channelPmTabs.ContainsKey(conversation))
            {
                AddChannelPmTab(conversation);
            }
            MainWindow.AppendLineToTextBox((_channelPmTabs[conversation].Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] " + author + ": " + message);
        }

        private void AddChatMessage(string mode, string author, string message)
        {
            message = Regex.Replace(message, @"\[.+?\]", "");
            if (mode != null)
            {
                author = "[" + mode + "]" + author;
            }
            MainWindow.AppendLineToTextBox((_localChatTab.Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] " + author + ": " + message);
        }

        private void AddEmoteMessage(string mode, string author, int emoteId)
        {
            if (mode != null)
            {
                author = "[" + mode + "]" + author;
            }
            MainWindow.AppendLineToTextBox((_localChatTab.Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] " + author + " is " + ChatEmotes.GetDescription(emoteId));
        }

        private void AddPrivateMessage(string conversation, string mode, string author, string message)
        {
            message = Regex.Replace(message, @"\[.+?\]", "");
            if (mode != null)
            {
                author = "[" + mode + "]" + author;
            }
            if (!_pmTabs.ContainsKey(conversation))
            {
                AddPmTab(conversation);
            }
            MainWindow.AppendLineToTextBox((_pmTabs[conversation].Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] " + author + ": " + message);
        }

        private void AddPrivateSystemMessage(string conversation, string mode, string author, string message)
        {
            message = Regex.Replace(message, @"\[.+?\]", "");
            if (mode != null)
            {
                author = "[" + mode + "]" + author;
            }
            if (!_pmTabs.ContainsKey(conversation))
            {
                AddPmTab(conversation);
            }
            MainWindow.AppendLineToTextBox((_pmTabs[conversation].Content as ChatPanel).ChatBox,
                "[" + DateTime.Now.ToLongTimeString() + "] " + author + " " + message);
        }

        private void InputChatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && _bot.Game != null && _bot.Game.IsMapLoaded)
            {
                SendChatInput(InputChatBox.Text);
                InputChatBox.Clear();
            }
        }

        private void SendChatInput(string text)
        {
            if (text == "" || text.Replace(" ", "") == "")
            {
                return;
            }
            lock (_bot)
            {
                if (_bot.Game == null)
                {
                    return;
                }
                TabItem tab = TabControl.SelectedItem as TabItem;
                text = Regex.Replace(text, @"\[(-|.{6})\]", "");
                if (text.Length == 0) return;
                if (_localChatTab == tab)
                {
                    text = text.Replace('|', '#');
                    _bot.Game.SendMessage(text);
                }
                else if (_channelTabs.ContainsValue(tab as ButtonTab))
                {
                    text = text.Replace('|', '#');
                    if (text[0] == '/')
                    {
                        _bot.Game.SendMessage(text);
                        return;
                    }
                    string channelName = (string)tab.Tag;
                    ChatChannel channel = _bot.Game.Channels.FirstOrDefault(e => e.Name == channelName);
                    if (channel == null)
                    {
                        return;
                    }
                    _bot.Game.SendMessage("/" + channel.Id + " " + text);
                }
                else if (_pmTabs.ContainsValue(tab as ButtonTab))
                {
                    text = text.Replace("|.|", "");
                    _bot.Game.SendPrivateMessage((string)tab.Tag, text);
                }
                else if (_channelPmTabs.ContainsValue(tab as ButtonTab))
                {
                    text = text.Replace('|', '#');
                    if (text[0] == '/')
                    {
                        _bot.Game.SendMessage(text);
                        return;
                    }
                    string conversation = (string)tab.Tag;
                    _bot.Game.SendMessage("/send " + conversation + ", " + text);
                }
            }
        }

        private void PlayNotification()
        {
            Window window = Window.GetWindow(this);
            if (!window.IsActive || !IsVisible)
            {
                IntPtr handle = new WindowInteropHelper(window).Handle;
                FlashWindowHelper.Flash(handle);

                if (File.Exists("Assets/message.wav"))
                {
                    using (SoundPlayer player = new SoundPlayer("Assets/message.wav"))
                    {
                        player.Play();
                    }
                }
            }
        }
    }
}
