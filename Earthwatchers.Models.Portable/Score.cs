using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Earthwatchers.Models
{
    public class Score
    {
        /// <summary>
        /// Crear un nuevo score,
        /// El campo Published Es obligatorio, pero si se deja en nulo se toma el DateTime.UtcNow
        /// </summary>
        public Score(int earthwatcherId, string action, int points, int playingRegion, int precisionPoints = 100, int? landId = null, DateTime? published = null, string param1 = "", string param2 = "")
        {
            precisionPoints = precisionPoints == 0 ? 100 : precisionPoints;
            this.EarthwatcherId = earthwatcherId;
            this.Action = action;
            this.Points = (points * precisionPoints) / 100;
            this.LandId = landId;
            this.RegionId = playingRegion;
            this.Param1 = param1;
            this.Param2 = param2;

            if (published != null)
                this.Published = (DateTime)published;
            else
                this.Published = DateTime.UtcNow;
        }

        //SI NO LO CREO TIRA UN ERROR EN ScoreRequests en el metodo Update. Necesita si o si un ctor sin parametros.
        /// <summary>
        /// NO USAR, NO USAR, NO USAR!!! De no estar este Ctor pincha la clase ScoreRequests. TODO: solucionarlo y borrar este Ctor
        /// </summary>
        public Score()
        {

        }

        public int Id { get; set; }
        public int EarthwatcherId { get; set; }
        public DateTime Published { get; set; }
        public string Action { get; set; }
        public int Points { get; set; }
        public int? LandId { get; set; }
        public int RegionId { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }

        public string Uri
        {
            get { return @"scores/" + Id.ToString(); }
        }
    }
}
