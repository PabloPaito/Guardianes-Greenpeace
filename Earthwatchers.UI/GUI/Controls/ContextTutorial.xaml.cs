using Earthwatchers.Models;
using Earthwatchers.UI.Requests;
using Earthwatchers.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class ContextTutorial
    {
        public delegate void PointsAddedEventHandler(object sender, EventArgs e);
        public event PointsAddedEventHandler PointsAdded;
        private int step;

        public ContextTutorial()
        {
            InitializeComponent();
            this.step = 1;
            this.bgImage.Source = ResourceHelper.GetBitmap("/Resources/Images/ContextTutorials/bgImage.png");

            TutorialText1.Visibility = Visibility.Visible;
            TutorialText2.Visibility = Visibility.Collapsed;
            DeforestationImg.Visibility = Visibility.Collapsed;

            if(Current.Instance.Earthwatcher.PlayingRegion == 1) //Salta
            {
                TutorialText1.Text = Labels.ContextTut1Salta;
                TutorialText2.Text = Labels.ContextTut2Salta;
                DeforestationImg.Source = ResourceHelper.GetBitmap("/Resources/Images/ContextTutorials/Salta.jpg");
            }
            else //Chaco
            {
                TutorialText1.Text = Labels.ContextTut1Chaco;
                TutorialText2.Text = Labels.ContextTut2Chaco;
                DeforestationImg.Source = ResourceHelper.GetBitmap("/Resources/Images/ContextTutorials/Chaco.jpg");
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            step++;
            if (step > 2)
            {
                Current.Instance.AddScore.Add(new Score(Current.Instance.Earthwatcher.Id, ActionPoints.Action.ContextTutorialCompleted.ToString(), ActionPoints.Points(ActionPoints.Action.ContextTutorialCompleted), Current.Instance.Earthwatcher.PlayingRegion, Current.Instance.PrecisionScore));
                PointsAdded(this, EventArgs.Empty);
                this.Close();
            }
            else
            {
                TutorialText1.Visibility = Visibility.Collapsed;
                TutorialText2.Visibility = Visibility.Visible;
                DeforestationImg.Visibility = Visibility.Visible;
            }
        }

      
    }
}

