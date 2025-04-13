

namespace TeammatoBackend.Abstractions
{
    // This class represents the data required for a user to sign in (login) to the system.
    public class SignInData
    {
        // The user's login or username.
        public string Login{get;set;}

        // The user's password, used to authenticate the user alongside the login.
        public string Password{get;set;}
    }
}