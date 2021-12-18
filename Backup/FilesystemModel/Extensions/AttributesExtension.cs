using System;
using System.IO;
using Common;
using Common.Translations;

namespace FilesystemModel.Extensions
{
    public static class AttributesExtension
    {
        public static void Set(this FileAttributes attributes, string path, ILogger logger)
        {
            try
            {
                System.IO.File.SetAttributes(path, attributes);
            }
            catch (Exception e)
            {
                logger.WriteError(string.Format(LoggerMessages.CantSetAtributes, path, e.Message));
            }
        }
    }
}
