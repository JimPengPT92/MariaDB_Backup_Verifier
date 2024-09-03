using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace MariaDB_Backup_and_Verifications
{
    public partial class Form1 : Form
    {
        private static readonly string glbMariaDBExePath = ConfigurationManager.AppSettings["MariaDBExePath"];
        private static readonly string glbMariaDBDataDir = ConfigurationManager.AppSettings["MariaDBDataDir"];
        private static readonly string glbMariDBUser = ConfigurationManager.AppSettings["MariDBUser"];
        private static readonly string glbMariDBPassword = ConfigurationManager.AppSettings["MariDBPassword"];

        private static readonly string glbRemoteBackupNetPath = ConfigurationManager.AppSettings["RemoteBackupNetPath"];
        private static readonly string glbRemoteBackupPath = ConfigurationManager.AppSettings["RemoteBackupPath"];
        private static readonly string glbRemoteServer = ConfigurationManager.AppSettings["RemoteServer"];
        private static readonly string glbRemoteUser = ConfigurationManager.AppSettings["RemoteUser"];
        private static readonly string glbRemotePassword = ConfigurationManager.AppSettings["RemotePassword"];

        private static readonly string glbLocalBackupPath = ConfigurationManager.AppSettings["LocalBackupPath"];
        private static readonly string glbLocalServer = ConfigurationManager.AppSettings["LocalServer"];
        private static readonly string glbLocalUser = ConfigurationManager.AppSettings["LocalUser"];
        private static readonly string glbLocalPassword = ConfigurationManager.AppSettings["LocalPassword"];

        public static string strmessage = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ConfigureWinRM();
        }

        public static void ConfigureWinRM()
        {
            // Configure WinRM on the Local Machine (glbLocalServer)
            string localWinRMCommand = $@"
                Set-Item WSMan:\localhost\Client\TrustedHosts -Value '{glbRemoteServer}' -Concatenate;
                Restart-Service WinRM";
            ExecuteCommand(localWinRMCommand, ref strmessage, "", "Local");

            // Configure WinRM on the Remote Machine (glbRemoteServer)
            string remoteWinRMCommand = $@"
                $securePassword = ConvertTo-SecureString '{glbRemotePassword}' -AsPlainText -Force;
                $cred = New-Object System.Management.Automation.PSCredential('{glbRemoteUser}', $securePassword);
                Invoke-Command -ComputerName {glbRemoteServer} -Credential $cred -ScriptBlock {{
                    Enable-PSRemoting -Force;
                    Set-Item WSMan:\localhost\Client\TrustedHosts -Value '{glbLocalServer}' -Concatenate;
                    Set-Service -Name WinRM -StartupType Automatic;
                    Start-Service -Name WinRM;
                }}";
            ExecuteCommand(remoteWinRMCommand, ref strmessage, "", "Remote");
        }

        public static void ExecuteCommand(string command, ref string strmessage, string backupPath, string type)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = new Process())
            {
                try
                {
                    proc.StartInfo = procStartInfo;
                    proc.Start();

                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        if (error.Contains("completed"))
                        {
                            if (type == "Local")
                            {
                                strmessage = "Completed successfully. Files saved at: " + glbLocalBackupPath + "\\" + backupPath.Substring(glbLocalBackupPath.Length + 1, backupPath.Length - (glbLocalBackupPath.Length + 1));
                            }
                            else if (type == "Remote")
                            {
                                strmessage = "Completed successfully. Files saved at: " + glbRemoteBackupPath + "\\" + backupPath.Substring(glbRemoteBackupNetPath.Length + 1, backupPath.Length - (glbRemoteBackupNetPath.Length + 1));
                            }
                            else
                            {
                                strmessage = "Completed successfully. \nFiles saved at: \nC:\\Backup\\Local or C:\\Backup\\Remote folders.";
                            }
                        }
                        else 
                        {
                            strmessage = "Error during operation: " + error;
                        }

                    }
                    else
                    {
                        strmessage = "Backup completed successfully. Files saved at: " + backupPath;
                    }
                }
                catch (Exception ex)
                {
                    strmessage = "Error during backup: " + ex.Message;
                }
            }
        }

        public static void StartBackup(ref string backupPath)
        {
            backupPath = Path.Combine(backupPath, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }
        }

        static void LocalBackup(string user, string password, ref string backupPath, ref string strmessage)
        {
            // Ensure MariaDB service is running
            if (!StartMariaDBService(ref strmessage))
            {
                strmessage = "Error starting MariaDB service: " + strmessage;
                return;
            }

            string dumpExe = glbMariaDBExePath;
            string dataDir = glbMariaDBDataDir;

            string backupCommand = $@"& '{dumpExe}' --backup --target-dir='{backupPath}' --user={user} --password={password} --datadir='{dataDir}'";
            ExecuteCommand(backupCommand, ref strmessage, backupPath, "Local");
        }

        static void RemoteBackup(string server, string user, string password, ref string backupPath, ref string strmessage)
        {
            // Ensure MariaDB service is running
            if (!StartMariaDBService(ref strmessage))
            {
                strmessage = "Error starting MariaDB service: " + strmessage;
                return;
            }

            string remoteDumpExe = glbMariaDBExePath;
            string remoteDataDir = glbMariaDBDataDir;

            string psCommand = $@"
    $securePassword = ConvertTo-SecureString '{glbRemotePassword}' -AsPlainText -Force;
    $cred = New-Object System.Management.Automation.PSCredential('{glbRemoteUser}', $securePassword);
    Invoke-Command -ComputerName {server} -Credential $cred -ScriptBlock {{
        $backupDir = '{backupPath}';
        
        # Credentials for accessing the shared folder on RemoteBackupNetPath
        $netResource = '{glbRemoteBackupNetPath}';
        $userName = '{glbLocalUser}';
        $password = ConvertTo-SecureString '{glbLocalPassword}' -AsPlainText -Force;
        $shareCred = New-Object System.Management.Automation.PSCredential($userName, $password);
        
        New-PSDrive -Name X -PSProvider FileSystem -Root $netResource -Credential $shareCred -Persist;

        if (-not (Test-Path -Path $backupDir)) {{
            New-Item -Path $backupDir -ItemType Directory -Force | Out-Null;
        }}
        & '{remoteDumpExe}' --backup --target-dir=$backupDir --user='{user}' --password='{password}' --datadir='{remoteDataDir}';

        # Remove the mapped drive after the operation
        Remove-PSDrive -Name X -Force;
    }}";

            ExecuteCommand(psCommand, ref strmessage, backupPath, "Remote");
        }

        static void VerifyBackups(ref string backupPath, ref string strmessage)
        {
            // Ensure MariaDB service is running
            if (!StartMariaDBService(ref strmessage))
            {
                strmessage = "Error: MariaDB service is not running. Cannot proceed with backup verification.";
                return;
            }

            // Find the most recent backup folder
            string[] backupDirs = Directory.GetDirectories(backupPath);
            if (backupDirs.Length == 0)
            {
                strmessage = $"Error: No backup directories found in {backupPath}. Verification cannot proceed.";
                return;
            }

            string latestBackupDir = backupDirs.OrderByDescending(dir => Directory.GetCreationTime(dir)).First();
            string cnfFile = Path.Combine(latestBackupDir, "backup-my.cnf");

            // Check if backup-my.cnf exists
            if (!File.Exists(cnfFile))
            {
                strmessage = $"Error: 'backup-my.cnf' not found in {latestBackupDir}. Verification cannot proceed.";
                return;
            }

            string dumpExe = glbMariaDBExePath;
            string verifyCommand = $@"& '{dumpExe}' --prepare --target-dir='{latestBackupDir}'";

            ExecuteCommand(verifyCommand, ref strmessage, latestBackupDir, "Verify");

            if (string.IsNullOrEmpty(strmessage) || strmessage.Contains("Completed successfully"))
            {
                strmessage += "\nBackup verification completed successfully.";
            }
            else
            {
                strmessage = "Error during backup verification: " + strmessage;
            }
        }

        static void RestoreBackups(ref string backupPath, ref string strmessage)
        {
            // Ensure MariaDB service is stopped before restoring
            if (!StopMariaDBService(ref strmessage))
            {
                strmessage = "Error stopping MariaDB service: " + strmessage;
                return;
            }

            // Clear the data directory before restoring
            ClearDataDirectory(ref strmessage);

            if (!strmessage.Contains("Failed"))
            {
                string dumpExe = glbMariaDBExePath;
                string dataDir = glbMariaDBDataDir;
                string restoreCommand = $@"& '{dumpExe}' --copy-back --target-dir='{backupPath}' --datadir='{dataDir}'";

                ExecuteCommand(restoreCommand, ref strmessage, backupPath, "Restore");

                // Start MariaDB service after restoring
                if (!StartMariaDBService(ref strmessage))
                {
                    strmessage = "Error starting MariaDB service: " + strmessage;
                }
                else
                {
                    strmessage += "\nBackup restored successfully.";
                }
            }
            else
            {
                strmessage = "Error clearing data directory: " + strmessage;
            }
        }

        static bool StartMariaDBService(ref string strmessage)
        {
            try
            {
                using (ServiceController service = new ServiceController("MariaDB"))
                {
                    if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }

                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        return true;
                    }
                    else
                    {
                        strmessage = "Failed to start MariaDB service.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strmessage = "Error starting MariaDB service: " + ex.Message;
                return false;
            }
        }

        static bool StopMariaDBService(ref string strmessage)
        {
            try
            {
                using (ServiceController service = new ServiceController("MariaDB"))
                {
                    if (service.Status != ServiceControllerStatus.Stopped && service.Status != ServiceControllerStatus.StopPending)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }

                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        return true;
                    }
                    else
                    {
                        strmessage = "Failed to stop MariaDB service.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strmessage = "Error stopping MariaDB service: " + ex.Message;
                return false;
            }
        }

        static void ClearDataDirectory(ref string strmessage)
        {
            string dataDir = glbMariaDBDataDir;
            bool success = false;
            int retryCount = 3;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    // Ensure MariaDB service is stopped
                    if (StopMariaDBService(ref strmessage))
                    {
                        ExecuteCommand($"Remove-Item -Recurse -Force -LiteralPath '{dataDir}'", ref strmessage, dataDir, "ClearDataDir");
                        if (!strmessage.Contains("Error"))
                        {
                            success = true;
                            break;
                        }
                    }
                }
                catch
                {
                    // Wait for a moment before retrying
                    System.Threading.Thread.Sleep(5000);
                }
            }

            if (!success)
            {
                strmessage = "Failed to clear data directory after multiple attempts.";
            }
        }

        private void btnLocalBackup_Click(object sender, EventArgs e)
        {
            string user = glbMariDBUser;
            string password = glbMariDBPassword;
            string backupPath = glbLocalBackupPath;
            StartBackup(ref backupPath);
            LocalBackup(user, password, ref backupPath, ref strmessage);
            MessageBox.Show(strmessage);
        }

        private void btnRemoteBackup_Click(object sender, EventArgs e)
        {
            string server = glbRemoteServer;
            string user =  glbMariDBUser;
            string password = glbMariDBPassword;
            string backupPath = glbRemoteBackupNetPath;
            StartBackup(ref backupPath);
            RemoteBackup(server, user, password, ref backupPath, ref strmessage);
            MessageBox.Show(strmessage);
        }

        private void btnVerifyBackups_Click(object sender, EventArgs e)
        {
            string backupPath = glbLocalBackupPath;
            VerifyBackups(ref backupPath, ref strmessage);

            if (string.IsNullOrEmpty(strmessage) || strmessage.Contains("completed successfully"))
            {
                strmessage += "\nLocal backup verified successfully.";
                backupPath = glbRemoteBackupPath;
                VerifyBackups(ref backupPath, ref strmessage);

                if (string.IsNullOrEmpty(strmessage) || strmessage.Contains("completed successfully"))
                {
                    strmessage += "\nRemote and Local backup verified successfully.";
                }
                else
                {
                    strmessage = "Error verifying remote backup: " + strmessage;
                }
            }
            else
            {
                strmessage = "Error verifying local backup: " + strmessage;
            }

            MessageBox.Show(strmessage);
        }

        private void btnRestoreBackups_Click(object sender, EventArgs e)
        {
            string backupPath = glbLocalBackupPath;
            RestoreBackups(ref backupPath, ref strmessage);

            if (string.IsNullOrEmpty(strmessage) || strmessage.Contains("completed successfully"))
            {
                strmessage += "\nLocal backup restored successfully.";
                backupPath = glbRemoteBackupPath;
                RestoreBackups(ref backupPath, ref strmessage);

                if (string.IsNullOrEmpty(strmessage) || strmessage.Contains("completed successfully"))
                {
                    strmessage += "\nRemote backup restored successfully.";
                }
                else
                {
                    strmessage = "Error restoring remote backup: " + strmessage;
                }
            }
            else
            {
                strmessage = "Error restoring local backup: " + strmessage;
            }

            MessageBox.Show(strmessage);
        }
    }
}