using iTextSharp.text;
using iTextSharp.text.pdf;
using Criacao.Etiqueta.Windows.Models;

namespace Criacao.Etiqueta.Windows.Services;

public class LabelService
{
    private const int LABELS_PER_PAGE = 65; // 13 linhas × 5 colunas  
    private const float SAFETY_MARGIN = 0.3f * 28.35f; // 0,3 cm para pontos  

    public static byte[] GenerateLabelsPdfA4251(List<LabelInfo> labels)
    {
        using MemoryStream ms = new();
        Document document = new(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, ms);
        float horizontalMargin = 0.3275f * 28.35f;
        float verticalMargin = 1.07f * 28.35f;
        document.SetMargins(horizontalMargin, horizontalMargin, verticalMargin, verticalMargin);
        document.Open();

        iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        float labelWidth = 4.069f * 28.35f;
        float labelHeight = 2.12f * 28.35f;

        // Criar uma lista de todas as etiquetas a serem impressas  
        var allLabels = new List<(string text, int remaining)>();
        foreach (var label in labels)
        {
            allLabels.Add((label.Text, label.Quantity));
        }

        int currentLabelIndex = 0;
        int labelsOnCurrentPage = 0;

        while (allLabels.Any(l => l.remaining > 0))
        {
            PdfPTable table = new(5) { WidthPercentage = 100 };
            table.SetWidths([labelWidth, labelWidth, labelWidth, labelWidth, labelWidth]);

            // Preencher a página atual  
            while (labelsOnCurrentPage < LABELS_PER_PAGE && allLabels.Any(l => l.remaining > 0))
            {
                var currentLabel = allLabels[currentLabelIndex];

                // Criar um parágrafo para suportar quebras de linha  
                Paragraph paragraph = new Paragraph();
                paragraph.Alignment = Element.ALIGN_CENTER;

                // Dividir o texto em linhas e adicionar cada linha como um novo Chunk  
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
                    FixedHeight = labelHeight,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BorderWidth = 0,
                    Padding = SAFETY_MARGIN
                };
                table.AddCell(cell);

                // Atualizar contadores  
                labelsOnCurrentPage++;
                allLabels[currentLabelIndex] = (currentLabel.text, currentLabel.remaining - 1);

                // Mover para o próximo texto se necessário  
                if (allLabels[currentLabelIndex].remaining == 0)
                {
                    currentLabelIndex = (currentLabelIndex + 1) % allLabels.Count;
                }
            }

            // Preencher células vazias na última página se necessário  
            while (labelsOnCurrentPage < LABELS_PER_PAGE)
            {
                PdfPCell emptyCell = new(new Phrase(""))
                {
                    FixedHeight = labelHeight,
                    BorderWidth = 0,
                    Padding = SAFETY_MARGIN
                };
                table.AddCell(emptyCell);
                labelsOnCurrentPage++;
            }

            document.Add(table);

            // Se ainda houver etiquetas para imprimir, criar nova página  
            if (allLabels.Any(l => l.remaining > 0))
            {
                document.NewPage();
                labelsOnCurrentPage = 0;
            }
        }

        document.Close();
        return ms.ToArray();
    }
    public static byte[] GenerateLabelsPdfA4256(List<LabelInfo> labels)
    {
        const int LABELS_PER_PAGE_A4256 = 33; // 11 linhas × 3 colunas
        const float LABEL_WIDTH_A4256 = 63.5f / 10f * 28.35f; // 63,5 mm para pontos
        const float LABEL_HEIGHT_A4256 = 25.4f / 10f * 28.35f; // 25,4 mm para pontos
        const float HORIZONTAL_MARGIN_A4256 = 7.5f / 10f * 28.35f; // 7,5 mm para pontos
        const float VERTICAL_MARGIN_A4256 = 13.5f / 10f * 28.35f; // 13,5 mm para pontos

        using MemoryStream ms = new();
        Document document = new(PageSize.A4);
        PdfWriter writer = PdfWriter.GetInstance(document, ms);
        document.SetMargins(HORIZONTAL_MARGIN_A4256, HORIZONTAL_MARGIN_A4256, VERTICAL_MARGIN_A4256, VERTICAL_MARGIN_A4256);
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
            table.SetWidths([LABEL_WIDTH_A4256, LABEL_WIDTH_A4256, LABEL_WIDTH_A4256]);

            while (labelsOnCurrentPage < LABELS_PER_PAGE_A4256 && allLabels.Any(l => l.remaining > 0))
            {
                var currentLabel = allLabels[currentLabelIndex];

                Paragraph paragraph = new Paragraph();
                paragraph.Alignment = Element.ALIGN_CENTER;

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
                    FixedHeight = LABEL_HEIGHT_A4256,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BorderWidth = 0,
                    Padding = SAFETY_MARGIN
                };
                table.AddCell(cell);

                labelsOnCurrentPage++;
                allLabels[currentLabelIndex] = (currentLabel.text, currentLabel.remaining - 1);

                if (allLabels[currentLabelIndex].remaining == 0)
                {
                    currentLabelIndex = (currentLabelIndex + 1) % allLabels.Count;
                }
            }

            while (labelsOnCurrentPage < LABELS_PER_PAGE_A4256)
            {
                PdfPCell emptyCell = new(new Phrase(""))
                {
                    FixedHeight = LABEL_HEIGHT_A4256,
                    BorderWidth = 0,
                    Padding = SAFETY_MARGIN
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
