using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SafeTalkApp.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly string _clientId;
        private readonly string _secret;
        private readonly string _mode;

        public PayPalService()
        {
            _clientId = ConfigurationManager.AppSettings["PayPalClientID"];
            _secret = ConfigurationManager.AppSettings["PayPalSecret"];
            _mode = ConfigurationManager.AppSettings["PayPalMode"]; // sandbox or live
        }

        private string GetBaseUrl() => _mode == "sandbox" ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

        private string GetAccessToken()
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Headers[HttpRequestHeader.Authorization] = "Basic " +
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_secret}"));

                var bytes = client.UploadData(GetBaseUrl() + "/v1/oauth2/token", "POST",
                    Encoding.UTF8.GetBytes("grant_type=client_credentials"));

                var result = JObject.Parse(Encoding.UTF8.GetString(bytes));
                return result["access_token"].ToString();
            }
        }

        public JObject CreateOrder(decimal amount, string returnUrl, string cancelUrl, int appointmentID)
        {
            var token = GetAccessToken();
            var order = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                        new {
                            amount = new { currency_code = "PHP", value = amount.ToString("F2") },
                            reference_id = appointmentID.ToString() // Use appointment ID as reference
                        }
                    },
                application_context = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl,
                    user_action = "CONTINUE"
                }
            };

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

                var response = client.UploadString(GetBaseUrl() + "/v2/checkout/orders", "POST",
                    JsonConvert.SerializeObject(order));

                return JObject.Parse(response);
            }
        }

        public JObject CaptureOrder(string orderId)
        {
            var token = GetAccessToken();
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

                var response = client.UploadString(GetBaseUrl() + $"/v2/checkout/orders/{orderId}/capture", "POST", "");
                return JObject.Parse(response);
            }
        }

    }
}