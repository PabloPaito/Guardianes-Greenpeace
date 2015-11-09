using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Earthwatchers.Models
{
    public class CustomShareText
    {
        public int Id { get; set; }
        public string ShareOk { get; set; }
        public string ShareAlert { get; set; }
        public string ShareAlertFinca{ get; set; }
        public int RegionId { get; set; }
        public string Language { get; set; }
        public string HashTagRegister { get; set; }
        public string HashTagReportConfirmed { get; set; }
        public string HashTagContestWon { get; set; }
        public string HashTagRanking { get; set; }
        public string HashTagTop1 { get; set; }
        public string HashTagCheck { get; set; }
        public string HashTagVerification { get; set; }
        public string HashTagDenounce { get; set; }
    }
}
