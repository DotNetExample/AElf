using System;

namespace AElf.Cryptography.ECDSA.Exceptions
{
    public class PublicKeyOperationException : Exception
    {
        public PublicKeyOperationException(string msg) : base(msg)
        {
            
        }
    }
}