using System.Threading.Tasks;

namespace gardenit_razor.Api
{
    public interface IApi
    {
        void Post(string req, object body);

        Task PostAsync(string req, object body);

        void Put(string req, object body);

        T Get<T>(string req);
        Task<T> GetAsync<T>(string req);

        Task Delete(string req);
    }
}