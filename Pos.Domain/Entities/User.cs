using System;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Entities
{
    /// <summary>
    /// Entidade Usuário - representa um usuário do sistema
    /// </summary>
    public class User : Entity
    {
        public string Login { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public bool Active { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime? PasswordChangedAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }
        public Guid TenantId { get; private set; }

        private User() { } // EF Core

        public User(string login, string name, string email, string passwordHash, UserRole role, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login não pode ser vazio", nameof(login));
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio", nameof(email));
            
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Hash da senha não pode ser vazio", nameof(passwordHash));

            Login = login;
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            TenantId = tenantId;
            Active = true;
            FailedLoginAttempts = 0;
        }

        public void UpdateBasicInfo(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));
            
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio", nameof(email));

            Name = name;
            Email = email;
            MarkAsUpdated();
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Hash da nova senha não pode ser vazio", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            PasswordChangedAt = DateTime.UtcNow;
            MarkAsUpdated();
        }

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
            MarkAsUpdated();
        }

        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            LockedUntil = null;
            MarkAsUpdated();
        }

        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;
            
            // Bloqueia após 5 tentativas por 30 minutos
            if (FailedLoginAttempts >= 5)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(30);
            }
            
            MarkAsUpdated();
        }

        public void Activate()
        {
            Active = true;
            MarkAsUpdated();
        }

        public void Deactivate()
        {
            Active = false;
            MarkAsUpdated();
        }

        public void Unlock()
        {
            FailedLoginAttempts = 0;
            LockedUntil = null;
            MarkAsUpdated();
        }

        public bool IsLocked()
        {
            return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
        }

        public bool CanLogin()
        {
            return Active && !IsLocked();
        }
    }

    public enum UserRole
    {
        Cashier,        // Operador de caixa
        Manager,        // Gerente
        Admin,          // Administrador
        Salesperson     // Vendedor mobile
    }
}
