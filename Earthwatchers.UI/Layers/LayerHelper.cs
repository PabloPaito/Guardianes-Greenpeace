using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui;
using Earthwatchers.Models;
using BruTile.PreDefined;
using BruTile.Web;

namespace Earthwatchers.UI.Layers
{
    public class LayerHelper
    {
        private readonly LayerCollection layerCollection;
        public List<ILayer> BaseLayers { get; private set; }
        public List<ILayer> Layers { get; private set; }
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler BaseLayersChanged;
        public event ChangedEventHandler LayersChanged;

        public LayerHelper(LayerCollection layerCollection)
        {
            Layers = new List<ILayer>();
            BaseLayers = new List<ILayer>();
            this.layerCollection = layerCollection;
        }

        public LayerCollection LayerCollection
        {
            get { return layerCollection; }
        }

        public void AddBaseLayer(ILayer layer)
        {
            BaseLayers.Add(layer);
            layerCollection.Add(layer);
            OnBaseLayersChanged(EventArgs.Empty);
        }

        public void AddLayer(ILayer layer, bool init=true)
        {
            if (init)
            {
                Layers.Add(layer);
                layerCollection.Add(layer); 
            }
           
            OnLayersChanged(EventArgs.Empty);
        }

        public void InsertLayer(ILayer layer, int index)
        {
            Layers.Insert(index, layer);
            layerCollection.Insert(index + 1, layer);

            OnLayersChanged(EventArgs.Empty);
        }

        public ILayer FindLayer(string name)
        {
            return Layers.FirstOrDefault(layer => layer.LayerName.Equals(name));
        }

        public void RemoveByLayerType<T>() where T: ILayer
        {
            this.Layers.RemoveAll(l => l is T && l.LayerName != Current.Instance.Earthwatcher.PlayingRegion + Constants.RegionLawLayerName);
            
            var currentLayers = layerCollection.Where(l => l is T && !(l is BaseTileLayer) || (l.LayerName.EndsWith(Constants.RegionLawLayerName))).ToList();
            foreach(var layer in currentLayers)
            {
                layerCollection.Remove(layer);
                Current.Instance.LayerHelper.LayerCollection.Remove(layer);
            }

        }
        public void RemoveLayer(string name)
        {
            var layers = layerCollection.FindLayer(name).ToList();
                        
            for (int i = 0; i < layers.Count(); i++)
            {
                layerCollection.Remove(layers[i]);
                Layers.Remove(layers[i]);
            }

            OnLayersChanged(EventArgs.Empty);
        }
        protected virtual void OnBaseLayersChanged(EventArgs e)
        {
            if (BaseLayersChanged != null)
                BaseLayersChanged(this, e);
        }

        protected virtual void OnLayersChanged(EventArgs e)
        {
            if (LayersChanged != null)
                LayersChanged(this, e);
        }

        public void addForestLawLayer(SatelliteImage image, int positionIndex)
        {
            var topLeft = SphericalMercator.FromLonLat(image.Extent.MinX, image.Extent.MinY);
            var bottomRight = SphericalMercator.FromLonLat(image.Extent.MaxX, image.Extent.MaxY);
            var newExtend = new BruTile.Extent(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);

            var schema = new SphericalMercatorWorldSchema();
            schema.Extent = newExtend;

            var max = image.MaxLevel + 1;
            while (schema.Resolutions.Count > max)
            {
                schema.Resolutions.RemoveAt(max);
            }
            var tms = new TmsTileSource(image.UrlTileCache, schema);
            //EL NOMBRE DEL LAW LEYER SIEMPRE ES EL MISMO: RegionId + Constants.RegionLawLayerName
            Current.Instance.LayerHelper.InsertLayer(new TileLayer(tms) { LayerName = Current.Instance.Earthwatcher.PlayingRegion + Constants.RegionLawLayerName, Opacity = 0, Tag = image.Published.Value }, positionIndex);
        }
    }
}
