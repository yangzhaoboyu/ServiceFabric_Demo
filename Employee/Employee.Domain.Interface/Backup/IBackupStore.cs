using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace Employee.Domain.Interface.Backup
{
    public interface IBackupStore
    {
        long backupFrequencyInSeconds { get; }

        Task ArchiveBackupAsync(BackupInfo backupInfo, CancellationToken cancellationToken);

        Task DeleteBackupsAsync(CancellationToken cancellationToken);

        Task<string> RestoreLatestBackupToTempLocation(CancellationToken cancellationToken);
    }
}