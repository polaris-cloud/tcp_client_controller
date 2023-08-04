using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using testMailValidate.data;

namespace testMailValidate.services
{

    public class SqlUtil<T> : IDisposable, IAsyncDisposable where T : class
    {
        private readonly UserContext<T> _context;

        public SqlUtil(UserContext<T> context)
        {
            _context = context;
            context.Database.EnsureCreated();
        }



        public async Task AddUserAsync(T user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteUserAsync(object userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                //throw new ArgumentException("User not found.");
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public delegate void UpdateFiled(T oldFiled, T newFiled);

        /// <summary>
        /// updata operation
        /// </summary>
        /// <param name="user"></param>
        /// <param name="primaryKey">find key</param>
        /// <param name="selector">how to copy filed, put </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual async Task<bool> UpdateUserAsync(T user, object primaryKey, UpdateFiled selector)
        {
            var existingUser = await _context.Users.FindAsync(primaryKey);
            if (existingUser == null)
            {
                //throw new ArgumentException("User not found.");
                return false;
            }
            selector(existingUser, user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<T?> GetUserAsync(object userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<List<T>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }


        public void Dispose()
        {

            _context.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _context.DisposeAsync();
        }
    }
}
