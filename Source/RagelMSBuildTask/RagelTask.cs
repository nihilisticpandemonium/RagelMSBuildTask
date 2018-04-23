using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildTasks
{
    public class RagelTask : ToolTask
    {
        public static bool IsLinux
        {
            get
            {
                string windir = Environment.GetEnvironmentVariable("windir");
                if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
                {
                    return false;
                }
                else if (File.Exists(@"/proc/sys/kernel/ostype"))
                {
                    string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                    if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                    {
                        // Note: Android gets here too
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
                {
                    // Note: iOS gets here too
                    return true;
                }
                return false;
            }
        }

        protected override string ToolName => IsLinux ? "ragel" : "ragel.exe";

        public new string ToolPath => IsLinux ? "/usr/bin" : "C:\\Ragel\\";

        public string WorkingDirectory { get; set;  }

        [Required]
        public ITaskItem[] Sources { get; set; }

        [Output]
        public ITaskItem[] Outputs { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolName);
        }

        public override bool Execute()
        {
            var toolPath = GenerateFullPathToTool();
            if (!File.Exists(toolPath))
            {
                Log.LogError(
                    $"Unable to locate the tool binary.  Please provide the ToolPath property corresponding to the location of {ToolName}, given path {ToolPath}.");
                return false;
            }

            Directory.SetCurrentDirectory(WorkingDirectory ?? Directory.GetCurrentDirectory());

            var generatedFiles = new List<string>();

            foreach (var taskItem in Sources)
            {
                if (!File.Exists(taskItem.ToString()))
                {
                    Log.LogError($"Missing source file {taskItem}.");
                    return false;
                }

                var generatedFileName = taskItem.ToString().Replace(".rl", ".cs");
                var commandLine = $"{GenerateCommandLineCommands()} -o {generatedFileName} {taskItem}";
                var startInfo = new ProcessStartInfo
                {
                    FileName = toolPath,
                    Arguments = commandLine,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                Log.LogMessage(MessageImportance.High, $"Compiling {taskItem}...");
                var proc = Process.Start(startInfo);
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    generatedFiles.Add(generatedFileName);
                    continue;
                }
                Log.LogError($"Error while compiling source file {taskItem}.");
                return false;
            }

            Outputs = generatedFiles.Select(name => new TaskItem(name)).ToArray();
            return true;
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            builder.AppendSwitch("-A");

            return builder.ToString();
        }
    }
}
