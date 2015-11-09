using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Earthwatchers.Models;
using System.Collections.Generic;
using Earthwatchers.UI.Requests;
using System;
using Earthwatchers.UI.Layers;
using System.Linq;
using Earthwatchers.UI.Resources;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class Ranking
    {
        private readonly ScoreRequests scoreRequests;
        private readonly ContestRequests contestRequests;

        public Ranking()
        {
            InitializeComponent();

            scoreRequests = new ScoreRequests(Constants.BaseApiUrl);
            contestRequests = new ContestRequests(Constants.BaseApiUrl);

            this.Loaded += Ranking_Loaded;
        }

        void Ranking_Loaded(object sender, RoutedEventArgs e)
        {
            scoreRequests.ScoresReceived += scoreRequests_ScoresReceived;
            scoreRequests.ScoresInterReceived +=scoreRequests_ScoresInterReceived;
            scoreRequests.ContestLeaderboardReceived += scoreRequests_ContestLeaderboardReceived;
            contestRequests.ContestReceived += contestRequests_ContestReceived;
            scoreRequests.GetLeaderBoardNationalRanking(Current.Instance.Earthwatcher.Id);
            scoreRequests.GetLeaderBoardInternationalRanking(Current.Instance.Earthwatcher.Id);
            contestRequests.GetContest(Current.Instance.Earthwatcher.PlayingRegion);
        }

        void contestRequests_ContestReceived(object sender, EventArgs e)
        {
            Contest contest = sender as Contest;
            if (contest != null)
            {
                scoreRequests.GetContestLeaderBoard(Current.Instance.Earthwatcher.Id);
                this.ContestFooterBorder.Visibility = System.Windows.Visibility.Visible;
                this.VerPremios.Visibility = System.Windows.Visibility.Visible;
                this.ContestFooterTextBox.Text = string.Format("{0} {1}", Labels.Ranking3, contest.EndDate.ToString("dd/MM/yyyy HH:mm:ss"));
                this.TitleContestTextBox.Text = string.Format("{0} {1}",Labels.Ranking, contest.ShortTitle);
                this.contestGrid.Visibility = System.Windows.Visibility.Visible;

                //Gifts Grid
                this.VerPremios.Visibility = Visibility.Visible;
                this.VolverAlRanking.Visibility = Visibility.Collapsed;
                this.TitleGiftsTextBox.Text = Labels.Prizes;
                this.TextGiftDescription.Text = contest.Description;
            }
            else
            {
                this.contestGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        void scoreRequests_ContestLeaderboardReceived(object sender, EventArgs e)
        {
            List<Rank> ranking = sender as List<Rank>;
            if (ranking == null || ranking.Count == 0)
            {
                this.contestGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.rankingContestList.ItemsSource = sender as List<Ranking>;
            }
        }

        void scoreRequests_ScoresReceived(object sender, EventArgs e)
        {
            var ranking = sender as List<Rank>;
            this.rankingList.ItemsSource = ranking;
            this.loadinAnim.Visibility = System.Windows.Visibility.Collapsed;
        }

        void scoreRequests_ScoresInterReceived(object sender, EventArgs e)
        {
            var ranking = sender as List<Rank>;
            this.InternationalrankingList.ItemsSource = ranking;
        }

        void Premios_Click(object sender, RoutedEventArgs e)
        {
            this.VerPremios.Visibility = Visibility.Collapsed;
            this.VolverAlRanking.Visibility = Visibility.Visible;
            this.contestGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.giftsGrid.Visibility = Visibility.Visible;
        }

        void VolverAlRanking_Click(object sender, RoutedEventArgs e)
        {
            this.VerPremios.Visibility = Visibility.Visible;
            this.VolverAlRanking.Visibility = Visibility.Collapsed;
            this.contestGrid.Visibility = System.Windows.Visibility.Visible;
            this.giftsGrid.Visibility = Visibility.Collapsed;
        }
    }
}

