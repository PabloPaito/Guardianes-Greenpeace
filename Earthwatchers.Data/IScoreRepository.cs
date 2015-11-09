using Earthwatchers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Earthwatchers.Data
{
    public interface IScoreRepository
    {
        List<Score> GetScoresByUserId(int id);
        List<Rank> GetLeaderBoardNationalRanking(bool isContest, int regionId);
        List<Rank> GetLeaderBoardInternationalRanking();
        Score PostScore(Score score);
        Score UpdateScore(Score score);
    }
}
