using System.Threading.Tasks;
using app_api.Data.Models;

namespace app_api.V1.Repos.UserRepo
{
    public interface IUserRepo
    {
         Task<User> CreateNewUser(User userToCreate, string password, string keytabFilePath);
         Task<bool> UserExists(string username);
         Task<byte[]> GetUserKeyTab(string username, string password);
    }
}