using System;

namespace VisitorRegistry.Infrastructure
{
    public class LoginException : Exception
    {
        public LoginException(string message) : base(message) { }
    }
}
