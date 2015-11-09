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
using Earthwatchers.Models;
using Earthwatchers.UI.Requests;
using Earthwatchers.UI.Resources;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class PostTutorialTasks
    {

        public PostTutorialTasks()
        {
            InitializeComponent();

            this.TasksTitle.Text = Labels.TasksTitle;
            this.Task1.Text = Labels.Task1;
            this.Task2.Text = Labels.Task2;
            this.Task3.Text = Labels.Task3;
            this.Task4.Text = Labels.Task4;
            this.Task5.Text = Labels.Task5;
            this.Task6.Text = Labels.Task6;

            this.Task1.Visibility = Visibility.Visible;
            this.Task2.Visibility = Visibility.Visible;
            this.Task3.Visibility = Visibility.Visible;
            this.Task4.Visibility = Visibility.Visible;
            this.Task5.Visibility = Visibility.Visible;
            this.Task6.Visibility = Visibility.Visible;

            this.HideTask1.Completed += HideTask1_Completed;
            this.HideTask2.Completed += HideTask2_Completed;
            this.HideTask3.Completed += HideTask3_Completed;
            this.HideTask4.Completed += HideTask4_Completed;
            this.HideTask5.Completed += HideTask5_Completed;
            this.HideTask6.Completed += HideTask6_Completed;
        }

        public void CheckDoneList(List<Score> scores, bool isFirst)
        {

            if(scores.Any(x => x.Action == ActionPoints.Action.LandStatusChanged.ToString()))
            {
                if(isFirst)
                    this.Task1.Visibility = Visibility.Collapsed;
                else
                    this.HideTask1.Begin();
            }
            if (scores.Any(x => x.Action == ActionPoints.Action.ConfirmationAdded.ToString()))
            {
                if (isFirst)
                    this.Task2.Visibility = Visibility.Collapsed;
                else
                    this.HideTask2.Begin();
            }
            if (scores.Any(x => x.Action == ActionPoints.Action.Shared.ToString()))
            {
                if (isFirst)
                    this.Task3.Visibility = Visibility.Collapsed;
                else
                    this.HideTask3.Begin();
            }
            if (scores.Any(x => x.Action == ActionPoints.Action.LandReassigned.ToString()))
            {
                if (isFirst)
                    this.Task4.Visibility = Visibility.Collapsed;
                else
                    this.HideTask4.Begin();
            }
            if (scores.Count(x => x.Action == ActionPoints.Action.Login.ToString()) > 5)
            {
                if (isFirst)
                    this.Task5.Visibility = Visibility.Collapsed;
                else
                    this.HideTask5.Begin();
            }
            if (scores.Count(x => x.Action == ActionPoints.Action.ConfirmationAdded.ToString()) > 5)
            {
                if (isFirst)
                    this.Task6.Visibility = Visibility.Collapsed;
                else
                    this.HideTask6.Begin();
            }

            if((Task1.Opacity == 0 || Task1.Visibility == Visibility.Collapsed)
            && (Task2.Opacity == 0 || Task2.Visibility == Visibility.Collapsed)
            && (Task3.Opacity == 0 || Task3.Visibility == Visibility.Collapsed)
            && (Task4.Opacity == 0 || Task4.Visibility == Visibility.Collapsed)
            && (Task5.Opacity == 0 || Task5.Visibility == Visibility.Collapsed)
            && (Task6.Opacity == 0 || Task6.Visibility == Visibility.Collapsed))
            {
                this.MainGrid.Opacity = 0;
                this.MainBorder.Opacity = 0;
            }
            else
            {
                this.MainGrid.Opacity = 1;
                this.MainBorder.Opacity = 1;
            }
        }

        //Callbacks - Animation to hide tasks
        void HideTask1_Completed(object sender, EventArgs e)
        {
            this.Task1.Visibility = Visibility.Collapsed;
        }
        void HideTask2_Completed(object sender, EventArgs e)
        {
            this.Task2.Visibility = Visibility.Collapsed;
        }
        void HideTask3_Completed(object sender, EventArgs e)
        {
            this.Task3.Visibility = Visibility.Collapsed;
        }
        void HideTask4_Completed(object sender, EventArgs e)
        {
            this.Task4.Visibility = Visibility.Collapsed;
        }
        void HideTask5_Completed(object sender, EventArgs e)
        {
            this.Task5.Visibility = Visibility.Collapsed;
        }
        void HideTask6_Completed(object sender, EventArgs e)
        {
            this.Task6.Visibility = Visibility.Collapsed;
        }
    }
}

