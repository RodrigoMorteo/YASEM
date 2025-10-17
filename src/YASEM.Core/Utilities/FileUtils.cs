using System;
using System.IO;

namespace YASEM.Core.Utilities
{
    static class FileUtils 
    {
        public static string CheckOrSetFullyQuallifiedFilePath(string filePath) 
        {
            if(filePath == null || filePath.Length == 0)
                throw new Exception ("Invalid file path: " + filePath);
            if(Path.IsPathFullyQualified(filePath) && Path.GetFileName(filePath) != "" && Path.HasExtension(filePath))
            {
                return filePath;
            }
            else 
                {
                    string? directory = Path.GetDirectoryName(filePath); //directory may be null
                    //if no directory is present
                    if(directory == null || directory.Length == 0)
                    {
                        directory = Directory.GetCurrentDirectory(); //set directory to current as default
                    }
                    //if it is a relative path
                    else if(!Path.IsPathFullyQualified(directory))
                    {
                        directory = Path.Combine(Directory.GetCurrentDirectory(), directory); //combine current and relative path 
                    }
                    
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string extension = Path.GetExtension(filePath);

                    //if filename is not properly formated
                    if(fileName.Length == 00 || extension.Length == 0) 
                    {
                        throw new Exception ("File path is mising one or more components (directory, filename or extension)");
                    }
                    filePath = Path.Combine(directory, fileName + extension);
                }
            return filePath;
        }

        public static String CheckOrCreatePath(string? filePath){
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            filePath = CheckOrSetFullyQuallifiedFilePath(filePath); //get fullyquallified path
            filePath = Path.GetDirectoryName(filePath); //get only the fully quallified directory path (remove filename if any)
            if(filePath == null || filePath.Length == 0)
            {
                throw new Exception("Operation resulted in a null or empty path.");
            }
            if (!Directory.Exists(filePath)) //check if directory does not exists and create it
            {
                Directory.CreateDirectory(filePath);
            }
            return filePath;
        }
    }
}