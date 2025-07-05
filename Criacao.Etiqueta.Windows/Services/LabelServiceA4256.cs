using iTextSharp.text;
using iTextSharp.text.pdf;
using Criacao.Etiqueta.Windows.Models;
using Criacao.Etiqueta.Windows.AbstractServices;

namespace Criacao.Etiqueta.Windows.Services;

public class LabelServiceA4256 : LabelAbstractService
{
    private const int LABELS_PER_PAGE = 33;
    private const decimal SAFETY_MARGIN = 0.3m * CONVERTER_CM_TO_POINT;
    private const decimal HORIZONTAL_MARGIN = 0.975m * CONVERTER_CM_TO_POINT;
    private const decimal VERTICAL_MARGIN = 0.88m * CONVERTER_CM_TO_POINT;
    private const decimal LABEL_WIDTH = 6.35m * CONVERTER_CM_TO_POINT;
    private const decimal LABEL_HEIGHT = 2.54m * CONVERTER_CM_TO_POINT;

    
    public override byte[] GenerateLabelsPdf(List<LabelInfo> labels)
    {
        using MemoryStream ms = new();
        Document document = new(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, ms);
        document.SetMargins((float)HORIZONTAL_MARGIN, (float)HORIZONTAL_MARGIN, (float)VERTICAL_MARGIN, (float)VERTICAL_MARGIN);
        document.Open();

        iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        var allLabels = new List<(string text, int remaining)>();
        foreach (var label in labels)
        {
            allLabels.Add((label.Text, label.Quantity));
        }

        int currentLabelIndex = 0;
        int labelsOnCurrentPage = 0;

        while (allLabels.Any(l => l.remaining > 0))
        {
            PdfPTable table = new(3) { WidthPercentage = 100 };
            table.SetWidths([(float)LABEL_WIDTH, (float)LABEL_WIDTH, (float)LABEL_WIDTH]);

            while (labelsOnCurrentPage < LABELS_PER_PAGE && allLabels.Any(l => l.remaining > 0))
            {
                var currentLabel = allLabels[currentLabelIndex];

                Paragraph paragraph = new()
                {
                    Alignment = Element.ALIGN_CENTER
                };

                string[] lines = currentLabel.text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                    {
                        paragraph.Add(new Chunk("\n", font));
                    }
                    paragraph.Add(new Chunk(lines[i], font));
                }

                PdfPCell cell = new(paragraph)
                {
                    FixedHeight = (float)LABEL_HEIGHT,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BorderWidth = 1,
                    Padding = (float)SAFETY_MARGIN
                };
                table.AddCell(cell);

                labelsOnCurrentPage++;
                allLabels[currentLabelIndex] = (currentLabel.text, currentLabel.remaining - 1);

                if (allLabels[currentLabelIndex].remaining == 0)
                {
                    currentLabelIndex = (currentLabelIndex + 1) % allLabels.Count;
                }
            }

            while (labelsOnCurrentPage < LABELS_PER_PAGE)
            {
                PdfPCell emptyCell = new(new Phrase(""))
                {
                    FixedHeight = (float)LABEL_HEIGHT,
                    BorderWidth = 0,
                    Padding = (float)SAFETY_MARGIN
                };
                table.AddCell(emptyCell);
                labelsOnCurrentPage++;
            }

            document.Add(table);

            if (allLabels.Any(l => l.remaining > 0))
            {
                document.NewPage();
                labelsOnCurrentPage = 0;
            }
        }

        document.Close();
        return ms.ToArray();
    }
}
