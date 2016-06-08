namespace TorrentCreator.IO
{
    using System;

    public class Io
    {
        public string Read()
        {
            return Console.ReadLine();
        }

        public void Write(object text, Colors color = Colors.Default, bool newLine = true)
        {
            switch (color)
            {
                case Colors.Red:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case Colors.Green:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case Colors.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Colors.Default:
                    break;
                default:
                    break;
            }

            if (newLine)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.Write(text);
            }

            Console.ResetColor();
        }
    }
}