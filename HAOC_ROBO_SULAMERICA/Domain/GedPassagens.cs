using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAOC_ROBO_SULAMERICA.Domain
{
    public class GedPassagens
    {
        public decimal PAS_IDPASSAGEM { get; set; }
        public decimal PAS_IDUNIDADE { get; set; }
        public decimal PAS_IDCONVENIO { get; set; }
        public Nullable<System.DateTime> PAS_DATAHORAPASSAGEM { get; set; }
        public string PAS_CODIGOPASSAGEM { get; set; }
        public Nullable<decimal> PAS_FLAGCLIENTEPF { get; set; }
        public string PAS_REGISTRO { get; set; }
        public Nullable<System.DateTime> PAS_DATAHORAPASSAGEMFIM { get; set; }
        public string PAS_TIPOATENDIMENTO { get; set; }

    }
}
