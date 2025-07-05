using Criacao.Etiqueta.Windows.AbstractServices;
using Criacao.Etiqueta.Windows.Models;
using Criacao.Etiqueta.Windows.Services;
using System.Diagnostics;

namespace Criacao.Etiqueta.Windows;

public partial class Form1 : Form
{
    private List<LabelInfo> labels = new();
    private readonly Dictionary<string, LabelAbstractService> dictLabelService;
    private DataGridView dataGridView;
    private TextBox textBox;
    private NumericUpDown quantityBox;
    private string? tempPdfPath;

    public Form1()
    {
        InitializeComponent();
        dictLabelService = new Dictionary<string, LabelAbstractService>
        {
            { LabelAbstractService.A4251, new LabelServiceA4251() },
            { LabelAbstractService.A4256, new LabelServiceA4256() }
        };
        InitializeCustomComponents();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        
        // Limpar arquivo temporário ao fechar o formulário
        if (tempPdfPath != null && File.Exists(tempPdfPath))
        {
            try
            {
                File.Delete(tempPdfPath);
            }
            catch { } // Ignora erros ao tentar deletar
        }
    }

    private void InitializeCustomComponents()
    {
        this.Text = "Gerador de Etiquetas A4251";
        this.Size = new Size(800, 600);

        // Painel principal
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            RowCount = 2,
            ColumnCount = 1
        };

        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Grupo para adicionar etiqueta
        var addGroup = new GroupBox
        {
            Text = "Adicionar Etiqueta",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        // Campo de texto
        var textLabel = new Label
        {
            Text = "Texto da Etiqueta:",
            Location = new Point(10, 25),
            AutoSize = true
        };

        textBox = new TextBox
        {
            Multiline = true,
            Location = new Point(10, 50),
            Size = new Size(740, 80),
            ScrollBars = ScrollBars.Vertical
        };

        // Campo de quantidade
        var quantityLabel = new Label
        {
            Text = "Quantidade:",
            Location = new Point(10, 140),
            AutoSize = true
        };

        quantityBox = new NumericUpDown
        {
            Location = new Point(100, 138),
            Minimum = 1,
            Maximum = 1000,
            Value = 1,
            Width = 100
        };

        // ComboBox para seleção do modelo
        var modelLabel = new Label
        {
            Text = "Modelo:",
            Location = new Point(220, 140),
            AutoSize = true
        };

        var modelComboBox = new ComboBox
        {
            Location = new Point(280, 137),
            Width = 100,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        modelComboBox.Items.AddRange(new string[] { "A4251", "A4256" });
        modelComboBox.SelectedIndex = 0;

        // Botão adicionar
        var addButton = new Button
        {
            Text = "Adicionar Etiqueta",
            Location = new Point(390, 137),
            Width = 150
        };

        // Lista de etiquetas
        var listGroup = new GroupBox
        {
            Text = "Etiquetas",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        dataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };

        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Text",
            HeaderText = "Texto",
            DataPropertyName = "Text",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        dataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Quantity",
            HeaderText = "Quantidade",
            DataPropertyName = "Quantity",
            Width = 100
        });

        // Botão remover
        var removeButton = new Button
        {
            Text = "Remover Selecionada",
            Dock = DockStyle.Bottom,
            Height = 40,
            Margin = new Padding(0, 10, 0, 0),
            Enabled = false
        };

        // Botão gerar PDF
        var generateButton = new Button
        {
            Text = "Gerar PDF",
            Dock = DockStyle.Bottom,
            Height = 40,
            Margin = new Padding(0, 10, 0, 0)
        };

        // Eventos
        addButton.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show("Por favor, digite um texto para a etiqueta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var labelInfo = new LabelInfo
            {
                Text = textBox.Text,
                Quantity = (int)quantityBox.Value
            };

            labels.Add(labelInfo);
            UpdateDataGridView();

            textBox.Clear();
            quantityBox.Value = 1;
        };

        dataGridView.SelectionChanged += (s, e) =>
        {
            removeButton.Enabled = dataGridView.SelectedRows.Count > 0;
        };

        removeButton.Click += (s, e) =>
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var index = dataGridView.SelectedRows[0].Index;
                labels.RemoveAt(index);
                UpdateDataGridView();

                if (dataGridView.Rows.Count > 0)
                {
                    dataGridView.Rows[Math.Min(index, dataGridView.Rows.Count - 1)].Selected = true;
                }
                else
                {
                    removeButton.Enabled = false;
                }
            }
        };

        generateButton.Click += async (s, e) =>
        {
            if (labels.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos uma etiqueta antes de gerar o PDF.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] pdfBytes = null;
                if(dictLabelService.TryGetValue(modelComboBox.SelectedItem?.ToString(), out var labelService))
                {
                    pdfBytes = labelService.GenerateLabelsPdf(labels);
                }
                
                if (tempPdfPath != null && File.Exists(tempPdfPath))
                {
                    try
                    {
                        File.Delete(tempPdfPath);
                    }
                    catch { }
                }

                tempPdfPath = Path.Combine(Path.GetTempPath(), $"etiquetas_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                if(pdfBytes is not null)
                    await File.WriteAllBytesAsync(tempPdfPath, pdfBytes);

                // Abrir o PDF
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = tempPdfPath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Não foi possível abrir o PDF: {ex.Message}",
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar o PDF: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // Adicionar controles
        addGroup.Controls.AddRange(new Control[] { textLabel, textBox, quantityLabel, quantityBox, modelLabel, modelComboBox, addButton });
        listGroup.Controls.AddRange(new Control[] { dataGridView, removeButton });

        var bottomPanel = new Panel { Dock = DockStyle.Fill };
        bottomPanel.Controls.AddRange(new Control[] { listGroup, generateButton });

        mainPanel.Controls.Add(addGroup, 0, 0);
        mainPanel.Controls.Add(bottomPanel, 0, 1);

        this.Controls.Add(mainPanel);
    }

    private void UpdateDataGridView()
    {
        dataGridView.Rows.Clear();
        foreach (var label in labels)
        {
            dataGridView.Rows.Add(label.Text, label.Quantity);
        }
    }
}
