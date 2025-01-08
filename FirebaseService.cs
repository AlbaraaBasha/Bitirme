using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bitirme
{
    public class FireBaseServices
    {
        private readonly FirebaseClient firebaseClient;

        public FireBaseServices()
        {
            // Initialize Firebase client with the database URL
            firebaseClient = new FirebaseClient("https://bitirmeprojesi-897ce-default-rtdb.firebaseio.com/");
        }

        // Add User
        public async Task<User?> AddUser(User user)
        {
            try
            {
                var result = await firebaseClient
                  .Child("Users")
                  .PostAsync(user);
                return user; // Return the added user object
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                return null;
            }
        }

        public async Task<User?> GetUser(string email, string password)
        {
            try
            {
                var allUsers = await firebaseClient
                  .Child("Users")
                  .OnceAsync<User>();

                return allUsers.FirstOrDefault(u => u.Object.Username == email && u.Object.Password == password)?.Object;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var users = await firebaseClient
                .Child("Users")
                .OrderBy("Username")  // Use the correct method depending on your SDK version
                .EqualTo(username)
                .OnceAsync<User>();

            return users.FirstOrDefault()?.Object; // Return the user if found, otherwise null
        }

        public async Task<string> GetUserName(string username)
        {
            // Query the database for the user with the specified username
            var users = await firebaseClient
                .Child("Users")
                .OrderBy("Username")
                .EqualTo(username)
                .OnceAsync<User>();

            // If a matching user is found, return their Name
            return users.FirstOrDefault()?.Object?.Name;
        }



        public async Task<string?> GetKey(string username)
        {
            try
            {
                // Fetch all users from the database
                var allUsers = await firebaseClient
                  .Child("Users")
                  .OnceAsync<User>();

                // Find the user by username
                var user = allUsers.FirstOrDefault(u => u.Object.Username == username)?.Object;

                // Return the encryption key if the user is found
                return user?.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching key: {ex.Message}");
                return null;
            }
        }



    }

    // User class to represent user data
    public class User
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
