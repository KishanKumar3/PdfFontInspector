using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace PdfFontInspector
{
    class Program
    {
        static HashSet<string> uniqueFonts = new HashSet<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("PDF Font Inspector by Kishan Kumar");
            Console.WriteLine("========================================");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: PdfFontInspector <path-to-pdf>");
                return;
            }

            string pdfPath = args[0];
            if (!System.IO.File.Exists(pdfPath))
            {
                Console.WriteLine("File not found: " + pdfPath);
                return;
            }

            try
            {
                using var reader = new PdfReader(pdfPath);
                using var pdfDoc = new PdfDocument(reader);

                Console.WriteLine($"Inspecting: {pdfPath}");
                Console.WriteLine($"Pages: {pdfDoc.GetNumberOfPages()}");
                Console.WriteLine("----------------------------------------");

                for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                {
                    try
                    {
                        var page = pdfDoc.GetPage(pageNum);

                        // 1. Check fonts
                        var resources = page.GetResources();
                        InspectResources(resources, pageNum);

                        // 2. Check if text is extractable
                        string text = PdfTextExtractor.GetTextFromPage(page);
                        int charCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Length;
                        Console.WriteLine($"Page {pageNum} → Copyable text: {(charCount > 0 ? $"YES ({charCount} chars)" : "NO")}");
                    }
                    catch (Exception pageEx)
                    {
                        Console.WriteLine($"Page {pageNum} → Error processing page: {pageEx.Message}");
                    }
                }

                Console.WriteLine("----------------------------------------");
                Console.WriteLine("Unique fonts found:");
                foreach (var font in uniqueFonts)
                {
                    Console.WriteLine($"- {font}");
                }

                Console.WriteLine("========================================");
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error reading PDF: " + ex.Message);
            }
        }

        static void InspectResources(PdfResources resources, int pageNum)
        {
            if (resources == null) return;

            var fontDict = resources.GetResource(PdfName.Font) as PdfDictionary;
            if (fontDict != null)
            {
                foreach (PdfName fontName in fontDict.KeySet())
                {
                    var font = fontDict.GetAsDictionary(fontName);
                    string baseFontStr = font?.GetAsName(PdfName.BaseFont)?.ToString() ?? "(unknown)";
                    string subtypeStr = font?.GetAsName(PdfName.Subtype)?.ToString() ?? "(unknown)";
                    bool subset = baseFontStr.Contains("+");
                    string fontId = $"{baseFontStr} | {subtypeStr}";
                    if (uniqueFonts.Add(fontId))
                    {
                        Console.WriteLine($"Page {pageNum} → Font: {baseFontStr} | Type: {subtypeStr} | Subset: {subset}");
                    }
                }
            }

            var xObjDict = resources.GetResource(PdfName.XObject) as PdfDictionary;
            if (xObjDict != null)
            {
                foreach (PdfName xobjName in xObjDict.KeySet())
                {
                    var xobjStream = xObjDict.GetAsStream(xobjName);
                    if (xobjStream != null)
                    {
                        var xobjDict = xobjStream.GetAsDictionary(PdfName.Resources);
                        if (xobjDict != null)
                        {
                            var xobjResources = new PdfResources(xobjDict);
                            InspectResources(xobjResources, pageNum);
                        }
                    }
                }
            }

            var patternDict = resources.GetResource(PdfName.Pattern) as PdfDictionary;
            if (patternDict != null)
            {
                foreach (PdfName patternName in patternDict.KeySet())
                {
                    var patternStream = patternDict.GetAsStream(patternName);
                    if (patternStream != null)
                    {
                        var patDict = patternStream.GetAsDictionary(PdfName.Resources);
                        if (patDict != null)
                        {
                            var patResources = new PdfResources(patDict);
                            InspectResources(patResources, pageNum);
                        }
                    }
                }
            }
        }
    }
}
