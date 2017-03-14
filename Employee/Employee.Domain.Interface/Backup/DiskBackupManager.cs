using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace Employee.Domain.Interface.Backup
{
    public class DiskBackupManager : IBackupStore
    {
        private readonly long backupFrequencyInSeconds;
        private readonly long keyMax;
        private readonly long keyMin;
        private readonly int MaxBackupsToKeep;
        private readonly string PartitionArchiveFolder;
        private readonly string PartitionTempDirectory;

        public DiskBackupManager(ConfigurationSection configSection, string partitionId, long keymin, long keymax, string codePackageTempDirectory)
        {
            this.keyMin = keymin;
            this.keyMax = keymax;

            string BackupArchivalPath = configSection.Parameters["BackupArchivalPath"].Value;
            this.backupFrequencyInSeconds = long.Parse(configSection.Parameters["BackupFrequencyInSeconds"].Value);
            this.MaxBackupsToKeep = int.Parse(configSection.Parameters["MaxBackupsToKeep"].Value);

            this.PartitionArchiveFolder = Path.Combine(BackupArchivalPath, "Backups", partitionId);
            this.PartitionTempDirectory = Path.Combine(codePackageTempDirectory, partitionId);
        }

        #region IBackupStore Members

        long IBackupStore.backupFrequencyInSeconds
        {
            get { return this.backupFrequencyInSeconds; }
        }

        public Task ArchiveBackupAsync(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            string fullArchiveDirectory = Path.Combine(
                this.PartitionArchiveFolder,
                $"{Guid.NewGuid().ToString("N")}_{this.keyMin}_{this.keyMax}");

            DirectoryInfo dirInfo = new DirectoryInfo(fullArchiveDirectory);
            dirInfo.Create();

            string fullArchivePath = Path.Combine(fullArchiveDirectory, "Backup.zip");

            ZipFile.CreateFromDirectory(backupInfo.Directory, fullArchivePath, CompressionLevel.Fastest, false);

            DirectoryInfo backupDirectory = new DirectoryInfo(backupInfo.Directory);
            backupDirectory.Delete(true);

            return Task.FromResult(true);
        }

        public async Task DeleteBackupsAsync(CancellationToken cancellationToken) => await Task.Run(
            () =>
            {
                if (!Directory.Exists(this.PartitionArchiveFolder))
                {
                    return;
                }

                DirectoryInfo dirInfo = new DirectoryInfo(this.PartitionArchiveFolder);

                IEnumerable<DirectoryInfo> oldBackups = dirInfo.GetDirectories().OrderByDescending(x => x.LastWriteTime).Skip(this.MaxBackupsToKeep);

                foreach (DirectoryInfo oldBackup in oldBackups)
                {
                    oldBackup.Delete(true);
                }
            },
            cancellationToken);

        public Task<string> RestoreLatestBackupToTempLocation(CancellationToken cancellationToken)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(this.PartitionArchiveFolder);

            string backupZip = dirInfo.GetDirectories().OrderByDescending(x => x.LastWriteTime).First().FullName;

            string zipPath = Path.Combine(backupZip, "Backup.zip");

            DirectoryInfo directoryInfo = new DirectoryInfo(this.PartitionTempDirectory);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            directoryInfo.Create();

            ZipFile.ExtractToDirectory(zipPath, this.PartitionTempDirectory);

            return Task.FromResult(this.PartitionTempDirectory);
        }

        #endregion IBackupStore Members
    }
}