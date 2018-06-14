
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;
using System;
using System.Text.RegularExpressions;

namespace FunctionAppValidateCPFDocument
{
    public static class FunctionAppValidateCPFDocument
    {
        [FunctionName("FunctionAppValidateCPFDocument")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string cpf = req.Query["cpf"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            cpf = cpf ?? data?.cpf;

            return cpf != null
                ?// (ActionResult)new OkObjectResult($"Hello, {cpf}") 
                (ActionResult)new OkObjectResult(ValidateCPF(cpf))
                : new BadRequestObjectResult("Please pass a cpf on the query string or in the request body");
        }

        private static bool ValidateCPF(string cpf)
        {
            #region Pex

            if (string.IsNullOrEmpty(cpf))
            {
                return false;
            }

            // checks if the string is longer than 11 characters
            cpf = cpf.Replace(".", "").Replace("-", "").Replace("_", "");
            if (cpf.Length != 11)
            {
                return false;
            }

            // after removing invalid characters, checks to see if the new string is still longer than 11 characters
            cpf = ExtractNumbers(cpf);
            if (cpf.Length != 11)
            {
                return false;
            }

            #endregion Pex

            string s = cpf;
            string c = s.Substring(0, 9);
            string dv = s.Substring(9, 2);
            int d1 = 0;
            for (int i = 0; i < 9; i++)
            {
                d1 += Int32.Parse(c.Substring(i, 1)) * (10 - i);
            }

            if (d1 == 0) return false;
            d1 = 11 - (d1 % 11);
            if (d1 > 9) d1 = 0;
            if (Int32.Parse(dv.Substring(0, 1)) != d1)
            {
                return false;
            }
            d1 *= 2;
            for (int i = 0; i < 9; i++)
            {
                d1 += Int32.Parse(c.Substring(i, 1)) * (11 - i);
            }
            d1 = 11 - (d1 % 11);
            if (d1 > 9) d1 = 0;
            if (Int32.Parse(dv.Substring(1, 1)) != d1)
            {
                return false;
            }

            return true;
        }
        private static string ExtractNumbers(string original)
        {
            // removes special characters from the string
            var newString = TrataTexto(original);
            // remove only numbers
            return Regex.Replace(newString, "[^0-9]+", string.Empty);
        }
        private static string TrataTexto(string strText)
        {
            if (string.IsNullOrEmpty(strText))
            {
                return string.Empty;
            }

            strText = RemoveAcents(strText);
            strText = RemoveSpecialChars(strText);
            return strText;
        }
        private static string RemoveAcents(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return string.Empty;

            strText = strText.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in strText.ToCharArray())
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);

            return sb.ToString();
        }
        private static string RemoveSpecialChars(string str)
        {
            #region Pex

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            #endregion Pex

            String C_acents = "¡¿…»ÕÃ”“⁄Ÿ¬√’‘ Œ€·ÈÌÛ˙‡ËÏÚ˘„‚ÍÓÙ˚Á¡»Ù«·Ë“Á¬ÀÚ‚Îÿ—¿–¯Ò‡’≈ı›ÂÕ÷˝√Ìˆ„ŒƒÓ⁄‰Ã˙∆Ï€Êœ˚ÔŸÆ…˘©È”‹ﬁ Û¸˛Í‘";
            String S_acents = "AAEEIIOOUUAAOOEIUaeiouaeiouaaeioucAEoCaeOcAEoaeoNADonaoOAoYAIOyAioaUAiUaIuEiUeIuiUrEUceOUpEoupeO";

            for (int i = 0; i < C_acents.Length; i++)
                str = str.Replace(C_acents[i].ToString(), S_acents[i].ToString()).Trim();

            // remove \0 = null
            str = str.Replace("\0", string.Empty);

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            str = rgx.Replace(str, "");

            return str;
        }

    }
}
