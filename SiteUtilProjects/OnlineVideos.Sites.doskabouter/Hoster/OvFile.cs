﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OnlineVideos.Hoster.Base;
using OnlineVideos.Sites;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;

namespace OnlineVideos.Hoster
{
    public class OvFile : HosterBase
    {
        public override string getHosterUrl()
        {
            return "ovfile.com";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            if (url.Contains("embed"))
            {
                string page = SiteUtilBase.GetWebData(url);
                if (!string.IsNullOrEmpty(page))
                {
                    Match n = Regex.Match(page, @"'file=(?<url>[^']+)'");
                    if (n.Success)
                    {
                        videoType = VideoType.flv;
                        return n.Groups["url"].Value;
                    }
                }
            }
            else
            {
                string page = SiteUtilBase.GetWebData(url, cc);

                if (!string.IsNullOrEmpty(page))
                {

                    string op = Regex.Match(page, @"<input\stype=""hidden""\sname=""op""\svalue=""(?<value>[^""]+)"">").Groups["value"].Value;
                    string usrlogin = Regex.Match(page, @"<input\stype=""hidden""\sname=""usr_login""\svalue=""(?<value>[^""]+)"">").Groups["value"].Value;
                    string id = Regex.Match(page, @"<input\stype=""hidden""\sname=""id""\svalue=""(?<value>[^""]+)"">").Groups["value"].Value;
                    string fname = Regex.Match(page, @"<input\stype=""hidden""\sname=""fname""\svalue=""(?<value>[^""]+)"">").Groups["value"].Value;
                    string referer = Regex.Match(page, @"<input\stype=""hidden""\sname=""referer""\svalue=""(?<value>[^""]+)"">").Groups["value"].Value;

                    string timeToWait = Regex.Match(page, @"<script>\s*var\stminus\s*=\s*(?<time>[^;]*);").Groups["time"].Value;
                    if (Convert.ToInt32(timeToWait) < 10)
                    {
                        string postdata = "op=" + op +
                                          "&usr_login=" + usrlogin +
                                          "&id=" + id +
                                          "&fname=" + fname +
                                          "&referer=" + HttpUtility.UrlEncode(referer) +
                                          "&method_free=Continue+to+file";

                        System.Threading.Thread.Sleep(Convert.ToInt32(timeToWait) * 1001);

                        string page2 = SiteUtilBase.GetWebDataFromPost(url, postdata, cc, url);

                        if (!string.IsNullOrEmpty(page2))
                        {
                            string packed = null;
                            int i = page2.LastIndexOf(@"return p}");
                            if (i >= 0)
                            {
                                int j = page2.IndexOf(@"</script>", i);
                                if (j >= 0)
                                    packed = page2.Substring(i + 9, j - i - 9);
                            }
                            string resUrl;
                            if (!String.IsNullOrEmpty(packed))
                            {
                                packed = packed.Replace(@"\'", @"'");
                                string unpacked = UnPack(packed);
                                videoType = VideoType.unknown;
                                string res = GetSubString(unpacked, @"'file','", @"'");
                                if (!String.IsNullOrEmpty(res))
                                    resUrl = res;
                                else
                                    resUrl = GetSubString(unpacked, @"name=""src""value=""", @"""");
                            }
                            else
                                resUrl = GetSubString(page2, @"addVariable('file','", @"'");
                            return resUrl;
                        }
                    }
                }
            }
            return String.Empty;
        }
    }
}