using System.Collections.Generic;
using Earthwatchers.Models;
using System;

namespace Earthwatchers.Data
{
    public interface ILandRepository
    {
        LandMini AddVerification(LandMini land, bool isAlert);
        List<Land> GetAll(int earthwatcherId, int playingRegion);
        List<LandCSV> GetLandsToConfirm(int page, bool showVerifieds, int regionId);
        Land GetLand(int id);
        Land GetTutorLand(int regionId);
        List<Land> GetLandByIntersect(string wkt, int landId);
        List<Land> GetLandByStatus(LandStatus status);
        void UnassignLand(int id);
        void UpdateLandStatus(int id, LandStatus landStatus);
        bool UpdateLandsDemand(List<Land> lands, int earthwatcherId = 0);
        List<Land> GetLandByEarthwatcherName(string earthwatcherName, int regionId);
        void AssignLandToEarthwatcher(string geohex, int earthwatcherid, int playingRegion);
        bool ResetLands(int regionId);
        LandMini ReassignLand(int earthwatcherId);
        Land GetLandByGeoHexKey(string geoHexKey);
        bool MassiveReassign(int basecamp);
        List<Score> GetLastUsersWithActivityScore(int landId);
        List<LandCSV> GetLandsCSV();
        List<Statistic> GetStats();
        List<string> GetVerifiedLandsGeoHexCodes(int earthwatcherId, bool isPoll);
        string GetImageBaseUrl(bool isBase, string geoHexKey);
        void AddPoll(LandMini land);
        bool ForceLandsReset(List<Land> lands, int earthwatcherId = 0);
        void LoadLandBasecamp(int fincaRegionId);
        List<Land> GetAllLands();
        decimal GetCheckPercentageByRegionId(int regionId);
        decimal? GetPrecicionDenouncePercentageByRegionId(int regionId, int earthwatcherId);
        void UpdateTutorLand(int landId, int regionId);
    }
}
