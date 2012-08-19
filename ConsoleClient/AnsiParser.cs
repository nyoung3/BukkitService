using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Media;

namespace ConsoleClient {
    public class AnsiParser {
        [ImmutableObject(true)]
        public struct TextBlock {
            public TextBlock(String text, Color color) {
                Text = text;
                Color = color;
            }

            public readonly Color Color;
            public readonly String Text;

            public static implicit operator Inline(TextBlock block) {
                return new Run(block.Text) {Foreground = new SolidColorBrush(block.Color)};
            }
        }

        public class TextBlockCollection : List<TextBlock> {
            public static implicit operator Block(TextBlockCollection collection) {
                var p = new Paragraph();
                foreach (var b in collection) {
                    p.Inlines.Add(b);
                }
                p.LineHeight = 1;
                return p;
            }
        }

        static Color color = Colors.Black;
        private readonly object ParseLock = new object();
        public TextBlockCollection ParseText(string text) {
            lock (ParseLock) {
                text = text.Trim();
                var blocks = new TextBlockCollection();

                int index;
                while ((index = text.IndexOf('\u001b')) != -1) {
                    var msg = text.Substring(0, index);
                    if (!string.IsNullOrEmpty(msg)) blocks.Add(new TextBlock(msg, color));

                    var mindex = text.Substring(index).IndexOf('m');
                    if (mindex == -1) break;
                    var ccs = text.Substring(index + 2, mindex - 2);
                    text = text.Length > mindex + 1 ? text.Substring(index + mindex + 1) : "";
                    int cci;
                    if (!int.TryParse(ccs, out cci)) continue;
                    var cc = ParseAnsiColorCode(cci);
                    if (cc != null) color = (Color)cc;
                }
                blocks.Add(new TextBlock(text, color));

                return blocks;
            }
        }

        private static Color? ParseAnsiColorCode(int code) {
            switch (code) {
                case 0:
                    return Colors.Black;
                case 30:
                    return Colors.Black;
                case 31:
                    return Colors.Red;
                case 32:
                    return Colors.Green;
                case 33:
                    return Colors.DarkOrange;
                case 34:
                    return Colors.Blue;
                case 35:
                    return Colors.DarkMagenta;
                case 36:
                    return Colors.DarkCyan;
                case 37:
                    return Colors.Black;
                default:
                    return null;
            }
        }
    }
}