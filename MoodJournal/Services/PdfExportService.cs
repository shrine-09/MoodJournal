using System.Globalization;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using MoodJournal.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace MoodJournal.Services;

public class PdfExportService
{
    private static bool _fontResolverInitialized;

    public async Task ExportEntriesAsync(IEnumerable<JournalEntry> entries, DateTime from, DateTime to)
    {
        var list = entries
            .OrderBy(e => e.EntryDate)
            .ToList();

        EnsureFontResolver();

        var doc = new PdfDocument();
        doc.Info.Title = $"MoodJournal Export {from:yyyy-MM-dd} to {to:yyyy-MM-dd}";

        const double margin = 40;
        const double gap = 3;

        var titleFont = new XFont("OpenSans", 18, XFontStyleEx.Bold);
        var hFont     = new XFont("OpenSans", 12, XFontStyleEx.Bold);
        var bodyFont  = new XFont("OpenSans", 11, XFontStyleEx.Regular);
        var smallFont = new XFont("OpenSans", 9,  XFontStyleEx.Regular);

        var (page, gfx, y) = NewPage(doc, margin);

        var width = page.Width.Point - margin * 2;
        var pageBottom = page.Height.Point - margin;

        //header
        y = DrawLine(gfx, "MoodJournal Export", titleFont, margin, y, width) + 2;
        y = DrawLine(
                gfx,
                $"{from:MMM d, yyyy} – {to:MMM d, yyyy} • {list.Count} entries",
                smallFont,
                margin,
                y,
                width
            ) + 12;

        foreach (var e in list)
        {
            if (y > pageBottom - 120)
                (page, gfx, y) = NewPage(doc, margin);

            var d = ParseEntryDateSafe(e.EntryDate);

            y = DrawLine(gfx, d.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture), hFont, margin, y, width);
            y = DrawLine(gfx, $"Primary mood: {(e.PrimaryMood ?? "").Trim()}", smallFont, margin, y, width);

            var tags = string.Join(", ", SplitTags(e.TagsCsv));
            if (!string.IsNullOrWhiteSpace(tags))
                y = DrawLine(gfx, $"Tags: {tags}", smallFont, margin, y, width);

            if (!string.IsNullOrWhiteSpace(e.Title))
                y = DrawLine(gfx, e.Title.Trim(), hFont, margin, y + 6, width);

            var body = (e.ContentText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(body))
                body = "(No content)";

            y += 6;

            foreach (var line in WrapText(gfx, body, bodyFont, width))
            {
                if (y > pageBottom - 20)
                    (page, gfx, y) = NewPage(doc, margin);

                gfx.DrawString(line, bodyFont, XBrushes.Black,
                    new XRect(margin, y, width, 20), XStringFormats.TopLeft);

                y += bodyFont.GetHeight() + gap;
            }

            y += 14;
        }

        var fileName = $"MoodJournal_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf";
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        doc.Save(path);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Export Journal PDF",
            File = new ShareFile(path)
        });
    }

    private static void EnsureFontResolver()
    {
        if (_fontResolverInitialized) return;

        if (PdfSharp.Capabilities.Build.IsCoreBuild && GlobalFontSettings.FontResolver == null)
            GlobalFontSettings.FontResolver = new SimpleFontResolver();

        _fontResolverInitialized = true;
    }

    private static (PdfPage page, XGraphics gfx, double y) NewPage(PdfDocument doc, double margin)
    {
        var page = doc.AddPage();
        page.Size = PageSize.A4;
        page.Orientation = PageOrientation.Portrait;
        var gfx = XGraphics.FromPdfPage(page);
        return (page, gfx, margin);
    }

    private static double DrawLine(XGraphics gfx, string text, XFont font, double x, double y, double width)
    {
        gfx.DrawString(text, font, XBrushes.Black,
            new XRect(x, y, width, 1000), XStringFormats.TopLeft);

        return y + font.GetHeight() + 3;
    }

    private static IEnumerable<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        var paragraphs = text.Replace("\r\n", "\n").Split('\n');

        foreach (var para in paragraphs)
        {
            var p = para.TrimEnd();
            if (p.Length == 0)
            {
                yield return "";
                continue;
            }

            var words = p.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var line = "";

            foreach (var w in words)
            {
                var test = string.IsNullOrEmpty(line) ? w : $"{line} {w}";
                if (gfx.MeasureString(test, font).Width <= maxWidth)
                    line = test;
                else
                {
                    if (!string.IsNullOrEmpty(line)) yield return line;
                    line = w;
                }
            }

            if (!string.IsNullOrEmpty(line)) yield return line;
        }
    }

    private static DateTime ParseEntryDateSafe(string entryDate)
    {
        if (DateTime.TryParseExact(entryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            return dt.Date;

        return DateTime.Today;
    }

    private static IEnumerable<string> SplitTags(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return Array.Empty<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

internal sealed class SimpleFontResolver : IFontResolver
{
    public string DefaultFontName => "OpenSans";

    public byte[] GetFont(string faceName)
    {
        return LoadFont("OpenSans-Regular.ttf");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo("OpenSans#");
    }

    private static byte[] LoadFont(string fileName)
    {
        using var streamTask = FileSystem.OpenAppPackageFileAsync(fileName);
        streamTask.Wait();
        using var stream = streamTask.Result;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

