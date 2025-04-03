using System.Text.Json;
using Azure.Core;

namespace Hao_Hao.Models
{
    public static class XacThucPhanQuyen
    {
        public static bool XacThuc(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("UserInfo", out string userInfoJson))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }

}


