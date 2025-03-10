using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileWatching
{
    public class FileConsumerService : IFileConsumerService
    {
        #region config

        static int initialTimerInterval = 5;
        static int delayedTimerIntervalAddition = 1;
        static int permittedIntervalBetweenFiles = 60;

        static bool LogFSWEvents = false;
        static bool LogFileReadyEvents = false;
        static bool FSWUseRegex = false;

        static string FSWRegex = null;
        static string FTPUrl = null;
        static string FTPUser = null;
        static string FTPPassword = null;
        static string FTPPathUpload = null;
        List<string> _filteredFileTypes;
        #endregion
        ILogger<FileConsumerService> _logger;

        public FileConsumerService(ILogger<FileConsumerService> logger)
        {
            _logger = logger;
            ReadAllSettings();
        }
        void ReadAllSettings()
        {
            try
            {
                initialTimerInterval = int.Parse(ConfigValueProvider.Get("FSW:initialTimerInterval"));
                delayedTimerIntervalAddition = int.Parse(ConfigValueProvider.Get("FSW:delayedTimerAddition"));
                permittedIntervalBetweenFiles = int.Parse(ConfigValueProvider.Get("FSW:permittedSecondsBetweenReadyEvents"));
                LogFileReadyEvents = bool.Parse(ConfigValueProvider.Get("FSW:LogFileReadyEvents"));
                LogFSWEvents = bool.Parse(ConfigValueProvider.Get("FSW:LogFSWEvents"));
                FSWUseRegex = bool.Parse(ConfigValueProvider.Get("FSW:FSWUseRegex"));
                FSWRegex = ConfigValueProvider.Get("FSW:FSWRegex");
                FTPUrl = ConfigValueProvider.Get("FSW:FTPUrl");
                FTPUser = ConfigValueProvider.Get("FSW:FTPUser");
                FTPPassword = ConfigValueProvider.Get("FSW:FTPPassword");
                FTPPathUpload = ConfigValueProvider.Get("FSW:FTPPathUpload");
                _filteredFileTypes = ConfigValueProvider.GetArray("FSW:FileTypes");
                Console.WriteLine($"initialTimerInterval:[{initialTimerInterval}], delayedTimerIntervalAddition:[{delayedTimerIntervalAddition}], permittedIntervalBetweenEvents:[{permittedIntervalBetweenFiles}]");
                Console.WriteLine($"LogFileReadyEvents:[{LogFileReadyEvents}], LogFSWEvents:[{LogFSWEvents}]");

            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error reading R2NETFSWSettings settings(setting defaults...), Message: {e.Message}", true);
            }
        }

        public async Task ConsumeFile(string pathToFile)
        {
            if(!File.Exists(pathToFile))
                return;

            _logger.LogInformation($"Starting read of {pathToFile}");
            try
            {
                FileInfo f = new FileInfo(pathToFile);
                if (FSWUseRegex && !Regex.IsMatch(f.Name, FSWRegex))
                    return;
                if (_filteredFileTypes.Any(str => f.Extension.ToLower().Equals(str)))
                {
                    DateTime eventTime = DateTime.Now;
                    string fileName = f.Name;
                    if (LogFSWEvents)
                        Console.WriteLine($"Time: {eventTime.TimeOfDay}\t  FileName: {fileName,-50} Path: {f.FullName} ");
                    using (var client = new SftpClient(FTPUrl, FTPUser, FTPPassword))
                    {
                        client.Connect();
                        using (FileStream fs = new FileStream(f.FullName, FileMode.Open, FileAccess.Read))
                        {
                            client.UploadFile(fs, FTPPathUpload + f.Name);

                        }
                        client.Disconnect();
                        client.Dispose();
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //using (StreamReader sr = File.OpenText(pathToFile))
            //{
            //    string? s = null;
            //    int counter = 1;
            //    while ((s = await sr.ReadLineAsync()) != null)
            //    {
            //        _logger.LogInformation($"Reading Line {counter} of the file {pathToFile}");
            //        counter++;
            //    }
            //}

            _logger.LogInformation($"Completed read of {pathToFile}");
        }
    }
}
