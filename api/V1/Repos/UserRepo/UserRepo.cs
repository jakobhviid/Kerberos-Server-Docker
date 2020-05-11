using System.Threading.Tasks;
using app_api.Data.Models;
using app_api.Data;
using app_api.helpers;
using System.IO;
using System;

using Microsoft.EntityFrameworkCore;

namespace app_api.V1.Repos.UserRepo
{
    public class UserRepo : IUserRepo
    {
        private readonly DataContext _context;

        public UserRepo(DataContext context)
        {
            _context = context;
        }

        public async Task<User> CreateNewUser(User userToCreate, string password, string keytabFilePath) {
            byte[] passwordHash, passwordSalt;
            CryptographyHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);

            userToCreate.PasswordHash = passwordHash;
            userToCreate.PasswordSalt = passwordSalt;

            userToCreate.KeyTabFile = File.ReadAllBytes(keytabFilePath);
            userToCreate.KeyTabFilePath = keytabFilePath;

            await _context.Users.AddAsync(userToCreate);
            await _context.SaveChangesAsync();

            return userToCreate;
        }
        
        public async Task<bool> UserExists(string username) {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<byte[]> GetUserKeyTab(string username, string clearPassword) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) {
                return null;
            }
            if (!CryptographyHelper.PasswordsMatch(clearPassword, user.PasswordHash, user.PasswordSalt)) {
                return null;
            }
            return user.KeyTabFile;
        }
    }
}