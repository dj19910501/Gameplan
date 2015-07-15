using System;


namespace Integration.WorkFront
{
    class ClientException : Exception
    {
        public ClientException(string message) : base(message) { }
    }
}
