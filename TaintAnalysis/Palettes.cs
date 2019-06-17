using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TaintAnalysis
{
    public static class Palettes
    {
        public static Color[][] Warm =
        {
            new Color[0],
            new Color[] { C("F4B656") },
            new Color[] { C("F8CD61"), C("E97E3B") },
            new Color[] { C("F8CD61"), C("E97E3B"), C("A34529") },
            new Color[] { C("FBDB68"), C("EF9C49"), C("E45E2D"), C("8B412B") },
            new Color[] { C("FDEA6F"), C("F3B355"), C("E97E3B"), C("CF4F29"), C("723C2C") },
            new Color[] { C("FDEB73"), C("F6C15B"), C("ED9445"), C("E66731"), C("B84A29"), C("6A3A2D") },
            new Color[] { C("FDEB78"), C("F8CB60"), C("F0A44D"), C("E97E3B"), C("E3572A"), C("A64629"), C("63392D") },
            new Color[] { C("FDEC7C"), C("F9D464"), C("F2B154"), C("EC8F43"), C("E66C33"), C("D35028"), C("98432A"), C("5B372E") },
            new Color[] { C("FDED82"), C("FBDC68"), C("F5BC59"), C("EF9C49"), C("E97E3B"), C("E45E2D"), C("C04B29"), C("8A402B"), C("54362F") },
            new Color[] { C("FDED86"), C("FCE36B"), C("F7C65D"), C("F1A84F"), C("EC8C41"), C("E76F34"), C("E25328"), C("B04829"), C("7E3E2B"), C("4C3430") },
        };

        private static Color C(string hex)
        {
            return Color.FromArgb(
            int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
            int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
            int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }
    }
}
