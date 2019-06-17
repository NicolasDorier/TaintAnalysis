using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TaintAnalysis
{
    public class TaintedUTXOBuilder
    {
        Random _Rand;
        public TaintedUTXOBuilder()
        {
            _Rand = new Random();
        }

        public TaintedUTXOBuilder SetSeed(int seed)
        {
            _Rand = new Random(seed);
            return this;
        }

        public Model Model { get; set; } = new Model();

        Color[] _PaletteArray;
        IEnumerable<Color> _PaletteGenerator;
        IEnumerator<Color> _Palette;
        int n = 0;
        TaintedUTXO _TaintedUTXO;
        public TaintedUTXO CurrentTaintedUTXO => _TaintedUTXO;
        List<TaintedUTXO> UnspentCoins = new List<TaintedUTXO>();
        List<TaintedUTXO> Coins = new List<TaintedUTXO>();
        public TaintedUTXOBuilder NewTaintedUTXO()
        {
            var tainted = new TaintedUTXO();
            tainted.Id = n;
            n++;
            _TaintedUTXO = tainted;
            UnspentCoins.Add(tainted);
            Coins.Add(tainted);
            return this;
        }

        public TaintedUTXOBuilder SetPalette(Color[] colors)
        {
            _PaletteArray = colors;
            _PaletteGenerator = Enumerate(colors);
            _Palette = _PaletteGenerator.GetEnumerator();
            return this;
        }

        private IEnumerable<Color> Enumerate(Color[] colors)
        {
            retry:
            for (int i = 0; i < colors.Length; i++)
            {
                yield return colors[i];
            }
            goto retry;
        }

        public TaintedUTXOBuilder AddTaint(decimal? value = null)
        {
            value = value ?? GetNextDecimal();
            _TaintedUTXO.Taints.Add((GetNextColor(), value.Value));
            return this;
        }

        public void Write(DotWriter dotWriter)
        {
            dotWriter.WriteAttribute("splines", "ortho");
            foreach (var u in Coins)
            {
                u.Write(dotWriter);
            }
        }

        private Color GetNextColor()
        {
            _Palette.MoveNext();
            return _Palette.Current;
        }
        class PaletteComparer : IComparer<(Color, decimal)>
        {
            Color[] _Palette;
            public PaletteComparer(Color[] palette)
            {
                _Palette = palette;
            }
            public int Compare((Color, decimal) x, (Color, decimal) y)
            {
                var a = Array.IndexOf(_Palette, x.Item1);
                var b = Array.IndexOf(_Palette, y.Item1);
                if (a < b)
                    return -1;
                if (a > b)
                    return 1;
                return 0;
            }
        }
        public TaintedUTXOBuilder MakeCoinjoin()
        {
            var participants = UnspentCoins.GroupBy(o => o.RealOwner).Shuffle(_Rand)
                                .Take(_Rand.Next(Model.CoinJoinBehavior.MinAnonymitySet, Model.CoinJoinBehavior.MaxAnonymitySet)).ToList();

            // Each participant take a random UTXO to mix next
            var coinsToMix = participants
                            .Select(g => g.Shuffle(_Rand))
                            .Select(g => g.FirstOrDefault())
                            .ToArray();
            foreach (var u in coinsToMix)
            {
                UnspentCoins.Remove(u);
                NewTaintedUTXO();
                CurrentTaintedUTXO.CoinJoin(u, coinsToMix);
                CurrentTaintedUTXO.Taints.Sort(new PaletteComparer(_PaletteArray));
                NewTaintedUTXO();
                CurrentTaintedUTXO.ParentIds = new[] { u.Id };
                CurrentTaintedUTXO.RealOwner = u.RealOwner;
                CurrentTaintedUTXO.Taints.AddRange(u.Taints);
            }
            return this;
        }

        public TaintedUTXOBuilder Merge()
        {
            var participants = UnspentCoins.GroupBy(o => o.RealOwner).Shuffle(_Rand).ToList();
            var toMerge = participants.Where(g => g.Where(Mixed).Count() >= Model.MergeBehavior.MinMergedCoins)
                          .Select(g => g.Where(Mixed).Shuffle(_Rand))
                          .Select(g => g.Take(_Rand.Next(Model.MergeBehavior.MinMergedCoins, Model.MergeBehavior.MaxMergedCoins)).ToArray());
            // One participant merge at a time
            foreach (var merge in toMerge.Take(1))
            {
                foreach (var u in merge)
                    UnspentCoins.Remove(u);
                NewTaintedUTXO();
                CurrentTaintedUTXO.CoinJoin(merge[0], merge, true);
                CurrentTaintedUTXO.Taints.Sort(new PaletteComparer(_PaletteArray));

                var taintChanges = CurrentTaintedUTXO.Taints.ToDictionary(t => t.Taint, t => 0m);
                foreach (var taint in CurrentTaintedUTXO.Taints)
                {
                    foreach (var parent in merge)
                    {
                        var parentTaint = parent.Taints.FirstOrDefault(t => t.Taint == taint.Taint);
                        var parentValue = parentTaint.Value == 0.0m ? 1000.0m : parentTaint.Value;
                        taintChanges[taint.Taint] = taintChanges[taint.Taint] + Math.Abs(taint.Value - parentValue);
                        if (taint.Taint == parentTaint.Taint)
                        {
                            taintChanges[taint.Taint] = taintChanges[taint.Taint] + Math.Abs(taint.Value - parentTaint.Value);
                        }
                    }
                }
                CurrentTaintedUTXO.ProbableOwner = taintChanges.ToList().OrderBy(kv => kv.Value).First().Key;
            }
            return this;
        }

        bool Mixed(TaintedUTXO utxo)
        {
            return utxo.Taints.Count != 1 && !utxo.IsMerge;
        }
        bool Lucky<T>(T utxo)
        {
            return _Rand.Next() % 2 == 0;
        }

        private decimal GetNextDecimal()
        {
            return (decimal)_Rand.NextDouble();
        }
    }
}
