using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker.Models
{
    public class EmployerFacetData
    {
        public string InteractionId { get; set; }

        public string ContactId { get; set; }

        public string SubTitle { get; set; }

        public string Company { get; set; }

        public string Consent { get; set; }

        public string PreferredOfficeLocation { get; set; }
    }
}
