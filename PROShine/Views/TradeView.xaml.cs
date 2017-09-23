using PROBot;
using PROProtocol;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PROShine.Views
{
    public partial class TradeView : UserControl
    {
        private readonly BotClient _bot;

        public TradeView(BotClient bot)
        {
            InitializeComponent();
            _bot = bot;
            OnRequest.Visibility = Visibility.Collapsed;
            Trade.Visibility = Visibility.Collapsed;
            FinalView.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        public void Reset()
        {
            Dispatcher.Invoke(() =>
            {
                TeamToTrade.Children.Clear();
                WaitingTrade.Visibility = Visibility.Visible;
                Trade.Visibility = Visibility.Collapsed;
                OnRequest.Visibility = Visibility.Collapsed;

                FinalView.Visibility = Visibility.Collapsed;
                TradeControls.Visibility = Visibility.Visible;
                TeamToTrade.Visibility = Visibility.Visible;

                FirstList.SetValue(BorderBrushProperty, Brushes.Silver);
                FirstList.BorderThickness = new Thickness(1);
                SecondList.SetValue(BorderBrushProperty, Brushes.Silver);
                SecondList.BorderThickness = new Thickness(1);
                FirstList.ItemsSource = null;
                SecondList.ItemsSource = null;
            });
        }

        public void TradeRequest(string applicant)
        {
            Dispatcher.Invoke(() =>
            {
                TradeApplicant.Text = applicant;
                OnRequest.Visibility = Visibility.Visible;
                WaitingTrade.Visibility = Visibility.Collapsed;
            });
        }

        private void AcceptTrade_Click(object sender, RoutedEventArgs e)
        {
            var applicant = TradeApplicant.Text;
            _bot.Game.SendPacket("mb|.|/trade " + applicant);
            Dispatcher.Invoke(() =>
            {
                if (TeamToTrade.Children.Count < _bot.Game.Team.Count)
                    InitTeam();
            });
            Trade.Visibility = Visibility.Visible;
            OnRequest.Visibility = Visibility.Collapsed;
        }

        public void UpdateMoney(string[] data)
        {
            Dispatcher.Invoke(() =>
            {
                if (TeamToTrade.Children.Count < _bot.Game.Team.Count)
                    InitTeam();
                Trade.Visibility = Visibility.Visible;
                OnRequest.Visibility = Visibility.Collapsed;
                WaitingTrade.Visibility = Visibility.Collapsed;
                FirstNickname.Text = data[1]; // First exchanger
                SecondNickname.Text = data[2]; // Second
                FirstMoney.Text = '$' + data[3]; // First money on exchange
                SecondMoney.Text = '$' + data[4]; // Second money on exchange
            });
        }

        public void InitTeam()
        {
            var team = _bot.Game.Team;
            team.ForEach(delegate (Pokemon pkmn)
            {
                var b = new ToggleButton();
                b.Margin = new Thickness(5, 0, 5, 0);
                b.Content = pkmn.Name;
                b.Click += setTradeInfos_Click;
                TeamToTrade.Children.Add(b);
            });
        }

        private void cancelOnTrade_Click(object sender, RoutedEventArgs e)
        {
            _bot.Game.SendMessage("ftradeadd,1");
            Reset();
        }

        private void setTradeInfos_Click(object sender, RoutedEventArgs e)
        {
            var pokemonsToTrade = "";
            for (var i = 0; i < TeamToTrade.Children.Count; i++)
                pokemonsToTrade += (((ToggleButton)TeamToTrade.Children[i]).IsChecked == true
                                       ? Convert.ToString(i + 1)
                                       : "0") + ",";
            _bot.Game.SendMessage("ftradeadd,0," + Money.Text + "," + pokemonsToTrade);
        }

        private void acceptOnTrade_Click(object sender, RoutedEventArgs e)
        {
            _bot.Game.SendMessage("ftradeadd,2");
            if (FinalView.Visibility == Visibility.Visible)
                Reset();
        }

        public void StatusChanged(string[] data)
        {
            var sdata = data[1].Split('|');
            if (sdata[0] == "1")
                Dispatcher.Invoke(() =>
                {
                    FirstList.SetValue(BorderBrushProperty, Brushes.ForestGreen);
                    FirstList.BorderThickness = new Thickness(2);
                });
            else if (sdata[0] == "0")
                Dispatcher.Invoke(() =>
                {
                    FirstList.SetValue(BorderBrushProperty, Brushes.Silver);
                    FirstList.BorderThickness = new Thickness(2);
                });

            if (sdata[1] == "1")
                Dispatcher.Invoke(() =>
                {
                    SecondList.SetValue(BorderBrushProperty, Brushes.ForestGreen);
                    SecondList.BorderThickness = new Thickness(2);
                });
            else if (sdata[1] == "0")
                Dispatcher.Invoke(() =>
                {
                    SecondList.SetValue(BorderBrushProperty, Brushes.Silver);
                    SecondList.BorderThickness = new Thickness(2);
                });
        }

        public void StatusReset()
        {
            Dispatcher.Invoke(() =>
            {
                FirstList.SetValue(BorderBrushProperty, Brushes.Silver);
                FirstList.BorderThickness = new Thickness(1);
                SecondList.SetValue(BorderBrushProperty, Brushes.Silver);
                SecondList.BorderThickness = new Thickness(1);
            });
        }

        public void ChangeToFinalView()
        {
            Dispatcher.Invoke(() =>
            {
                TradeControls.Visibility = Visibility.Collapsed;
                TeamToTrade.Visibility = Visibility.Collapsed;
                FinalView.Visibility = Visibility.Visible;
            });
        }
    }
}