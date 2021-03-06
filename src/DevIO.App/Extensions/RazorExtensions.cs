using Microsoft.AspNetCore.Mvc.Razor;
using System;

namespace DevIO.App.Extensions
{
    public static class RazorExtensions
    {
        public static string FormatarDocumento(this RazorPage page, int tipopessoa, string documento)
        {
            return tipopessoa == 1 ? Convert.ToUInt64(documento).ToString(@"000\.000\.000\-00") : Convert.ToUInt64(documento).ToString(@"00\.000\/0000\-00");
        }
    }
}
