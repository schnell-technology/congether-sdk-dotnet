using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Congether.SDK.DotNet
{
    internal partial class Client
    {
        protected static String GetHMACSHA256(Stream content, String key)
        {
            Byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            HMACSHA256 hash = new HMACSHA256(keyBytes);
            Byte[] hashBytes = hash.ComputeHash(content);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        protected static String GetHMACSHA256(byte[] content, String key)
        {
            Byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            HMACSHA256 hash = new HMACSHA256(keyBytes);
            Byte[] hashBytes = hash.ComputeHash(content);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    /*
     * 
     *  
        //SIGN CONTENT
        if (x_Congether_SIGN == null)
            request_.Headers.TryAddWithoutValidation("X-Congether-SIGN", ConvertToString(GetHMACSHA256(await content_.ReadAsStreamAsync(), _apiSecret), System.Globalization.CultureInfo.InvariantCulture));
        //END SIGN CONTENT

    */
}
