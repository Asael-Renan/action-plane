using Xunit;
using Moq;
using FiveW2H.App.Application;
using FiveW2H.App.Core.Models;
using FiveW2H.App.Data;
using FiveW2H.App.Infrastructure.ImportExport;
using TaskStatus = FiveW2H.App.Core.Models.TaskStatus;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace FiveW2H.Tests;

public class BackupServiceTests
{
    private static readonly string[] TemplateHeaders = ["Item", "Data criação", "O que", "Porque", "Quem", "Quando", "Observação", "Status"];
    private static readonly string[] TemplateAddRow = ["1", "29/12/2025", "Adequar procedimento", "Atender item 7.5", "", "31/12/2025", "Nota", "Concluído"];
    private static readonly string[] TemplateUpdateRow = ["1", "29/12/2025", "Adequar procedimento", "Atender item 7.5", "", "31/12/2025", "Nova nota", "Concluído"];
    private static readonly string[] TemplateTitleRow = ["XXXXXX\nCRONOGRAMA DE ATIVIDADES - 2026"];
    private static readonly IReadOnlyList<string>[] TemplateAddRows = [TemplateAddRow];
    private static readonly IReadOnlyList<string>[] TemplateUpdateRows = [TemplateUpdateRow];

    private readonly BackupService _service;
    private readonly Mock<ITaskRepository> _mockRepository;

