using System;
using System.Collections.Generic;
using Earthwatchers.Models;

namespace Earthwatchers.Data
{
    public interface IEarthwatcherRepository
    {
        Earthwatcher GetEarthwatcher(int id);
        Earthwatcher GetEarthwatcher(string name, bool getLands);
        Earthwatcher GetEarthwatcherByGuid(Guid guid);
        Earthwatcher CreateEarthwatcher(Earthwatcher earthwatcher);
        List<Earthwatcher> GetAllEarthwatchers();
        ApiEw GetApiEw(string api, string userId);
        ApiEw GetApiEwById(int apiEwId);
        ApiEw CreateApiEwLogin(ApiEw ew);
        LandMini AssignLandToEarthwatcher(int earthwatcherid, string geohexKey);
        LandMini GetFreeLand(int regionId, string geohexKey);
        LandMini GetFreeLandByEarthwatcherIdAndPlayingRegion(int earthwatcherId, int playingRegion);

        bool EarthwatcherExists(string name);
        void UpdateEarthwatcher(int id, Earthwatcher earthwatcher);
        void SetEarthwatcherAsPowerUser(int id, Earthwatcher earthwatcher);
        void DeleteEarthwatcher(int id);
        void LinkApiAndEarthwatcher(int apiEwId, int EwId);
        void UpdateAccessToken(int Id, string AccessToken);
        void ChangePlayingRegion(Earthwatcher earthwatcher);
    }
}
