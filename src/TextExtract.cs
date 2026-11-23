using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GinPdfRw
{
    public class PdfTextLine
    {
        public string Text;
        public double X, Y, W, H;   // PDF points, origin top-left of the page
        public double FontPt;
    }

    public class PdfPageText
    {
        public double PageWidth, PageHeight;
        public List<PdfTextLine> Lines = new List<PdfTextLine>();
    }

    public static class TextExtract
    {
        // Isolated so PdfPig is only loaded when text editing is actually used.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PdfPageText Extract(string path, int pageIndex)
        {
            var result = new PdfPageText();
            using (var doc = UglyToad.PdfPig.PdfDocument.Open(path))
            {
                var page = doc.GetPage(pageIndex + 1);
                result.PageWidth = page.Width;
                result.PageHeight = page.Height;
                var words = new List<PdfTextLine>();
                foreach (var w in page.GetWords())
                {
                    if (string.IsNullOrWhiteSpace(w.Text)) continue;
                    var b = w.BoundingBox;
                    double pt = 0;
                    foreach (var l in w.Letters) { pt = l.PointSize; break; }
                    words.Add(new PdfTextLine {
                        Text = w.Text, X = b.Left, Y = page.Height - b.Top, W = b.Width, H = b.Height,
                        FontPt = pt > 0 ? pt : b.Height * 0.85
                    });
                }
                // Group words into lines: similar vertical position and small horizontal gap.
                words.Sort(delegate(PdfTextLine a, PdfTextLine c) {
                    int r = (a.Y + a.H / 2).CompareTo(c.Y + c.H / 2);
                    return r != 0 ? r : a.X.CompareTo(c.X);
                });
                var used = new bool[words.Count];
                for (int i = 0; i < words.Count; i++)
                {
                    if (used[i]) continue;
                    var cur = words[i]; used[i] = true;
                    var line = new PdfTextLine { Text = cur.Text, X = cur.X, Y = cur.Y, W = cur.W, H = cur.H, FontPt = cur.FontPt };
                    bool extended = true;
                    while (extended)
                    {
                        extended = false;
                        int best = -1; double bestGap = double.MaxValue;
                        for (int j = 0; j < words.Count; j++)
                        {
                            if (used[j]) continue;
                            var o = words[j];
                            double cy1 = line.Y + line.H / 2, cy2 = o.Y + o.H / 2;
                            if (Math.Abs(cy1 - cy2) > Math.Max(line.H, o.H) * 0.5) continue;
                            double gap = o.X - (line.X + line.W);
                            if (gap < -2 || gap > Math.Max(line.FontPt, 6) * 1.2) continue;
                            if (gap < bestGap) { bestGap = gap; best = j; }
                        }
                        if (best >= 0)
                        {
                            var o = words[best]; used[best] = true;
                            line.Text += (bestGap > 1 ? " " : "") + o.Text;
                            double top = Math.Min(line.Y, o.Y), bot = Math.Max(line.Y + line.H, o.Y + o.H);
                            line.W = (o.X + o.W) - line.X; line.Y = top; line.H = bot - top;
                            extended = true;
                        }
                    }
                    result.Lines.Add(line);
                }
            }
            return result;
        }
    }
}
