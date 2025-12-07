using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IPayPalService
    {
        JObject CreateOrder(decimal amount, string returnUrl, string cancelUrl, int appointmentID);
        JObject CaptureOrder(string orderId);
    }
}