using System;
using System.Globalization;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Input;
using System.Collections.Generic;
using Earthwatchers.UI.Resources;
using Earthwatchers.Models;
using Earthwatchers.UI.Models;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class WelcomeWindow
    {
        private int currentStep = 0;

        public delegate void PointsAddedEventHandler(object sender, EventArgs e);
        public event PointsAddedEventHandler PointsAdded;

        public WelcomeWindow()
        {
            InitializeComponent();

            this.StartStoryBoard.Completed += WelcomeStoryBoard_Completed;
            this.Welcome2StoryBoard.Completed += WelcomeStoryBoard_Completed;
            this.Welcome3StoryBoard.Completed += WelcomeStoryBoard_Completed;
            this.Welcome4StoryBoard.Completed += WelcomeStoryBoard_Completed;
            
            //Logo
            this.logo.Source = ResourceHelper.GetBitmap(string.Format("/Resources/Images/{0}", Labels.LogoPathMini));

            this.Loaded += WelcomeWindow_Loaded;
        }

        //Handlers
        void WelcomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WelcomeStoryBoard.Begin();
        }
        void WelcomeStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Next1.IsHitTestVisible = true;
        }
        
        //Button's Click
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            currentStep++;
            this.StartStoryBoard.Begin();

            Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.PreTutorialStep1.ToString(), ActionPoints.Points(ActionPoints.Action.PreTutorialStep1), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
            PointsAdded(this, EventArgs.Empty);
        }
        private void Next1_Click(object sender, RoutedEventArgs e)
        {
            this.Next1.IsHitTestVisible = false;

            currentStep++;
            if (currentStep == 2)
            {
                this.Welcome2.Visibility = System.Windows.Visibility.Visible;
                this.Welcome2StoryBoard.Begin();

                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.PreTutorialStep2.ToString(), ActionPoints.Points(ActionPoints.Action.PreTutorialStep2), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                PointsAdded(this, EventArgs.Empty);
            }
            else if (currentStep == 3)
            {
                this.Welcome1.Visibility = System.Windows.Visibility.Collapsed;
                this.Welcome3.Visibility = System.Windows.Visibility.Visible;
                this.Welcome3StoryBoard.Begin();

                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.PreTutorialStep3.ToString(), ActionPoints.Points(ActionPoints.Action.PreTutorialStep3), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                PointsAdded(this, EventArgs.Empty);
            }
            else if (currentStep == 4)
            {
                this.Welcome2.Visibility = System.Windows.Visibility.Collapsed;
                this.Welcome4.Visibility = System.Windows.Visibility.Visible;
                this.Welcome4StoryBoard.Begin();

                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.PreTutorialStep4.ToString(), ActionPoints.Points(ActionPoints.Action.PreTutorialStep4), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                PointsAdded(this, EventArgs.Empty);
            }
            else
            {
                this.Close();
            }
        }
        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}

