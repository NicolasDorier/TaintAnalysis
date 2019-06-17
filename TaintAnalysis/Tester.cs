using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace TaintAnalysis
{
    public class Tester : IDisposable
    {
        private string Directory { get; }
        public string DotFilePath => string.Format(DotFilePathTemplate, ImageNumber);
        public string DotFilePathTemplate { get; set; }
        public string ImageFilePath => string.Format(ImageFilePathTemplate, ImageNumber);
        public string ImageFilePathTemplate { get; set; }
        int ImageNumber = 0;
        public Tester(string directory)
        {
            this.Directory = directory;
            DotFilePathTemplate = Path.Combine(directory, "graph-{0}.dot");
            ImageFilePathTemplate = Path.Combine(directory, "graph-{0}.png");
        }

        public static Tester Create([CallerMemberNameAttribute]string name = null)
        {
            string directory = name;
            if (System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.Delete(directory, true);
                while (System.IO.Directory.Exists(directory))
                {
                    Thread.Sleep(10);
                }
            }
            System.IO.Directory.CreateDirectory(directory);
            return new Tester(directory);
        }


        private TaintedUTXOBuilder _Builder = new TaintedUTXOBuilder().SetSeed(0);
        public TaintedUTXOBuilder Builder => _Builder;

        public void ResetBuilder(int seed)
        {
            _Builder = new TaintedUTXOBuilder().SetSeed(seed);
        }

        public GraphvizExecutor GraphvizExecutable { get; set; } = new GraphvizExecutor();
        public void CreateImage(bool open = true)
        {
            using (var dotWriter = new DotWriter(File.OpenWrite(DotFilePath), false))
            {
                using (dotWriter.WriteGraph(GraphType.Digraph))
                {
                    Builder.Write(dotWriter);
                }
            }
            GraphvizExecutable.Generate(DotFilePath, ImageFilePath);
            if (open && File.Exists(ImageFilePath)) // TODO: do something for mac and linux
                Process.Start("explorer.exe", ImageFilePath);
            ImageNumber++;
        }

        public void Dispose()
        {
        }
    }
}
