using System;

namespace TestApp.jwt
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public byte ProductFlag { get; set; }
        public string Email { get; set; }
        public byte UserType { get; set; }
        public byte ProviderFlag { get; set; }
        public byte LockFlag { get; set; }
        public DateTime PasswordExpirationDate { get; set; }
        public string SchFileType { get; set; }
        public byte SchZipFlag { get; set; }
        public byte ActiveFlag { get; set; }
        public int PMMCAccount { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public bool IsOnline { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
        public string Company { get; set; }
        public string Role { get; set; }
        public string Title { get; set; }

    }
}