using System;
using System.Security.Cryptography;

namespace JWTAuthDemo.Helpers
{
    internal static class PaasswordHashing
    {
        static internal string HashPassword(string pass)
        {
            //Create the salt value with a cryptographic PRNG
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //Create the Rfc2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            //Combine the salt and password bytes for later use
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            //return the combined salt+hash into a string
            return Convert.ToBase64String(hashBytes);
        }

        static internal bool Verify(string hasPass, string passToCheck)
        {

            // Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(hasPass);

            // Get the salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Compute the hash on the password the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(passToCheck, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            bool passOK = true;
            // Compare the results
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    passOK = false;

            return passOK;
        }
    }
}