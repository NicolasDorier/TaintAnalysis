using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TaintAnalysis
{
    public class GraphvizExecutor
    {
        public string BinariesPath { get; set; } = @"C:\Program Files (x86)\Graphviz2.38\bin";
        public string WorkingDirectory { get; set; }
        public GraphvizExecutor()
        {

        }

        public string Generate(string dotFile, string outputFile)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(GetToolFullPath("dot"));
            if (WorkingDirectory != null)
                processStartInfo.WorkingDirectory = WorkingDirectory;
            var extension = Path.GetExtension(outputFile).Replace(".", string.Empty);
            processStartInfo.ArgumentList.Add($"-T{extension.ToLowerInvariant()}");
            processStartInfo.ArgumentList.Add(dotFile);
            processStartInfo.ArgumentList.Add("-o");
            processStartInfo.ArgumentList.Add(outputFile);
            var process = Process.Start(processStartInfo);
            process.WaitForExit();
            return null;
        }

        private string GetToolFullPath(string toolName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                toolName += ".exe";
            if (BinariesPath != null)
                toolName = Path.Combine(BinariesPath, toolName);
            return toolName;
        }
    }
}
