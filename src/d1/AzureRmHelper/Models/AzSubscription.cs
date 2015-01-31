using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRmHelper.Models
{
    public class AzSubscription
    {
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string AadTenantId { get; set; }

    }
}
