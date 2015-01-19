﻿using MultiMiner.Xgminer.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MultiMiner.UX.Extensions
{
    public static class StringExtensions
    {
        public static bool VersionIsGreater(this string thisVersion, string thatVersion)
        {
            Version thisVersionObj = new Version(thisVersion);
            Version thatVersionObj = new Version(thatVersion);

            return thisVersionObj > thatVersionObj;
        }

        private readonly static Dictionary<string, string> hostDomainNames = new Dictionary<string, string>();

        public static string DomainFromHost(this string host)
        {
            if (String.IsNullOrEmpty(host))
                return String.Empty;

            if (hostDomainNames.ContainsKey(host))
                return hostDomainNames[host];

            string domainName = host.Trim();

            if (!host.Contains(":"))
                host = "http://" + host;

            try
            {
                Uri uri = new Uri(host);

                domainName = uri.Host;

                if (uri.HostNameType == UriHostNameType.Dns)
                {
                    // remove subdomain if there is one
                    if (domainName.Split('.').Length > 2)
                    {
                        int index = domainName.IndexOf(".") + 1;
                        domainName = domainName.Substring(index, domainName.Length - index);
                    }

                    // remove TLD
                    if (domainName.Length > 7)
                        domainName = Path.GetFileNameWithoutExtension(domainName);
                }
            }
            catch (UriFormatException)
            {
                // System.UriFormatException: Invalid URI: The hostname could not be parsed.
                // don't crash - fall back on domainName = host (initialized above)
            }

            hostDomainNames[host] = domainName;

            return domainName;
        }

        public static string ShortHostFromHost(this string host)
        {
            return host.Replace("http://", "").Replace("stratum+tcp://", "").TrimEnd('/');
        }

        private static string StripLowerChars(this string text)
        {
            return new String(text.Where(c => Char.IsNumber(c) || Char.IsUpper(c)).ToArray());
        }

        public static string FitCurrency(this string amount, int totalWidth)
        {
            var result = amount.PadCurrency();
            if (result.Length > totalWidth) result = result.Substring(0, totalWidth);
            return result;
        }

        public static string PadCurrency(this string amount)
        {
            if (String.IsNullOrEmpty(amount)) return String.Empty;

            var result = amount;

            var symbol = amount[0];
            var parts = amount.Remove(0, 1).Split(new string[] { CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator }, StringSplitOptions.None);

            result = symbol + parts[0].PadLeft(4) + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + parts[1].PadRight(3);

            return result;
        }

        public static string ShortCoinSymbol(this string coinSymbol)
        {
            //may be BTC or, for MultiCoin groups, NiceHash:X11, NiceHash:NeoScrypt, Other:SHA256
            string result = coinSymbol;
            const char Colon = ':';
            if (result.Contains(Colon))
            {
                var parts = coinSymbol.Split(Colon);
                result = String.Format("{0}:{1}", parts[0].StripLowerChars(), parts[1].StripLowerChars());
                //get SHA256 shorter
                result = result.Replace(AlgorithmNames.SHA256, "SHA2");
            }
            return result;
        }

        public static string EllipsisString(this string text, int totalWidth, string ellipsis)
        {
            var result = text;
            
            if (text.Length > totalWidth)
            {
                var partLen = (totalWidth - ellipsis.Length) / 2;
                result = String.Format("{0}{1}{2}",
                    text.Substring(0, partLen),
                    ellipsis,
                    text.Substring(text.Length - partLen));
            }

            return result;
        }

        public static string FitLeft(this string text, int totalWidth, string ellipsis)
        {
            return text.EllipsisString(totalWidth, ellipsis).PadLeft(totalWidth);
        }

        public static string FitRight(this string text, int totalWidth, string ellipsis)
        {
            return text.EllipsisString(totalWidth, ellipsis).PadRight(totalWidth);
        }

        public static string PadFitRight(this string text, int totalWidth, string ellipsis)
        {
            return text.EllipsisString(totalWidth - 1, ellipsis).PadRight(totalWidth);
        }

        public static string PadFitLeft(this string text, int totalWidth, string ellipsis)
        {
            return text.EllipsisString(totalWidth - 1, ellipsis).PadLeft(totalWidth);
        }

        public static bool ParseHostAndPort(this string hostAndPort, out string host, out int port)
        {
            const char Separator = ':';
            host = String.Empty;
            port = 0;

            if (hostAndPort.Contains(Separator))
            {
                string[] parts = hostAndPort.Split(Separator);
                int newPort = 0;

                if (Int32.TryParse(parts.Last(), out newPort))
                {
                    string newHost = String.Empty;

                    //loop through all but last (- 1)
                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        if (!String.IsNullOrEmpty(newHost))
                            newHost = newHost + Separator;
                        newHost = newHost + parts[i];
                    }

                    host = newHost;
                    port = newPort;
                    
                    return true;
                }
            }

            return false;
        }

    }
}
