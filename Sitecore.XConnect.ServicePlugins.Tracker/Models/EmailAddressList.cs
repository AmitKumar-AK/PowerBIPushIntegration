using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker.Models
{
    public class EmailAddressList
    {
        public string Email { get; set; }
        public string InteractionId { get; set; }
        public string ContactId { get; set; }
    }
}
