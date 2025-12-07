using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}