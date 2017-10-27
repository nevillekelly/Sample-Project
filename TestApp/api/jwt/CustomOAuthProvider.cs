using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.jwt
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        string dbServer;
        string dbName;

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            var dbServerParm = context.Parameters.GetValues("dbserver").ToList();
            var dbNameParm = context.Parameters.GetValues("dbname").ToList();
            if(dbServerParm.Any() && dbNameParm.Any())
            {
                dbServer = dbServerParm.FirstOrDefault().ToLower();
                dbName = dbNameParm.FirstOrDefault().ToLower();
            }
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //header info
            var allowedOrigin = "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            //build connection string to validate with, use provided server and dbname or default if none
            string connStr;
            if (dbServer == "" || dbName == "")
            {
                connStr = ConfigurationManager.ConnectionStrings["sqlclientdb"].ConnectionString;

                var dbServerStart = connStr.IndexOf("Server=") + 7;
                var dbServerLen = connStr.IndexOf(";", dbServerStart) - dbServerStart;
                var dbNameStart = connStr.IndexOf("Database=") + 9;
                var dbNameLen = connStr.IndexOf(";", dbNameStart) - dbNameStart;

                dbServer = connStr.Substring(dbServerStart, dbServerLen);
                dbName = connStr.Substring(dbNameStart, dbNameLen);
            }
            else
            {
                connStr = "server=" + dbServer + ";Trusted_Connection=yes;database=" + dbName + ";";
            }

            //authenticate the user against tblUsers
            var authUser = await AuthenticateUser(connStr, context.UserName, context.Password);

            if (authUser == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            if (authUser.ActiveFlag == 1)
            {
                context.SetError("invalid_grant", "The user is inactive.");
                return;
            }

            if (authUser.LockFlag == 1)
            {
                context.SetError("invalid_grant", "The user is locked.");
                return;
            }

            //have a user, now get their permissions
            IList<Claim> securityClaimCollection = new List<Claim>();
            if (authUser.UserName != null)
            {
                securityClaimCollection = await GetUserPermissions(connStr, authUser.UserID);
            }

            //create basic claims for jwt to include db conn info
            IList<Claim> claimsCollection = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, authUser.UserName),
                    new Claim("userID", authUser.UserID.ToString()),
                    new Claim("dbServer", dbServer),
                    new Claim("dbName", dbName)
                };

            ClaimsIdentity oAuthIdentity = new ClaimsIdentity("JWT");
            oAuthIdentity.AddClaims(claimsCollection);

            //add in any permission specific claimns
            if (securityClaimCollection.Count > 0) { oAuthIdentity.AddClaims(securityClaimCollection); };

            var ticket = new AuthenticationTicket(oAuthIdentity, null);
            context.Validated(ticket);

        }

        private Task<User> AuthenticateUser(string connStr, string userName, string password)
        {
            string encPwd = encryptUserPwd(password);
            User result = new User { UserID = 1, UserName = "Sample User", ProductFlag = 2, Email = "sample@user.com", ActiveFlag = 0, LockFlag = 0, Role = "Site Administrators - All Permissions" };
            return Task.Run(() => result);
        }

        private Task<IList<Claim>> GetUserPermissions(string connStr, int userID)
        {


            List<string> securityNames = new List<string> { "Site Administrators - All Permissions" };

            IList<Claim> result = new List<Claim>();

            if (securityNames.Any(x => x == "Site Administrators - All Permissions" ||
                                       x == "Contract Profile - Full Permissions" ||
                                       x == "Contract Profile - Read Only")) { result.Add(new Claim("appPermission", "canViewContracts")); }

            if (securityNames.Any(x => x == "Site Administrators - All Permissions" ||
                                       x == "Contract Profile - Full Permissions")) { result.Add(new Claim("appPermission", "canEditContracts")); }

            if (securityNames.Any(x => x == "Site Administrators - All Permissions" ||
                                       x == "Contract Affiliations - Full Permissions" ||
                                       x == "Contract Affiliations - Read Only")) { result.Add(new Claim("appPermission", "canViewAffiliations")); }

            if (securityNames.Any(x => x == "Site Administrators - All Permissions" ||
                                       x == "Contract Affiliations - Full Permissions")) { result.Add(new Claim("appPermission", "canEditAffiliations")); }

            return Task.Run(() => result);
        }

        private string encryptUserPwd(string userPwd)
        {
            TripleDESCryptoServiceProvider desCrypto = new TripleDESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream();

            try
            {
                desCrypto.Key = truncateHash("Password", desCrypto.KeySize / 8);
                desCrypto.IV = truncateHash("", desCrypto.BlockSize / 8);

                byte[] bPwd = Encoding.Unicode.GetBytes(userPwd);

                CryptoStream encStream = new CryptoStream(ms, desCrypto.CreateEncryptor(), CryptoStreamMode.Write);
                encStream.Write(bPwd, 0, bPwd.Length);
                encStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Convert the encrypted stream to a printable string.
            return Convert.ToBase64String(ms.ToArray());
        }

        private byte[] truncateHash(string val, int len)
        {
            SHA1CryptoServiceProvider sha1Crypto = new SHA1CryptoServiceProvider();

            try
            {
                byte[] key = Encoding.Unicode.GetBytes(val);
                byte[] hash = sha1Crypto.ComputeHash(key);

                Array.Resize(ref hash, len);
                return hash;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}