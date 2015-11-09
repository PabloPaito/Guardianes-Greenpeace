using System.Windows;
using Earthwatchers.UI.Resources;
using Mapsui.Layers;
using Earthwatchers.UI.Layers;
using System.Windows.Media;
using System.Windows.Controls;
using System;

namespace Earthwatchers.UI.GUI.Controls
{
    public partial class LayerControlOnOff
    {
        private ILayer layer;

        public delegate void VisibilityChangedEventHandler(object sender, SharedEventArgs e);
        public event VisibilityChangedEventHandler VisibilityChanged;

        public LayerControlOnOff()
        {
            InitializeComponent();
        }

        private bool on = true;
        public LayerControlOnOff(ILayer layer, bool _on)
        {
            InitializeComponent();
            on = _on;

            if (!on)
            {
                this.SItext.Opacity = 0;
                this.NOtext.Opacity = 1;
                this.SliderCircleTranslateTransform.X = -27;
                this.OnOffButton.Background = new SolidColorBrush(Color.FromArgb(255, 31, 31, 31));
            }

            this.layer = layer;

            if (layer is HexagonLayer)
            {
                this.Title.Text = Labels.Layers13;
                ToolTipService.SetToolTip(this.SwitchGrid, Labels.layers15);
            }
            else if (layer is TileLayer)
            {
                this.Title.Text = Labels.Layers14;
                ToolTipService.SetToolTip(this.SwitchGrid, Labels.layers16);
            }
            else if (layer is BasecampLayer)
            {
                this.Title.Text = Labels.Layers17;
                ToolTipService.SetToolTip(this.SwitchGrid, Labels.Layers18);
            }
        }

        public string GetLayerName()
        {
            return layer.LayerName;
        }

        private static void RefreshMap()
        {
            Current.Instance.MapControl.OnViewChanged(true);
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (on)
            {
                this.OffAnimation.Begin();
                on = false;
                if (layer is HexagonLayer)
                {
                    ((HexagonLayer)layer).Opacity = 0;
                }
                else
                {
                    layer.Opacity = 0;
                }
            }
            else
            {
                this.OnAnimation.Begin();
                on = true;
                if (layer is HexagonLayer)
                {
                    ((HexagonLayer)layer).Opacity = 0.6;
                }
                else
                {
                    layer.Opacity = 1;
                }
                if(layer is TileLayer) //ForestLawLayer
                {
                    layer.Opacity = 0.4;
                    foreach(var l in Current.Instance.LayerHelper.LayerCollection)
                    {
                        if (l.LayerName == layer.LayerName)
                            l.Opacity = 0.4;
                    }
                }
                //if (layer is ArgentineLawLayer)
                //{
                //    if (((ArgentineLawLayer)layer).isFirstTime)
                //    {
                //        ((ArgentineLawLayer)layer).LoadData();
                //    }
                //}
                if (layer is BasecampLayer)
                {
                    if (((BasecampLayer)layer).isFirstTime)
                    {
                        ((BasecampLayer)layer).LoadData();
                    }
                }
            }
            if(VisibilityChanged != null)
                VisibilityChanged(this, new SharedEventArgs { Action = on.ToString() });
            RefreshMap();
        }

        public void HideHexagonBg()
        {
            ((HexagonLayer)layer).Opacity = 0;
        }
        public void ShowHexagonBg()
        {
            ((HexagonLayer)layer).Opacity = 0.6;
        }
        public bool IsOn()
        {
            return this.on;
        }

        public ILayer getLayer()
        {
            return this.layer;
        }
        public void setLayer(ILayer layer)
        {
            this.layer = layer;
        }
    }
}