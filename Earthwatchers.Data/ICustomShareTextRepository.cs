using Earthwatchers.Models;
using System.Collections.Generic;

namespace Earthwatchers.Data
{
    public interface ICustomShareTextRepository
    {
        List<CustomShareText> GetAll();
        CustomShareText GetByRegionIdAndLanguage(int regionId, string language);
        CustomShareText GetById(int id);
        CustomShareText Insert(CustomShareText shareText);
        CustomShareText Edit(CustomShareText shareText);
        void Delete(int id);
    }
}
