using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TaintAnalysis
{
    public enum GraphType
    {
        Graph,
        Digraph
    }
    public enum Shape
    {
        None,
        Crow,
    }
    public class DotWriter : IDisposable
    {
        private StreamWriter outputStream;
        private bool ownStream;

        public DotWriter(Stream outputStream, bool leaveOpen)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            this.outputStream = new StreamWriter(outputStream, new UTF8Encoding(false), 1024, leaveOpen);
            this.ownStream = leaveOpen;
        }

        public void WriteAttributeName(string attrName)
        {
            Write($"{attrName} = ");
        }

        public void WriteAttribute(string attrName, string rawValue)
        {
            Write($"{attrName} = {rawValue}");
            NewLine();
        }

        public IDisposable WriteHTMLValue()
        {
            outputStream.Write("<");
            return ActionDisposable.Create(() =>
            {
                outputStream.Write(">");
                NewLine();
            });
        }

        public void WriteShape(Shape shape)
        {
            string value = null;
            switch (shape)
            {
                case Shape.None:
                    value = "none";
                    break;
                case Shape.Crow:
                    value = "crown";
                    break;
                default:
                    throw new NotSupportedException();
            }
            WriteAttribute("shape", value);
        }

        public IDisposable WriteNode(string nodeName)
        {
            Write(nodeName);
            NewLine();
            return WriteAttributeList();
        }

        public IDisposable WriteAttributeList()
        {
            Write("[");
            depth++;
            NewLine();
            return ActionDisposable.Create(() =>
            {
                depth--;
                Write("]");
                NewLine();
            });
        }

        public IDisposable WriteGraph(GraphType graphType, string id = null)
        {
            Write(graphType == GraphType.Digraph ? "digraph" : "graph");
            if (!string.IsNullOrEmpty(id))
            {
                Write($" {id}");
            }
            NewLine();
            return WriteStatementList();
        }

        bool needWriteTab = false;
        public void NewLine()
        {
            outputStream.Write(Environment.NewLine);
            needWriteTab = true;
        }

        public void Write(string str)
        {
            if (needWriteTab)
            {
                WriteTabs();
                needWriteTab = false;
            }
            outputStream.Write(str);
        }

        private void WriteTabs()
        {
            for (int i = 0; i < depth; i++)
            {
                outputStream.Write("    ");
            }
        }

        int depth;
        public IDisposable WriteStatementList()
        {
            Write("{");
            depth++;
            NewLine();
            return ActionDisposable.Create(() =>
            {
                depth--;
                Write("}");
                NewLine();
            });
        }

        public void Dispose()
        {
            Flush();
            outputStream.Dispose();
        }

        private void Flush()
        {
            outputStream.Flush();
        }
    }
}
