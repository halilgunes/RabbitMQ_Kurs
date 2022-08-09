using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Hubs
{
    /// <summary>
    /// SignalIR haberleşmesi burada oluyor.
    /// Client'a bilgi göndereceğiz.
    /// Normalde clienttan bilgi alıp clienta bilgi gönderme işlemi yapılıyor.
    /// </summary>
    public class MyHub : Hub
    {
    }
}
