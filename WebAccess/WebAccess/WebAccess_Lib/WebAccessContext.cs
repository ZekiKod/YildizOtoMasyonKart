/*
 * Author: İstanbul Yazılım Elektronik Sanayi
 */

using System;
using System.Collections.Specialized;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ASPNetCore_WebAccess
{
    public class ASPCoreContext
    {
        public HttpContext Context;

        public ASPCoreContext(HttpContext Context)
        {
            this.Context = Context;
        }

        public IQueryCollection? Query
        {
            get
            {
                try
                {
                    return Context.Request.Query;
                }
                catch
                {
                    return null;
                }
            }
        }

        public IFormCollection? Form
        {
            get
            {
                try
                {
                    return Context.Request.Form;
                }
                catch
                {
                    return null;
                }
            }
        }

        public string Param(string Name)
        {
            IQueryCollection? Q = Query;
            if (Q != null && Q.ContainsKey(Name))
            {
                return Q[Name].ToString();
            }
            else
            {
                IFormCollection? F = Form;
                if (F != null && F.ContainsKey(Name))
                {
                    return F[Name].ToString();
                }
            }

            return String.Empty;
        }

        public NameValueCollection? Params()
        {
            NameValueCollection RetVal = new NameValueCollection();

            if (Query != null)
            {
                string PrmName;
                List<string> PrmList = Query.Keys.ToList();

                for (int i = 0; i < PrmList.Count; i++)
                {
                    PrmName = PrmList[i];
                    if (PrmName != null)
                        RetVal.Add(PrmName, Param(PrmName));
                }
            }

            /*
            if (Form != null)
            {
                string PrmName;
                List<string> PrmList = Form.Keys.ToList();

                for (int i = 0; i < PrmList.Count; i++)
                {
                    PrmName = PrmList[i];
                    if (PrmName != null)
                        RetVal.Add(PrmName, Param(PrmName));
                }
            }
            */

            return RetVal;
        }

        public bool ParamExists(string Name)
        {
            IQueryCollection? Q = Query;
            if (Q != null && Q.ContainsKey(Name))
            {
                return true;
            }
            else
            {
                IFormCollection? F = Form;
                if (F != null && F.ContainsKey(Name))
                {
                    return true;
                }
            }

            return false;
        }

        public string HttpMethod
        {
            get { return Context.Request.Method; }
        }

        public string Response_ContentType
        {
            get { return Context.Response.ContentType; }
            set { Context.Response.ContentType = value; }

        }

        public void Response(string Value)
        {
            Context.Response.Clear();
            // Context.Response.ContentType = "text/file; charset=ISO-8859-9";

            /* Sistemde kullanılan kod sayfalarını listeler.

            string S = "";

            EncodingInfo[] e = Encoding.GetEncodings();

            for (int i = 0; i < e.Length; i++)
            {
                S += "CodePage: " + e[i].CodePage.ToString() + ",  DisplayName: " + e[i].DisplayName + "<br>";
            }

            byte[] Encodings_Bytes = System.Text.Encoding.Default.GetBytes(S);
            Context.Response.Body.Write(Encodings_Bytes);
            */

            if (Value.Length > 0)
            {
                // byte[] Value_Bytes = System.Text.Encoding.GetEncoding(1254).GetBytes(Value);
                // byte[] Value_Bytes = System.Text.Encoding.GetEncoding(857).GetBytes(Value);

                // Context.Response.Body.Write(System.Text.Encoding.Default.GetBytes(Value));

                Context.Response.Body.Write(System.Text.Encoding.UTF8.GetBytes(Value));
            }
        }
    }
}