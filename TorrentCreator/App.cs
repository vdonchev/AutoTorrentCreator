namespace TorrentCreator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using Exceptions;
    using IO;
    using MonoTorrent.Common;
    using Renci.SshNet;
    using Utils;

    public class App
    {
        private readonly TorrentCreator torrentCreator;
        private readonly Io io;
        
        private string sourceDir;
        private string destinationDir;
        private NameValueCollection settings;
        private string[] announces;
        private string[] directories;
        private List<string> torrentsToUpload;

        public App(Io io, TorrentCreator torrentCreator)
        {
            this.io = io;
            this.torrentCreator = torrentCreator;
        }

        public void Run()
        {
            try
            {
                this.LoadSettings();
                
                this.LoadDirsToUpload();

                this.InitializeTorrentCreator();
                this.io.Write("Loading settings completed!", Colors.Green);

                this.ProcessDirs();
                this.io.Write("Processing directories for upload completed!", Colors.Green);

                if (bool.Parse(this.settings["sbUpload"]))
                {
                    this.UploadToSeedbox();
                    this.io.Write("Files uploaded to seedbox!", Colors.Green);
                }

                if (bool.Parse(this.settings["siteUpload"]))
                {
                    this.UploadToWebsite();
                    this.io.Write("Files uploaded to website!", Colors.Green);
                }

                if (bool.Parse(this.settings["exportLinks"]))
                {
                    this.ExportLinks();
                    this.io.Write("Download links exported!", Colors.Green);
                }

                this.io.Write("All done!", Colors.Green);
                this.io.Read();
            }
            catch (TorrentCreatorException ex)
            {
                this.io.Write(ex.Message, Colors.Red);
            }
            catch (Exception exception)
            {
                this.io.Write(exception.Message, Colors.Red);
            }
        }

        private void ExportLinks()
        {
            var export = new StringBuilder();
            foreach (var torrent in this.torrentsToUpload)
            {
               export.AppendLine(this.settings["siteTorrentUrl"] + torrent);
            }

            File.WriteAllText("export.txt", export.ToString().Trim());
        }

        private void UploadToWebsite()
        {
            using (var client = new SftpClient(
                this.settings["siteHost"],
                int.Parse(this.settings["sitePort"]),
                this.settings["siteUsername"],
                this.settings["sitePassword"]))
            {
                client.Connect();
                client.ChangeDirectory(this.settings["siteUploadDir"]);

                foreach (var torrent in this.torrentsToUpload)
                {
                    var uploadfile = Path.Combine(this.destinationDir, torrent);
                    using (var fileStream = new FileStream(uploadfile, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                        client.UploadFile(fileStream, Path.GetFileName(uploadfile));
                    }
                }
            }
        }

        private void ProcessDirs()
        {
            this.torrentsToUpload = new List<string>();

            foreach (var dir in this.directories)
            {

                var dirName = Helper.ToAlphaNumberc(Helper.GetFolderName(dir));
                var torrentFileName = dirName + ".torrent";
                var torrentPath = Path.Combine(this.destinationDir, torrentFileName);

                this.io.Write("Creating torrent: " + torrentFileName, Colors.Default, false);

                ITorrentFileSource fileSource = new TorrentFileSource(dir);

                this.torrentCreator.Create(fileSource, torrentPath);

                this.StartTorrent(torrentFileName);

                this.torrentsToUpload.Add(torrentFileName);

                this.io.Write(" Done!", Colors.Green);
            }
        }

        private void StartTorrent(string torrentFileName)
        {
            Process.Start(Path.Combine(this.destinationDir, torrentFileName));
        }

        private void LoadDirsToUpload()
        {
            this.directories = Directory.GetDirectories(this.sourceDir);
        }

        private void UploadToSeedbox()
        {
            foreach (var torrent in this.torrentsToUpload)
            {
                this.UploadTorrent(torrent);
            }
        }

        private void UploadTorrent(string torrent)
        {
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(
                    this.settings["sbUsername"],
                    this.settings["sbPassword"]);
                client.UploadFile(
                    this.settings["sbAddress"] + torrent,
                    "STOR",
                    Path.Combine(this.destinationDir, torrent));
            }
        }

        private void InitializeTorrentCreator()
        {
            this.AddAnnounces();

            this.torrentCreator.Comment = this.settings["comment"];
            this.torrentCreator.CreatedBy = this.settings["createdBy"];
            this.torrentCreator.Publisher = this.settings["publisher"];
        }

        private void LoadSettings()
        {
            this.settings = ConfigurationManager.AppSettings;
            this.sourceDir = this.settings["sourceDir"];
            this.destinationDir = this.settings["torrentFileDestinationDir"];
            this.announces = this.settings["announces"].Split(';');

            if (!Directory.Exists(this.destinationDir))
            {
                Directory.CreateDirectory(this.destinationDir);
            }
        }

        private void AddAnnounces()
        {
            foreach (var announce in this.announces)
            {
                this.torrentCreator.Announces
                    .Add(new List<string>() { announce });
            }
        }
    }
}