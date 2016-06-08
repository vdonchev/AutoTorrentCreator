namespace TorrentCreator.Exceptions
{
    using System;

    public class TorrentCreatorException : Exception
    {
        public TorrentCreatorException(string message) 
            : base(message)
        {
        }
    }
}