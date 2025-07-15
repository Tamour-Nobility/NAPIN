using System.Collections.Generic;

namespace NPMAPI.Repositories
{
    public interface IRepository
    {
        IEnumerable<string> GetAll();
    }
}