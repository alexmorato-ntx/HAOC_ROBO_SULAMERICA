using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAOC_ROBO_SULAMERICA.Domain
{
    class RequestAtendimentoURI
    {
        public string MedicalCareCode { get; set; }
		public string MedicalCareType { get; set; }
		public string URI { get; set; }
		public string Error { get; set; }
    }
}
