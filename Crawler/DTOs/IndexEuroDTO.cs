using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerEuro
{
    public class IndexEuroDTO
    {
        public IList<IndexQuoteDTO> Value { get; set; }
    }

    public class IndexQuoteDTO
    {
        public double cotacaoVenda { get; set; }
        public DateTime dataHoraCotacao { get; set; }
        public string tipoBoletim { get; set; }
    }
}
