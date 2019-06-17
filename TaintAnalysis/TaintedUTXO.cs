using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TaintAnalysis
{
    public class TaintedUTXO
    {
        public int Id { get; set; }
        public List<(Color Taint, decimal Value)> Taints
        {
            get;
        } = new List<(Color Taint, decimal Value)>();

        public IEnumerable<(Color Taint, decimal Value, decimal Percent)> GetWeightedTaints(decimal minimumHeight, decimal maximumHeight)
        {
            HashSet<Color> excluded = new HashSet<Color>();
            decimal excludedTotal = 0.0m;
            bool newExcluded = false;
        retry:
            var total = Taints.Where(t => !excluded.Contains(t.Taint)).Select(t => t.Value).Sum();
            foreach (var t in Taints.Where(t => !excluded.Contains(t.Taint)))
            {
                var newValue = (t.Value / total) * maximumHeight;
                if (newValue < minimumHeight)
                {
                    excluded.Add(t.Taint);
                    excludedTotal += newValue;
                    newExcluded = true;
                }
            }
            if (newExcluded)
            {
                newExcluded = false;
                goto retry;
            }

            total = Taints.Select(t => t.Value).Sum();
            foreach (var t in Taints)
            {
                if (!excluded.Contains(t.Taint))
                {
                    var newValue = (((decimal)t.Value) / total) * maximumHeight;
                    var percent = (((decimal)t.Value) / total);
                    yield return (t.Taint, newValue, percent);
                }
            }

            if ((int)excludedTotal != 0)
            {
                var newValue = (excludedTotal / total) * maximumHeight;
                var percent = (excludedTotal / total);
                yield return (Color.Black, (int)excludedTotal, percent);
            }
        }

        decimal MinHeight = 1.0m;
        decimal MaxHeight = 200m;
        bool Spent;
        public int[] ParentIds = Array.Empty<int>();
        Color? _RealOwner;
        public Color RealOwner
        {
            get
            {
                return _RealOwner ?? Taints.Single().Taint;
            }
            set
            {
                _RealOwner = value;
            }
        }
        public Color? ProbableOwner { get; set; }
        public void CoinJoin(TaintedUTXO mainParent, TaintedUTXO[] parents, bool isMerge = false)
        {
            Taints.AddRange(parents.SelectMany(o => o.Taints)
                .GroupBy(o => o.Taint)
                .Select(g => (g.Key, g.Select(_ => _.Value).Sum())));
            Normalize();
            //ParentIds = new[] { mainParent.Id };
            RealOwner = mainParent.RealOwner;
            ParentIds = parents.Select(p => p.Id).ToArray();
            foreach (var p in parents)
                p.Spent = true;
            if (isMerge)
            IsMerge = true;
        }
        public bool IsMerge;
        private void Normalize()
        {
            var total = Taints.Select(t => t.Value).Sum();
            var old = Taints.ToList();
            Taints.Clear();
            foreach (var t in old)
            {
                Taints.Add((t.Taint, (t.Value / total) * 100.0m));
            }
        }

        public void Write(DotWriter dotWriter)
        {
            using (dotWriter.WriteNode($"utxo{Id}"))
            {
                dotWriter.WriteShape(Shape.None);
                dotWriter.WriteAttributeName("label");
                using (dotWriter.WriteHTMLValue())
                {
                    int transparency = IsMerge ? 255 : 128;
                    dotWriter.Write($"<table border=\"5\" cellspacing=\"0\" color=\"{ToString(transparency, Color.Black)}\" cellpadding=\"0\" cellborder=\"0\">");
                    dotWriter.NewLine();

                    foreach (var taint in GetWeightedTaints(MinHeight, MaxHeight))
                    {
                        var heuristicResult = !ProbableOwner.HasValue ? ""
                                            : ProbableOwner.Value == RealOwner ? $"<td width=\"15\" bgcolor=\"Green\" tooltip=\"Heuristic succeed\" href=\"#\"></td>"
                                            : $"<td width=\"15\" bgcolor=\"Red\" tooltip=\"Heuristic failed\" href=\"#\"></td>";
                        var owner = $"<td width=\"15\" bgcolor=\"{ToString(transparency, RealOwner)}\" tooltip=\"Real owner\" href=\"#\"></td>";
                        dotWriter.Write($"<tr>{owner}<td height=\"{taint.Value}\" width=\"100\" bgcolor=\"{ToString(transparency, taint.Taint)}\" tooltip=\"{taint.Percent:P2}\" href=\"#\"></td>{heuristicResult}</tr>");
                        dotWriter.NewLine();
                    }
                    dotWriter.Write("</table>");
                }
            }
            foreach (var parent in ParentIds)
            {
                var head = IsMerge ? "normal" : "inv";
                var style = IsMerge ? "dashed" : "solid";
                dotWriter.Write($"utxo{parent} -> utxo{Id} [arrowhead={head},style={style}]");
                dotWriter.NewLine();
            }
        }

        private string ToString(int alpha, Color c)
        {
            StringBuilder builder = new StringBuilder(1 + 3 * 2);
            builder.Append('#');
            builder.AppendFormat("{0:X2}", c.R);
            builder.AppendFormat("{0:X2}", c.G);
            builder.AppendFormat("{0:X2}", c.B);
            builder.AppendFormat("{0:X2}", alpha);
            return builder.ToString();
        }
    }
}
