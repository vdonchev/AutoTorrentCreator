namespace TorrentCreator
{
    using IO;
    using MonoTorrent.Common;

    public static class TorrentCreatorMain
    {
        public static void Main()
        {
            var io = new Io();
            var torrentCreator = new TorrentCreator();

            var app = new App(io, torrentCreator);
            app.Run();
        }
    }
}
