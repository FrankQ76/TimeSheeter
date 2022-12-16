using System;

namespace JsonFixer
{
    internal static class Write2Log
    {


        public static void WriteToLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) { return; }

            if (GlobalParm._logging)
            {
                Console.WriteLine($"{message}");

            }


        }
    }
}