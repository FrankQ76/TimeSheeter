using System;
using System.Linq;
using System.Windows.Forms;


namespace JsonFixer
{
    public static class Duplicates
    {

        public static void RemoveDuplicates(ContextMenuStrip contextMenu)
        {


            if (GlobalParm._logging)
            {
                Console.WriteLine($"\nFunction Remove Duplicates requested.");
            }

            string strValues = ActionClipboard.GetClipBoard();


            if (!string.IsNullOrEmpty(strValues))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    var listValues = strValues.Split(stringSeparators, StringSplitOptions.None).ToList().Distinct().ToList();
                    var result = String.Join("\n", listValues.ToArray());

                    ActionClipboard.SetClip(result);

                }
                catch
                {
                    if (GlobalParm._logging)
                    {
                        Console.WriteLine($"\nError trying to remove duplicates.");
                    }
                }



                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nNew values without duplicates has been copied to Clipboard.");
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

        public static void GetDuplicates(ContextMenuStrip contextMenu)
        {


            if (GlobalParm._logging)
            {
                Console.WriteLine($"\nFunction Get Duplicates requested.");
            }

            string strValues = ActionClipboard.GetClipBoard();


            if (!string.IsNullOrEmpty(strValues))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };

                    var listValues = strValues.Split(stringSeparators, StringSplitOptions.None).ToList();
                    var query = listValues.GroupBy(x => x)
                                                    .Where(g => g.Count() > 1)
                                                    .Select(y => y.Key)
                                                    .ToList();



                    var result = String.Join("\n", query.ToArray());

                    ActionClipboard.SetClip(result);

                }
                catch
                {
                    if (GlobalParm._logging)
                    {
                        Console.WriteLine($"\nError trying to get duplicates.");
                    }
                }



                if (GlobalParm._logging)
                {
                    Console.WriteLine($"\nNew values with duplicates only has been copied to Clipboard.");
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




















    }
}