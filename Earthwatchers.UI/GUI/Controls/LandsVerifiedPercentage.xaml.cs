using Earthwatchers.Models;
using Earthwatchers.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class LandsVerifiedPercentage
    {
        public LandsVerifiedPercentage()
        {
            InitializeComponent();
            this.TextContent.Text = "";
        }

        public void SetPercentage(decimal percentage)
        {
            if (percentage != 0)
            {
                this.ProgressBarLoading.Visibility = Visibility.Collapsed;
                this.TextContent.Text = Labels.PercentageVPlots + percentage + "%";
                this.ProgressBar.Value = Convert.ToDouble(percentage);
            }
            else
            {
                this.ProgressBarLoading.Visibility = Visibility.Visible;
            }
        }

        
    }
}

