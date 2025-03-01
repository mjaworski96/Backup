﻿using Common;
using Common.Translations;
using FilesystemModel.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FilesystemModel
{
    public static class SafeFileUsage
    {
        const int WAIT_TIME_IN_SECCONDS = 5;
        static readonly List<long> FILE_IN_USE_ERROR_CODES;
        static SafeFileUsage()
        {
            FILE_IN_USE_ERROR_CODES = new List<long> { 0x20, 0x21 }; //32, 33
            try
            {
                var os = Environment.OSVersion;
                if (os.Platform == PlatformID.Unix)
                {
                    FILE_IN_USE_ERROR_CODES = new List<long> { 0x1A }; //26
                }
            }
            catch(Exception) {}
        }

        public static FileStream GetFile(string filename, 
            FileMode fileMode, FileAccess fileAccess,
            ILogger logger,
            bool setFileNormalAttribute)
        {   
            FileStream file = null;
            do
            {
                try
                {
                    if (setFileNormalAttribute && System.IO.File.Exists(filename))
                    {
                        FileAttributes.Normal.Set(filename, logger);
                    }
                    
                    file = new FileStream(filename, fileMode, fileAccess);
                }
                catch (IOException e)
                {
                    long errorCode = e.HResult & 0xFFFF;
                    if(FILE_IN_USE_ERROR_CODES.Contains(errorCode))
                    {
                        logger.Write(string.Format(LoggerMessages.FileIsUsed, filename, WAIT_TIME_IN_SECCONDS));
                        Thread.Sleep(WAIT_TIME_IN_SECCONDS * 1000);
                    }
                    else
                    {
                        throw e;
                    }
                }
            } while (file == null);
            return file;
        }
    }
}
