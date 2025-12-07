using SafeTalkApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class DateTimeProviderService : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}