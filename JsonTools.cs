using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace JsonFixer
{
    public static class JsonTools
    {


        public static void GetJSonPathValue(ContextMenuStrip contextMenu)
        {


            if (GlobalParm._logging)
            {
                Console.WriteLine($"\nFunction Get Json Path values requested.");
            }

            var pathValuesTmp = ExtractAllJsonPath();


            if (!string.IsNullOrEmpty(pathValuesTmp))
            {
                var pathValues = pathValuesTmp.Remove(pathValuesTmp.Trim().Length - 1, 1);
                ActionClipboard.SetClip(pathValues);

                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nJson Path values has been copied to Clipboard.");
                }
            }
            else
            {
                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nClipboard is null or empty.");
                }
            }


        }

        private static string ExtractAllJsonPath()
        {
            var json = ActionClipboard.GetClipBoard();


            if (!string.IsNullOrWhiteSpace(json) && IsJsonValid(json))
            {
                var jobject = JObject.Parse(json);
                var sb = new StringBuilder();

                RecursiveParse(sb, jobject);

                if (GlobalParm._logging)
                {
                    Console.WriteLine("\nAll the Json Path values ready to be use in SQL: \n");
                    Console.WriteLine(sb.ToString());
                }

                return sb.ToString();
            }

            if (GlobalParm._logging)
            {
                Console.WriteLine("\nCannot extract path values.");
            }

            return string.Empty;

        }



        private static bool IsJsonValid(string json)
        {

            try
            {
                //var clipText = GetClipBoard();
                var jsonFormatted = JValue.Parse(json).ToString((Newtonsoft.Json.Formatting)Formatting.Indented);
                return true;
            }
            catch
            {
                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nNot a valid json document in the Clipboard.");

                }
                return false;
            }
        }


        private static void RecursiveParse(StringBuilder sb, JToken token)
        {
            foreach (var item in token.Children())
            {
                if (item.HasValues)
                {
                    RecursiveParse(sb, item);
                }
                else
                {
                    sb.AppendLine($"JSON_VALUE(Data, '$.{item.Path}') as {item.Path.Split('.').Last()},");
                }
            }

        }
    }
}