    public BackupServiceTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _service = new BackupService(_mockRepository.Object);
    }

    [Fact]
    public async Task ExportAsyncWithCsvExtensionWritesCsvContent()
    {
        var tasks = new[]
        {
            new FiveW2HTaskDto
            {
                Id = 1,
                What = "Task 1",
                Why = "Reason 1",
                Where = "Location 1",
                Company = "Acme",
                When = new DateTime(2024, 1, 1),
                Who = "Person 1",
                How = "Method 1",
                HowMuch = 1000,
                Status = TaskStatus.Pending,
                Priority = Priority.High,
                Notes = "Notes 1"
            }
        };

        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");

        try
        {
            await _service.ExportAsync(filePath, tasks);

            Assert.True(File.Exists(filePath));
            var content = await File.ReadAllTextAsync(filePath);
            Assert.Contains("Task 1", content);
            Assert.Contains("Acme", content);
            Assert.Contains("1000", content);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExportAsyncWithSpreadsheetExtensionWritesWorkbook()
    {
        var tasks = new[]
        {
            new FiveW2HTaskDto
            {
                Id = 7,
                What = "Task 1",
                Why = "Reason 1",
                Where = "Location 1",
                Company = "Acme",
                When = new DateTime(2026, 5, 22),
                Who = "Person 1",
                How = "Method 1",
                HowMuch = 1000,
                Status = TaskStatus.Pending,
                Priority = Priority.High,
                Notes = "Notes 1",
                CreatedAt = new DateTime(2026, 5, 1),
                UpdatedAt = new DateTime(2026, 5, 2)
            }
        };

        var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");

        try
        {
            await _service.ExportAsync(filePath, tasks);

            Assert.True(File.Exists(filePath));

            using var archive = ZipFile.OpenRead(filePath);
            Assert.NotNull(archive.GetEntry("xl/workbook.xml"));
            Assert.NotNull(archive.GetEntry("xl/worksheets/sheet1.xml"));
            Assert.NotNull(archive.GetEntry("xl/sharedStrings.xml"));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ImportAsyncWithSpreadsheetTemplateAddsTaskUsingDefaults()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.xlsx");
        CreateTemplateWorkbook(
            filePath,
            TemplateHeaders,
            TemplateAddRows);

        _mockRepository
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(Array.Empty<FiveW2HTask>());

        _mockRepository
            .Setup(repository => repository.AddAsync(It.IsAny<FiveW2HTask>()))
            .ReturnsAsync(42);

        try
        {
            var result = await _service.ImportAsync(filePath);

            Assert.Equal(1, result.ImportedCount);
            Assert.Equal(0, result.UpdatedCount);
            Assert.Empty(result.Errors);

            _mockRepository.Verify(repository => repository.AddAsync(It.Is<FiveW2HTask>(task =>
                task.What == "Adequar procedimento" &&
                task.Why == "Atender item 7.5" &&
                task.Who == "Nao informado" &&
                task.How == "Importado da planilha" &&
                task.Status == TaskStatus.Completed &&
                task.Priority == Priority.Medium &&
                task.HowMuch == 0m)), Times.Once);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ImportAsyncWithSpreadsheetTemplateUpdatesExistingTaskInsteadOfDuplicating()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.xlsx");
        CreateTemplateWorkbook(
            filePath,
            TemplateHeaders,
            TemplateUpdateRows);

        var existingTask = new FiveW2HTask
        {
            Id = 5,
            What = "Adequar procedimento",
            Why = "Atender item 7.5",
            Company = string.Empty,
            Where = "Qualidade",
            Who = "Carlos",
            How = "Revisar documentos",
            When = new DateTime(2025, 12, 31),
            Notes = "Nota antiga",
            Status = TaskStatus.Pending,
            Priority = Priority.High,
            HowMuch = 500,
            CreatedAt = new DateTime(2025, 12, 29),
            UpdatedAt = new DateTime(2025, 12, 29)
        };

        _mockRepository
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[] { existingTask });

        _mockRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<FiveW2HTask>()))
            .ReturnsAsync(true);

        try
        {
            var result = await _service.ImportAsync(filePath);

            Assert.Equal(0, result.ImportedCount);
            Assert.Equal(1, result.UpdatedCount);
            Assert.Empty(result.Errors);

            _mockRepository.Verify(repository => repository.UpdateAsync(It.Is<FiveW2HTask>(task =>
                task.Id == existingTask.Id &&
                task.Who == "Carlos" &&
                task.How == "Revisar documentos" &&
                task.Where == "Qualidade" &&
                task.Priority == Priority.High &&
                task.Status == TaskStatus.Completed &&
                task.Notes == "Nova nota")), Times.Once);

            _mockRepository.Verify(repository => repository.AddAsync(It.IsAny<FiveW2HTask>()), Times.Never);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsyncWithTasksReturnsJsonString()
    {
        var tasks = new[]
        {
            new FiveW2HTask
            {
                Id = 1,
                What = "Task 1",
                Why = "Reason 1",
                Company = "Acme",
                When = new DateTime(2024, 1, 1),
                Who = "Person 1",
                How = "Method 1",
                HowMuch = 1000
            }
        };

        var json = await _service.ExportToJsonAsync(tasks);

        Assert.NotEmpty(json);
        Assert.Contains("Task 1", json);
        Assert.Contains("Acme", json);
        Assert.Contains("1000", json);
    }

    private static void CreateTemplateWorkbook(string filePath, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var allRows = new List<IReadOnlyList<string>>
        {
            TemplateTitleRow,
            headers
        };

        allRows.AddRange(rows);

        var sharedStrings = BuildSharedStringMap(allRows);

        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        WriteEntry(archive, "[Content_Types].xml",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml" />
              <Default Extension="xml" ContentType="application/xml" />
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml" />
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml" />
              <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml" />
            </Types>
            """);
        WriteEntry(archive, "_rels/.rels",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml" />
            </Relationships>
            """);
        WriteEntry(archive, "xl/workbook.xml",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                      xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets>
                <sheet name="Plan1" sheetId="1" r:id="rId1" />
              </sheets>
            </workbook>
            """);
        WriteEntry(archive, "xl/_rels/workbook.xml.rels",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml" />
              <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml" />
            </Relationships>
            """);
        WriteEntry(archive, "xl/sharedStrings.xml", BuildSharedStringsXml(sharedStrings.Keys));
        WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(allRows, sharedStrings));
    }

    private static Dictionary<string, int> BuildSharedStringMap(IEnumerable<IReadOnlyList<string>> rows)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var value in rows.SelectMany(row => row))
        {
            if (!map.ContainsKey(value))
            {
                map[value] = map.Count;
            }
        }

        return map;
    }

    private static string BuildSharedStringsXml(IEnumerable<string> values)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var list = values.ToList();
        var document = new XDocument(
            new XElement(ns + "sst",
                new XAttribute("count", list.Count),
                new XAttribute("uniqueCount", list.Count),
                list.Select(value => new XElement(ns + "si", new XElement(ns + "t", value)))));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildWorksheetXml(IReadOnlyList<IReadOnlyList<string>> rows, Dictionary<string, int> sharedStrings)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var document = new XDocument(
            new XElement(ns + "worksheet",
                new XElement(ns + "sheetData",
                    rows.Select((row, rowIndex) =>
                        new XElement(ns + "row",
                            new XAttribute("r", rowIndex + 1),
                            row.Select((value, columnIndex) =>
                                new XElement(ns + "c",
                                    new XAttribute("r", $"{GetColumnName(columnIndex)}{rowIndex + 1}"),
                                    new XAttribute("t", "s"),
                                    new XElement(ns + "v", sharedStrings[value]))))))));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static string GetColumnName(int index)
    {
        var dividend = index + 1;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = (char)('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
