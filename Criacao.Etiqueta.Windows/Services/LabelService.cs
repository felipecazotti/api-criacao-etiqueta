using iTextSharp.text;
using iTextSharp.text.pdf;
using Criacao.Etiqueta.Windows.Models;

namespace Criacao.Etiqueta.Windows.Services;

public class LabelService
{
    private const int LABELS_PER_PAGE = 65; // 13 linhas × 5 colunas
    private const float SAFETY_MARGIN = 0.3f * 28.35f; // 0,3 cm para pontos

    public byte[] GenerateLabelsPdf(List<LabelInfo> labels)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            Document document = new Document(PageSize.A4);
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
                PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
                table.SetWidths(new float[] { labelWidth, labelWidth, labelWidth, labelWidth, labelWidth });

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

                    PdfPCell cell = new PdfPCell(paragraph)
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
                    PdfPCell emptyCell = new PdfPCell(new Phrase(""))
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
    }
} 