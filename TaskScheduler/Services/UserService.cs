using Microsoft.EntityFrameworkCore;
using TaskScheduler.Interfaces;
using TaskScheduler.Models;

namespace TaskScheduler.Services
{
    public class UserService: IUserService
    {

        private readonly DataBaseContext db;
        public UserService(DataBaseContext db) 
        {
            ArgumentNullException.ThrowIfNull(db);
            this.db = db;
        }
        public User Authorization(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Заполните все поля!");
            }
            User? user = db.Users.FirstOrDefault(u => u.Username == login);
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.HashPassword))
                {
                    SessionManager.StartSession(user);
                    return user;
                }
                else
                {
                    throw new InvalidOperationException("Неверный логин или пароль");
                }
            }
            else
            {
                throw new KeyNotFoundException("Учетной записи не существует");
            }
        }
        public User Registration(string login, string password, string verifyPassword)
        {
            if (db.Users.Any(u => u.Username == login))
            {
                throw new InvalidOperationException("Логин занят");
            }
            if (password != verifyPassword)
            {
                throw new InvalidOperationException("Пароли не совпадают");
            }
            string hashPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return SaveUser(login, hashPassword);

        }
        private User SaveUser(string login, string password)
        {
            try
            {
                User user = new User();
                user.Username = login;
                user.HashPassword = password;
                db.Users.Add(user);
                db.SaveChanges();
                SessionManager.StartSession(user);
                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Ошибка создания учетной записи", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Непредвиденная ошибка при создании учетной записи.", ex);
            }   
        }
    }
}
