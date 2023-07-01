using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Crypto;
using System.Diagnostics;
using System.Text.Json;

namespace GeradorDeRelatorio
{
    class Program
    {
        static List<Pessoa> pessoas = new List<Pessoa>();
        static BaseFont fonteBase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        static void Main(string[] args)
        {
            DesserializarPessoas();
            GerarRelatorioEmPdf(5);
        }

        static void DesserializarPessoas()
        {
            if (File.Exists("pessoas.json"))
            {
                using (var sr = new StreamReader("pessoas.json"))
                {
                    var dados = sr.ReadToEnd();
                    pessoas = JsonSerializer.Deserialize(dados, typeof(List<Pessoa>)) as List<Pessoa>;

                }
            }
        }

        static void GerarRelatorioEmPdf(int qtdPessoas)
        {
            var pessoasSelecionadas = pessoas.Take(qtdPessoas).ToList();
            if (pessoasSelecionadas.Count > 0)
            {
                //configuração do Documento PDF
                var pxPorMm = 72 / 25.2F;
                var pdf = new Document(PageSize.A4, 15 * pxPorMm, 15 * pxPorMm, 15 * pxPorMm, 20 * pxPorMm);
                var nomeArquivo = $"pessoas.{DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")}.pdf";
                var arquivo = new FileStream(nomeArquivo, FileMode.Create);
                var writer = PdfWriter.GetInstance(pdf, arquivo);
                writer.PageEvent = new EventosDePagina();
                pdf.Open();




                //adição de titulo
                var fonteParagrafo = new iTextSharp.text.Font(fonteBase, 24,
                    iTextSharp.text.Font.NORMAL, BaseColor.Black);
                var titulo = new Paragraph("Relatório de Pessoas\n\n", fonteParagrafo);
                titulo.Alignment = Element.ALIGN_LEFT;
                titulo.SpacingAfter = 4;
                pdf.Add(titulo);

                var caminhoImagem = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img\\logo.png");
                if (File.Exists(caminhoImagem))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(caminhoImagem);
                    float razaoAlturaLargura = logo.Width / logo.Height;
                    float alturaLogo = 38;
                    float larguraLogo = alturaLogo * razaoAlturaLargura;
                    logo.ScaleToFit(larguraLogo, alturaLogo);
                    var margemEsquerda = pdf.PageSize.Width - pdf.RightMargin - larguraLogo;
                    var margemTopo = pdf.PageSize.Height - pdf.TopMargin - 54;
                    logo.SetAbsolutePosition(margemEsquerda, margemTopo);
                    writer.DirectContent.AddImage(logo, false);
                }

                //adição da tabela de dados
                var tabela = new PdfPTable(5);
                float[] largurasColunas = { 0.6f, 2f, 1.5f, 1f, 1f };
                tabela.SetWidths(largurasColunas);
                tabela.DefaultCell.BorderWidth = 0;
                tabela.WidthPercentage = 100;

                // adição das celulas de titulos das colunas
                CriarCelulaTexto(tabela, "Codigo", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Nome", PdfCell.ALIGN_LEFT, true);
                CriarCelulaTexto(tabela, "Profissão", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Salario", PdfCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Empregada", PdfCell.ALIGN_CENTER, true);

                foreach (var p in pessoasSelecionadas)
                {
                    CriarCelulaTexto(tabela, p.IdPessoa.ToString("D6"), PdfCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, p.Nome + " " + p.Sobrenome);
                    CriarCelulaTexto(tabela, p.Profissao.Nome, PdfCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, p.Salario.ToString("C2"), PdfCell.ALIGN_RIGHT);
                    CriarCelulaTexto(tabela, p.Empregado ? "Sim" : "Não", PdfCell.ALIGN_CENTER);
                }

                pdf.Add(tabela);

                pdf.Close();
                arquivo.Close();

                //abre o PDF no visualizador padrão
                var caminhoPDF = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomeArquivo);
                if (File.Exists(caminhoPDF))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments = $"/c start {caminhoPDF}",
                        FileName = "cmd.exe",
                        CreateNoWindow = true
                    });

                }

            }

        }

        static void CriarCelulaTexto(PdfPTable tabela, string texto, 
            int alinhamentoHorz = PdfPCell.ALIGN_LEFT,
            bool negrito = false, bool italico = false,
            int tamanhoFonte = 12, int alturaCelula = 25)
        {
            int estilo = iTextSharp.text.Font.NORMAL;
            if (negrito && italico)
            {
                estilo = iTextSharp.text.Font.BOLDITALIC;
            }
            else if (negrito)
            {
                estilo = iTextSharp.text.Font.BOLD;
            }
            else if (italico)
            {
                estilo = iTextSharp.text.Font.ITALIC;
            }           
            var fontCelula = new iTextSharp.text.Font(fonteBase, 12,
                iTextSharp.text.Font.NORMAL, BaseColor.Black);

            var bgColor = iTextSharp.text.BaseColor.White;
            if(tabela.Rows.Count % 2 == 1)
            {
                bgColor = new BaseColor(0.95F, 0.95F, 0.95F);
            }

            var celula = new PdfPCell(new Phrase(texto, fontCelula));
            celula.HorizontalAlignment = alinhamentoHorz;
            celula.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            celula.Border = 0;
            celula.BorderWidthBottom = 1;
            celula.FixedHeight = alturaCelula;
            celula.PaddingBottom = 5;
            celula.BackgroundColor = bgColor;
            tabela.AddCell(celula);
        }
    }
}