using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using YourJournal.Models;
using PdfColors = QuestPDF.Helpers.Colors;

namespace YourJournal.Services;

public class PdfExportService
{
    private readonly DatabaseService _database;
    private readonly AuthService _authService;

    public PdfExportService(DatabaseService database, AuthService authService)
    {
        _database = database;
        _authService = authService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<(string FilePath, string FileName)> ExportToPdfAsync(DateTime startDate, DateTime endDate)
    {
        if (_authService.CurrentUser == null)
            throw new InvalidOperationException("User not authenticated");

        var entries = await _database.FilterEntriesAsync(
            _authService.CurrentUser.Id,
            startDate,
            endDate);

        var fileName = $"Journal_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf";
        
        // Save to Downloads folder for easy access
        var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadsFolder = Path.Combine(downloadsPath, "Downloads");
        
        if (!Directory.Exists(downloadsFolder))
            Directory.CreateDirectory(downloadsFolder);
            
        var filePath = Path.Combine(downloadsFolder, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(PdfColors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(PdfColors.Black));

                page.Header()
                    .Text($"Your Journal - {_authService.CurrentUser.FullName}")
                    .SemiBold().FontSize(20).FontColor(PdfColors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Export Date Range: {startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}")
                            .FontSize(12).Italic();

                        col.Item().Text($"Total Entries: {entries.Count}").FontSize(12);

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(PdfColors.Grey.Lighten2);

                        foreach (var entry in entries)
                        {
                            col.Item().PaddingTop(15).Column(entryCol =>
                            {
                                entryCol.Item().Text(entry.Date.ToString("dddd, MMMM dd, yyyy"))
                                    .SemiBold().FontSize(14);

                                if (!string.IsNullOrEmpty(entry.Title))
                                {
                                    entryCol.Item().PaddingTop(5).Text(entry.Title)
                                        .SemiBold().FontSize(12);
                                }

                                entryCol.Item().PaddingTop(5).Text($"Mood: {entry.PrimaryMood}")
                                    .FontSize(10).FontColor(PdfColors.Grey.Darken1);

                                if (!string.IsNullOrEmpty(entry.Category))
                                {
                                    entryCol.Item().Text($"Category: {entry.Category}")
                                        .FontSize(10).FontColor(PdfColors.Grey.Darken1);
                                }

                                entryCol.Item().PaddingTop(10).Text(entry.Content)
                                    .FontSize(11).LineHeight(1.5f);

                                entryCol.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(PdfColors.Grey.Lighten3);
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        })
        .GeneratePdf(filePath);

        // Open the file in default PDF viewer
        try
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }
        catch
        {
            // If opening fails, user can still access the file in Downloads
        }

        return (filePath, fileName);
    }
}
