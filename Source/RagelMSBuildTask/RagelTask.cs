//   Copyright 2018 Michael Tindal
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildTasks
{
    public class RagelTask : ToolTask
    {
        protected override string ToolName
        {
            get
            {
                var osArchitecture = RuntimeInformation.ProcessArchitecture;
                var windir = Environment.GetEnvironmentVariable("windir");
                if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
                {
                    return "ragel.exe";
                }

                if (File.Exists(@"/proc/sys/kernel/ostype"))
                {
                    var osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                    if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                    {
                        return osArchitecture switch
                        {
                            Architecture.X64 => "ragel-Linux-x86_64",
                            Architecture.Arm64 => "ragel-Linux-arm64",
                            _ => null
                        };
                    }

                    return null;
                }

                if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
                {
                    // Note: iOS gets here too
                    return "ragel-Darwin-x86_64";
                }
                return null;
            }
            
        }

        protected new string ToolPath
        {
            get
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(assemblyFolder, "..", "..", "content", "tools");
            }
        }

        public string WorkingDirectory { get; set; }

        [Required]
        public ITaskItem[] Sources { get; set; }

        public string OutputPath { get; set; }

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

                var outputPath = OutputPath ?? Path.GetDirectoryName(taskItem.ToString());
                if (!outputPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    outputPath = $"{outputPath}{Path.DirectorySeparatorChar}";
                }
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }
                var generatedFileName = $"{outputPath}{Path.GetFileNameWithoutExtension(taskItem.ToString())}.cs";
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
