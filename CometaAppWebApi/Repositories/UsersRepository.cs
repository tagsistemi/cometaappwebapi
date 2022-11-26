using System;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using UsersClassLibrary.Models;
using CometaAppWebApi.Caches;

namespace CometaAppWebApi.Repositories
{
	public class UsersRepository
	{
		private IDbConnection _conn;
        private static UsersCache _usersCache = new UsersCache();

		public UsersRepository(IDbConnection connection)
		{
			this._conn = connection;
		}

		public async Task<User?> getUserById(String idUser)
		{
            User? cachedUser = _usersCache.get("idUser", idUser);
            if (cachedUser != null)
            {
                return cachedUser;
            }
			String queryString = @"SELECT * FROM Users WHERE idUser = @idUser";
			List<User> results = (await _conn.QueryAsync<User>(queryString, new { idUser })).ToList();
			if(results.Count() == 0)
			{
				return null;
			}
            User result = results[0];
            _usersCache.put("idUser", idUser, result);
            return result;
		}

		public async Task<User?> getUserByName(String Username)
        {
            String queryString = @"SELECT * FROM Users WHERE LOWER(Username) = LOWER(@Username)";
            List<User> results = (await _conn.QueryAsync<User>(queryString, new { Username })).ToList();
            if (results.Count() == 0)
            {
                return null;
            }
            User result = results[0];
            return results[0];
        }

        public async Task<User?> getUserByNameAndPassword(String Username, String Password)
        {
            String queryString = @"SELECT * FROM Users WHERE LOWER(Username) = LOWER(@Username) AND Password = @Password";
            List<User> results = (await _conn.QueryAsync<User>(queryString, new { Username, Password })).ToList();
            if (results.Count() == 0)
            {
                return null;
            }
            return results[0];
        }

        public async Task<Boolean> insertUser(User user)
        {
            String insertString = @"INSERT INTO Users(Username, Password) VALUES (@Username, @Password)";
            await _conn.ExecuteAsync(insertString, new { user.Username, user.Password });
            _usersCache.put("idUser", user.IdUser, user);
            return true;
        }

        public async Task<Boolean> updateUser(String idUser, User user)
        {
            await _conn.ExecuteAsync(@"UPDATE Users SET Username = @Username, Password = @Password WHERE idUser = @idUser", new { user.Username, user.Password, idUser });
            _usersCache.put("idUser", user.IdUser, user);
            return true;
        }

        public async Task<Boolean> deleteUser(User user)
        {
            await _conn.ExecuteAsync(@"DELETE FROM Users WHERE idUser = @idUser", new { user.IdUser });
            _usersCache.delete("idUser", user.IdUser);
            return true;
        }
    }
}

