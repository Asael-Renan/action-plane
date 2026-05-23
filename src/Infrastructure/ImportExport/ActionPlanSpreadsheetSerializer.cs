using FiveW2H.App.Application;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace FiveW2H.App.Infrastructure.ImportExport;

internal sealed record SpreadsheetImportRow(string SheetName, int RowNumber, IReadOnlyDictionary<string, string> Values);

internal static class ActionPlanSpreadsheetSerializer
{
    private static readonly XNamespace SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace DocumentRelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly string[] ExportHeaders =
    [
        "Item",
        "Data criacao",
        "O que",
        "Porque",
        "Empresa",
        "Quem",
        "Onde",
        "Como",
        "Quando",
        "Quanto",
        "Observacao",
        "Status",
        "Prioridade",
        "Id sistema",
        "Atualizado em"
    ];

    private static readonly string[] ExportTitleRow = ["5W2H - Cronograma de Atividades"];

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["item"] = "item",
        ["id"] = "id",
        ["idsistema"] = "id",
        ["idsistemadoapp"] = "id",
        ["datacriacao"] = "createdAt",
        ["criacao"] = "createdAt",
        ["oque"] = "what",
        ["what"] = "what",
        ["porque"] = "why",
        ["porquê"] = "why",
        ["por que"] = "why",
        ["why"] = "why",
        ["empresa"] = "company",
        ["quem"] = "who",
        ["where"] = "where",
        ["onde"] = "where",
        ["como"] = "how",
        ["when"] = "when",
        ["quando"] = "when",
        ["quanto"] = "howMuch",
        ["custo"] = "howMuch",
        ["howmuch"] = "howMuch",
        ["observacao"] = "notes",
        ["observacoes"] = "notes",
        ["observação"] = "notes",
        ["observações"] = "notes",
        ["notes"] = "notes",
        ["status"] = "status",
        ["prioridade"] = "priority",
        ["priority"] = "priority",
        ["atualizadoem"] = "updatedAt",
        ["updatedat"] = "updatedAt"
    };

    public static async Task ExportAsync(string filePath, IEnumerable<FiveW2HTaskDto> tasks)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            ExportTitleRow,
            ExportHeaders
        };

        var orderedTasks = tasks.ToList();
        for (var index = 0; index < orderedTasks.Count; index++)
        {
            var task = orderedTasks[index];
            rows.Add(new[]
            {
                (index + 1).ToString(CultureInfo.InvariantCulture),
                task.CreatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                task.What,
                task.Why,
                task.Company,
                task.Who,
                task.Where,
                task.How,
                task.When.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                task.HowMuch.ToString("0.##", CultureInfo.InvariantCulture),
                task.Notes,
                ToSpreadsheetStatus(task.Status),
                ToSpreadsheetPriority(task.Priority),
                task.Id.ToString(CultureInfo.InvariantCulture),
                task.UpdatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            });
        }

        await using var stream = File.Create(filePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        var sharedStrings = BuildSharedStringTable(rows);
        var sheetXml = BuildWorksheetXml(rows, ExportHeaders.Length);

        CreateEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
        CreateEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
        CreateEntry(archive, "docProps/app.xml", BuildAppPropertiesXml());
        CreateEntry(archive, "docProps/core.xml", BuildCorePropertiesXml());
        CreateEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
        CreateEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationshipsXml());
        CreateEntry(archive, "xl/styles.xml", BuildStylesXml());
        CreateEntry(archive, "xl/sharedStrings.xml", sharedStrings.Document);
        CreateEntry(archive, "xl/worksheets/sheet1.xml", sheetXml);
    }

    public static async Task<IReadOnlyList<SpreadsheetImportRow>> ReadRowsAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var sharedStrings = ReadSharedStrings(archive);
        var sheetTargets = ReadWorksheetTargets(archive);
        var importedRows = new List<SpreadsheetImportRow>();

        foreach (var (sheetName, target) in sheetTargets)
        {
            var entry = archive.GetEntry(target);
            if (entry is null)
            {
                continue;
            }

            using var worksheetStream = entry.Open();
            var worksheet = XDocument.Load(worksheetStream);
            var rows = ReadWorksheetRows(sheetName, worksheet, sharedStrings);
            importedRows.AddRange(rows);
        }

        return await Task.FromResult(importedRows);
    }

    private static List<SpreadsheetImportRow> ReadWorksheetRows(
        string sheetName,
        XDocument worksheet,
        List<string> sharedStrings)
    {
        var rowElements = worksheet.Root?
            .Element(SpreadsheetNamespace + "sheetData")?
            .Elements(SpreadsheetNamespace + "row")
            .ToList() ?? [];

        if (rowElements.Count == 0)
        {
            return [];
        }

        Dictionary<int, string>? headerMap = null;
        var importedRows = new List<SpreadsheetImportRow>();

        foreach (var row in rowElements)
        {
            var rowNumber = (int?)row.Attribute("r") ?? 0;
            var cellMap = ReadCellMap(row, sharedStrings);
            if (cellMap.Count == 0)
            {
                continue;
            }

            if (headerMap is null)
            {
                headerMap = TryBuildHeaderMap(cellMap);
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (columnIndex, headerName) in headerMap)
            {
                if (cellMap.TryGetValue(columnIndex, out var value))
                {
                    values[headerName] = value.Trim();
                }
            }

            if (values.Count == 0 || values.Values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            importedRows.Add(new SpreadsheetImportRow(sheetName, rowNumber, values));
        }

        return importedRows;
    }

    private static Dictionary<int, string>? TryBuildHeaderMap(IReadOnlyDictionary<int, string> cellMap)
    {
        var mappedHeaders = new Dictionary<int, string>();
        foreach (var (columnIndex, rawHeader) in cellMap)
        {
            var normalizedHeader = NormalizeHeader(rawHeader);
            if (HeaderAliases.TryGetValue(normalizedHeader, out var canonicalHeader))
            {
                mappedHeaders[columnIndex] = canonicalHeader;
            }
        }

        return mappedHeaders.ContainsValue("what") && mappedHeaders.ContainsValue("when")
            ? mappedHeaders
            : null;
    }

    private static Dictionary<int, string> ReadCellMap(XElement row, List<string> sharedStrings)
    {
        var values = new Dictionary<int, string>();

        foreach (var cell in row.Elements(SpreadsheetNamespace + "c"))
        {
            var reference = (string?)cell.Attribute("r");
            if (string.IsNullOrWhiteSpace(reference))
            {
                continue;
            }

            var columnIndex = GetColumnIndex(reference);
            var cellType = (string?)cell.Attribute("t");
            var value = ReadCellValue(cell, cellType, sharedStrings);

            if (!string.IsNullOrWhiteSpace(value))
            {
                values[columnIndex] = value;
            }
        }

        return values;
    }

    private static string ReadCellValue(XElement cell, string? cellType, List<string> sharedStrings)
    {
        if (string.Equals(cellType, "inlineStr", StringComparison.OrdinalIgnoreCase))
        {
            return cell.Element(SpreadsheetNamespace + "is")?.Value ?? string.Empty;
        }

        var rawValue = cell.Element(SpreadsheetNamespace + "v")?.Value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        if (string.Equals(cellType, "s", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedStringIndex) &&
            sharedStringIndex >= 0 &&
            sharedStringIndex < sharedStrings.Count)
        {
            return sharedStrings[sharedStringIndex];
        }

        if (string.Equals(cellType, "b", StringComparison.OrdinalIgnoreCase))
        {
            return rawValue == "1" ? bool.TrueString : bool.FalseString;
        }

        return rawValue;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        return document.Root?
            .Elements(SpreadsheetNamespace + "si")
            .Select(item => string.Concat(item.Descendants(SpreadsheetNamespace + "t").Select(text => text.Value)))
            .ToList() ?? [];
    }

    private static List<(string SheetName, string Target)> ReadWorksheetTargets(ZipArchive archive)
    {
        using var workbookStream = archive.GetEntry("xl/workbook.xml")!.Open();
        using var relationshipsStream = archive.GetEntry("xl/_rels/workbook.xml.rels")!.Open();

        var workbook = XDocument.Load(workbookStream);
        var relationships = XDocument.Load(relationshipsStream);

        var targetByRelationshipId = relationships.Root?
            .Elements(RelationshipsNamespace + "Relationship")
            .ToDictionary(
                relationship => (string?)relationship.Attribute("Id") ?? string.Empty,
                relationship => NormalizeWorksheetTarget((string?)relationship.Attribute("Target") ?? string.Empty),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        return workbook.Root?
            .Element(SpreadsheetNamespace + "sheets")?
            .Elements(SpreadsheetNamespace + "sheet")
            .Select(sheet =>
            {
                var sheetName = (string?)sheet.Attribute("name") ?? "Planilha";
                var relationshipId = (string?)sheet.Attribute(DocumentRelationshipsNamespace + "id") ?? string.Empty;
                targetByRelationshipId.TryGetValue(relationshipId, out var target);
                return (sheetName, target ?? string.Empty);
            })
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Item2))
            .ToList() ?? [];
    }

    private static string NormalizeWorksheetTarget(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return string.Empty;
        }

        return target.StartsWith("/xl/", StringComparison.OrdinalIgnoreCase)
            ? target.TrimStart('/')
            : $"xl/{target.TrimStart('/')}";
    }

    private static (XDocument Document, Dictionary<string, int> IndexByValue) BuildSharedStringTable(IEnumerable<IReadOnlyList<string>> rows)
    {
        var values = rows.SelectMany(row => row).ToList();
        var uniqueValues = new List<string>();
        var indexByValue = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var value in values)
        {
            if (!indexByValue.ContainsKey(value))
            {
                indexByValue[value] = uniqueValues.Count;
                uniqueValues.Add(value);
            }
        }

        var document = new XDocument(
            new XElement(SpreadsheetNamespace + "sst",
                new XAttribute("count", values.Count),
                new XAttribute("uniqueCount", uniqueValues.Count),
                uniqueValues.Select(value =>
                    new XElement(SpreadsheetNamespace + "si",
                        new XElement(SpreadsheetNamespace + "t", value)))));

        return (document, indexByValue);
    }

    private static XDocument BuildWorksheetXml(IReadOnlyList<IReadOnlyList<string>> rows, int mergedColumnsCount)
    {
        var sharedStrings = BuildSharedStringTable(rows).IndexByValue;
        var mergeReference = $"A1:{GetColumnName(mergedColumnsCount - 1)}1";

        return new XDocument(
            new XElement(SpreadsheetNamespace + "worksheet",
                new XElement(SpreadsheetNamespace + "sheetViews",
                    new XElement(SpreadsheetNamespace + "sheetView",
                        new XAttribute("workbookViewId", 0))),
                new XElement(SpreadsheetNamespace + "sheetFormatPr",
                    new XAttribute("defaultRowHeight", 18)),
                new XElement(SpreadsheetNamespace + "sheetData",
                    rows.Select((row, rowIndex) =>
                        new XElement(SpreadsheetNamespace + "row",
                            new XAttribute("r", rowIndex + 1),
                            row.Select((value, columnIndex) =>
                                new XElement(SpreadsheetNamespace + "c",
                                    new XAttribute("r", $"{GetColumnName(columnIndex)}{rowIndex + 1}"),
                                    new XAttribute("t", "s"),
                                    new XElement(SpreadsheetNamespace + "v", sharedStrings[value])))))),
                new XElement(SpreadsheetNamespace + "mergeCells",
                    new XAttribute("count", 1),
                    new XElement(SpreadsheetNamespace + "mergeCell", new XAttribute("ref", mergeReference)))));
    }

    private static XDocument BuildContentTypesXml()
    {
        XNamespace contentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

        return new XDocument(
            new XElement(contentTypesNamespace + "Types",
                new XElement(contentTypesNamespace + "Default",
                    new XAttribute("Extension", "rels"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(contentTypesNamespace + "Default",
                    new XAttribute("Extension", "xml"),
                    new XAttribute("ContentType", "application/xml")),
                new XElement(contentTypesNamespace + "Override",
                    new XAttribute("PartName", "/xl/workbook.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                new XElement(contentTypesNamespace + "Override",
                    new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                new XElement(contentTypesNamespace + "Override",
                    new XAttribute("PartName", "/xl/sharedStrings.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml")),
                new XElement(contentTypesNamespace + "Override",
                    new XAttribute("PartName", "/xl/styles.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")),
                new XElement(contentTypesNamespace + "Override",
                    new XAttribute("PartName", "/docProps/core.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-package.core-properties+xml")),
                new XElement(contentTypesNamespace + "Override",
                    new XAttribute("PartName", "/docProps/app.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.extended-properties+xml"))));
    }

    private static XDocument BuildRootRelationshipsXml()
    {
        return new XDocument(
            new XElement(RelationshipsNamespace + "Relationships",
                new XElement(RelationshipsNamespace + "Relationship",
                    new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                    new XAttribute("Target", "xl/workbook.xml")),
                new XElement(RelationshipsNamespace + "Relationship",
                    new XAttribute("Id", "rId2"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"),
                    new XAttribute("Target", "docProps/core.xml")),
                new XElement(RelationshipsNamespace + "Relationship",
                    new XAttribute("Id", "rId3"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"),
                    new XAttribute("Target", "docProps/app.xml"))));
    }

    private static XDocument BuildWorkbookXml()
    {
        return new XDocument(
            new XElement(SpreadsheetNamespace + "workbook",
                new XAttribute(XNamespace.Xmlns + "r", DocumentRelationshipsNamespace),
                new XElement(SpreadsheetNamespace + "sheets",
                    new XElement(SpreadsheetNamespace + "sheet",
                        new XAttribute("name", "Plano de Acao"),
                        new XAttribute("sheetId", 1),
                        new XAttribute(DocumentRelationshipsNamespace + "id", "rId1")))));
    }

    private static XDocument BuildWorkbookRelationshipsXml()
    {
        return new XDocument(
            new XElement(RelationshipsNamespace + "Relationships",
                new XElement(RelationshipsNamespace + "Relationship",
                    new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                    new XAttribute("Target", "worksheets/sheet1.xml")),
                new XElement(RelationshipsNamespace + "Relationship",
                    new XAttribute("Id", "rId2"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                    new XAttribute("Target", "styles.xml")),
                new XElement(RelationshipsNamespace + "Relationship",
                    new XAttribute("Id", "rId3"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings"),
                    new XAttribute("Target", "sharedStrings.xml"))));
    }

    private static XDocument BuildStylesXml()
    {
        return new XDocument(
            new XElement(SpreadsheetNamespace + "styleSheet",
                new XElement(SpreadsheetNamespace + "fonts", new XAttribute("count", 1),
                    new XElement(SpreadsheetNamespace + "font",
                        new XElement(SpreadsheetNamespace + "sz", new XAttribute("val", 11)),
                        new XElement(SpreadsheetNamespace + "name", new XAttribute("val", "Calibri")))),
                new XElement(SpreadsheetNamespace + "fills", new XAttribute("count", 2),
                    new XElement(SpreadsheetNamespace + "fill", new XElement(SpreadsheetNamespace + "patternFill", new XAttribute("patternType", "none"))),
                    new XElement(SpreadsheetNamespace + "fill", new XElement(SpreadsheetNamespace + "patternFill", new XAttribute("patternType", "gray125")))),
                new XElement(SpreadsheetNamespace + "borders", new XAttribute("count", 1),
                    new XElement(SpreadsheetNamespace + "border",
                        new XElement(SpreadsheetNamespace + "left"),
                        new XElement(SpreadsheetNamespace + "right"),
                        new XElement(SpreadsheetNamespace + "top"),
                        new XElement(SpreadsheetNamespace + "bottom"),
                        new XElement(SpreadsheetNamespace + "diagonal"))),
                new XElement(SpreadsheetNamespace + "cellStyleXfs", new XAttribute("count", 1),
                    new XElement(SpreadsheetNamespace + "xf",
                        new XAttribute("numFmtId", 0),
                        new XAttribute("fontId", 0),
                        new XAttribute("fillId", 0),
                        new XAttribute("borderId", 0))),
                new XElement(SpreadsheetNamespace + "cellXfs", new XAttribute("count", 1),
                    new XElement(SpreadsheetNamespace + "xf",
                        new XAttribute("numFmtId", 0),
                        new XAttribute("fontId", 0),
                        new XAttribute("fillId", 0),
                        new XAttribute("borderId", 0),
                        new XAttribute("xfId", 0))),
                new XElement(SpreadsheetNamespace + "cellStyles", new XAttribute("count", 1),
                    new XElement(SpreadsheetNamespace + "cellStyle",
                        new XAttribute("name", "Normal"),
                        new XAttribute("xfId", 0),
                        new XAttribute("builtinId", 0)))));
    }

    private static XDocument BuildAppPropertiesXml()
    {
        XNamespace propertiesNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
        XNamespace vtNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";

        return new XDocument(
            new XElement(propertiesNamespace + "Properties",
                new XAttribute(XNamespace.Xmlns + "vt", vtNamespace),
                new XElement(propertiesNamespace + "Application", "5W2H Management"),
                new XElement(propertiesNamespace + "DocSecurity", "0"),
                new XElement(propertiesNamespace + "ScaleCrop", "false"),
                new XElement(propertiesNamespace + "HeadingPairs",
                    new XElement(vtNamespace + "vector",
                        new XAttribute("size", 2),
                        new XAttribute("baseType", "variant"),
                        new XElement(vtNamespace + "variant", new XElement(vtNamespace + "lpstr", "Worksheets")),
                        new XElement(vtNamespace + "variant", new XElement(vtNamespace + "i4", 1)))),
                new XElement(propertiesNamespace + "TitlesOfParts",
                    new XElement(vtNamespace + "vector",
                        new XAttribute("size", 1),
                        new XAttribute("baseType", "lpstr"),
                        new XElement(vtNamespace + "lpstr", "Plano de Acao")))));
    }

    private static XDocument BuildCorePropertiesXml()
    {
        XNamespace cpNamespace = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
        XNamespace dcNamespace = "http://purl.org/dc/elements/1.1/";
        XNamespace dctermsNamespace = "http://purl.org/dc/terms/";
        XNamespace xsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        var now = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "Z";

        return new XDocument(
            new XElement(cpNamespace + "coreProperties",
                new XAttribute(XNamespace.Xmlns + "dc", dcNamespace),
                new XAttribute(XNamespace.Xmlns + "dcterms", dctermsNamespace),
                new XAttribute(XNamespace.Xmlns + "xsi", xsiNamespace),
                new XElement(dcNamespace + "creator", "5W2H Management"),
                new XElement(cpNamespace + "lastModifiedBy", "5W2H Management"),
                new XElement(dctermsNamespace + "created",
                    new XAttribute(xsiNamespace + "type", "dcterms:W3CDTF"),
                    now),
                new XElement(dctermsNamespace + "modified",
                    new XAttribute(xsiNamespace + "type", "dcterms:W3CDTF"),
                    now)));
    }

    private static void CreateEntry(ZipArchive archive, string path, XDocument document)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        document.Save(writer);
    }

    private static int GetColumnIndex(string cellReference)
    {
        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var index = 0;
        foreach (var letter in letters)
        {
            index = (index * 26) + (char.ToUpperInvariant(letter) - 'A' + 1);
        }

        return index - 1;
    }

    private static string GetColumnName(int index)
    {
        var dividend = index + 1;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = ((char)('A' + modulo)) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static string NormalizeHeader(string value)
    {
        return value
            .Trim()
            .Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (builder, character) =>
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }

                return builder;
            })
            .ToString();
    }

    private static string ToSpreadsheetStatus(Core.Models.TaskStatus status) => status switch
    {
        Core.Models.TaskStatus.Completed => "Concluido",
        Core.Models.TaskStatus.InProgress => "Em andamento",
        Core.Models.TaskStatus.OnHold => "Em espera",
        Core.Models.TaskStatus.Cancelled => "Cancelado",
        _ => "Pendente"
    };

    private static string ToSpreadsheetPriority(Core.Models.Priority priority) => priority switch
    {
        Core.Models.Priority.Critical => "Critica",
        Core.Models.Priority.High => "Alta",
        Core.Models.Priority.Low => "Baixa",
        _ => "Media"
    };
}
