using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace IOManagerLibrary
{
    public class IOManager
    {
        public static String UppercaseWords(String value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ' || array[i - 1] == '(' || array[i - 1] == '[')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        public static bool MakeDirectory(String directoryPath)
        {
            bool retVal = false;
            try
            {
                Directory.CreateDirectory(directoryPath);
                retVal = true;
            }
            catch (IOException ex) { }
            catch (UnauthorizedAccessException ex) { }
            catch (ArgumentException ex) { }

            return retVal;
        }

        public static bool IsDirectoryExisted(String directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
    }
}
