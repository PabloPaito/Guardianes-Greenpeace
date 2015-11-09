using System;
using System.Net;
using System.Collections.Generic;
using Earthwatchers.UI.Layers;
using Mapsui.Windows;
using Earthwatchers.Models;

namespace Earthwatchers.UI
{
    public class Current
    {
        private Current() { }
        private static Current _instance;                 
        public MapControl MapControl { get; set; }
        public LayerHelper LayerHelper { get; set; }
        public Earthwatcher Earthwatcher { get; set; } //Earthwatcher taken from provided username
        public Region Region { get; set; }
        public JaguarGame JaguarGame { get; set; }
        public Features Features { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public CustomShareText CustomShareTexts { get; set; }
        public List<Land> LandInView { get; set; } //Land show in the current view
        public List<Land> Lands { get; set; }
        public List<Score> AllCountriesScores { get; set; }
        public List<Score> RegionScores { get; set; }
        public List<Score> AddScore { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LastImageDate { get; set; }
        public Boolean IsAuthenticated { get; set; } //Is the earthwatcher authenticated
        public bool JaguarGameStarted { get; set; }
        public bool TutorialStarted { get; set; }
        public decimal? Precision { get; set; }
        public int PrecisionScore { get; set; }
        public int TutorialCurrentStep { get; set; }

        public static Current Instance
        {
            get
            {
                return _instance ?? (_instance = new Current());
            }
        }
    }
}
