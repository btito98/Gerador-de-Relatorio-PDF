using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeradorDeRelatorio
{
    public class EventosDePagina : PdfPageEventHelper
    {
        public BaseFont fonteBaseRodape {get; set;}
        public iTextSharp.text.Font fonteRodape { get; set; }

        public EventosDePagina() 
        {
            fonteBaseRodape = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            fonteRodape = new iTextSharp.text.Font(fonteBaseRodape, 8f,
                iTextSharp.text.Font.NORMAL, BaseColor.Black);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            var textoMomentoGeracao = $"Gerado em {DateTime.Now.ToShortDateString()} às " +
                $"{DateTime.Now.ToShortTimeString()}";

            writer.DirectContent.BeginText();
            writer.DirectContent.SetFontAndSize(fonteRodape.BaseFont, fonteRodape.Size);
            writer.DirectContent.SetTextMatrix(document.LeftMargin,
                document.BottomMargin * 0.75F);
            writer.DirectContent.ShowText(textoMomentoGeracao);
            writer.DirectContent.EndText();
        }
    }
}
