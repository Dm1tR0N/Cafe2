using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Cafe2.Models;

namespace Cafe2.Functions
{
    internal class AuthenticationManager
    {
        private readonly PostgresContext _dbContext;

        // Конструктор класса AuthenticationManager
        public AuthenticationManager(PostgresContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Метод для аутентификации пользователя
        public bool AuthenticateUser(string login, string password)
        {
            // Для проверки наличия пользователя в базе данных
            return _dbContext.Users.Any(u => u.Login == login && u.Password == password);
        }
    }
}
