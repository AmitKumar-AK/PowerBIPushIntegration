using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker.Models
{
    public class PersonalInformation
    {
        public string InteractionId { get; set; }

        public string ContactId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }
    }
}
