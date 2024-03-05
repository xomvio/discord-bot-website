using Bitguard.DiscordRazor;
using Microsoft.AspNetCore.Diagnostics;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Dynamic;

namespace Bitguard.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ShownError = "";
        public string Error = "";
        public string Message = "";
        public string Path = "";
        public string Endpoint = "";
        public ErrorViewModel(string xshownError)
        {
            ShownError = PageActions.StatusErrorMsg(xshownError);
        }
        public ErrorViewModel(IConfiguration config, IExceptionHandlerPathFeature handler)
        {
            ShownError = "500: Internal Server Error (Reported).";
            Error = handler.Error.ToString();
            Message = handler.Error.Message;
            Path = handler.Path;
            Endpoint = handler.Endpoint?.ToString() ?? "none";
            DbActions db = new DbActions(config);
            db.AddErrorLog(handler);
        }
    }
}