using Talleres360.Interfaces.Password;

namespace Talleres360.Services.Password
{
    public class BcryptPasswordService : IPasswordService
    {
        private const int _workFactor = 12;

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // Si el hash está corrupto o no es válido, devolvemos false por seguridad
                return false;
            }
        }
    }
}