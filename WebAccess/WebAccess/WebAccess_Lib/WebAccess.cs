/*
 * Author: İstanbul Yazılım Elektronik Sanayi
 */

// #define USE_CRYPTO_DLL

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Drawing;
using System.Text.Unicode;
using System.Text;

namespace ASPNetCore_WebAccess
{
    /* WebAccess yönetici sınıfı. */
    public class WebAccess
    {
        private string Response_Buffer = "";

        public WebAccess(string Password, bool Crypto_Enabled, HttpContext Context)
        {
            byte[]? Buf = ToBytes(Password);

#if USE_CRYPTO_DLL
            UInt64 P64 = 0;
            UInt64 Tmp = 0;

            Get_Password(Password, "", out P64, out Tmp);
            this.Password = P64; // Önceden cihaza verilmiş olan şifre.
            this.Crypto_Enable = Crypto_Enabled; // Bu alan true ise cihaza gönderilecek olan veriler şifrelenir.
#endif

            this.Context = new ASPCoreContext(Context);
            Params = new HttpParams();

            /* Cihazdan gelen verileri al. */

            // URL`deki "&" ile bir birinden ayrılmış olan kullanıcı parametrelerini al.
            Split_Params(null, false, Crypto_Enabled, true, false);


            if (this.Context.HttpMethod.ToUpper() == "POST") // POST Metoduyla işlem yapılıyorsa
            {
                if (this.Context.ParamExists("b_urlencoded")) // POST metodu ile kriptolu www_form_urlencoded işlemi yapılıyor ise
                {
                    byte[]? cb = new byte[0];
                    cb = ToBytes(this.Context.Param("b_urlencoded"));
                    Split_Params(cb, true, Crypto_Enabled, false, true);  // cb kriptolu olsa bile üst satırda kriptosu çözülmüş olduğundan kripto parametresini False geçiyoruz.
                }
                else
                if (this.Context.ParamExists("Session_Id")) // POST metodu ile kriptosuz www_form_urlencoded işlemi yapılıyor ise
                {
                    Split_Params(null, false, Crypto_Enabled, false, true); // URL`deki "&" ile bir birinden ayrılmış olan cihaz parametrelerini al.
                }
                else // Form-Data kullanılıyor.
                {
                    if (this.Context.ParamExists("b")) // Tüm parametreler tek bir form-data parametresi içinde gönderilmiş ise
                    {
                        byte[]? cb = new byte[0];
                        cb = ToBytes(Get_Form_Value(this.Context.Param("b")));
                        Split_Params(cb, true, false, false, true);  // cb kriptolu olsa bile üst satırda kriptosu çözülmüş olduğundan kripto parametresini False geçiyoruz.
                    }
                    else // Her parametre kendi ismiyle gönderilmiş.
                    if (this.Context.ParamExists("SA")) // Tüm parametreler tek bir form-data parametresi içinde gönderilmiş ise
                    {
                        Params.Session_Id = Convert.ToUInt64(Get_Form_Value(this.Context.Param("SA")));                           // Session_Id - Oturum id`si. Sunucu ile başlatılan her işlemde bir oturum id`si bulunur.
                        Params.Message_Type = (HttpMessageType)Convert.ToUInt32(Get_Form_Value(this.Context.Param("MT")));        // Message_Type - Mesaj türü. Sunucuya gönderilen her pakitn bir mesaj türü vardır.
                        Params.Completed_Type = (HttpCompletedType)Convert.ToUInt32(Get_Form_Value(this.Context.Param("CT")));    // Completed_Type - Completed komutunda kullanılır. Tamamlanan işlemin türünü bildirir.
                        Params.Process_Id = Convert.ToUInt64(Get_Form_Value(this.Context.Param("PI")));                           // Process_Id - Sunucu tarafından oluşturulan bir kod`dur. Completed mesajı ile yapılan işlemin durumu bildirilirken tekrar sunucuya gönderilir.
                        Params.SoftVer = Convert.ToUInt64(Get_Form_Value(this.Context.Param("SV")));                              // Cihazdaki yazılımın versiyonu.
                        Params.Multi_User = (Params.SoftVer > 1083);                                                              // Bu alan true ise cihaza toplu şekilde kullanıcı gönderilebilir.
                        Params.Device_Id = Convert.ToUInt32(Get_Form_Value(this.Context.Param("DI")));                            // Device_Id - Cihaz id`si. Sunucunun hangi cihaz ile çalıştığını anlaması için cihaz id gönderilir.
                        Params.Reader_Num = Convert.ToInt32(Get_Form_Value(this.Context.Param("RN")));                            // Reader_Num - İşlem yapılan okuyucu nosu.
                        Params.Wiegand_Num = Convert.ToInt32(Get_Form_Value(this.Context.Param("WN")));                           // İşlem yapılan okuyucunun, işlem yapılan wiegand nosu.
                        Params.Reader_Count = Convert.ToInt32(Get_Form_Value(this.Context.Param("RC")));                          // Reader_Count - Cihazın okuyucu portu sayısı.
                        Params.Card_Id = Convert.ToUInt64(Get_Form_Value(this.Context.Param("CI")));                              // Card_Id - Okutulan kartın bilgisi.
                        Params.Password = Convert.ToInt64(Get_Form_Value(this.Context.Param("PW")));                              // Password - Kullanıcının girmiş olduğu şifre.
                        Params.Registration_Number = Convert.ToInt64(Get_Form_Value(this.Context.Param("RG")));                   // Registration_Number - İşlem yapan kullanıcının kayıt/sicil nosu.
                        Params.Written_User_Count = Convert.ToUInt64(Get_Form_Value(this.Context.Param("WU")));                   // Written_User_Count - "User" komutu ile cihaza yazılan kullanıcı sayısı.
                        Params.Written_User_Forbidden_Count = Convert.ToUInt64(Get_Form_Value(this.Context.Param("FR")));         // Written_User_Forbidden_Count - "User_Forbidden" komutu ile cihaza yazılan kullanıcı sayısı.
                        Params.Written_User_Unforbidden_Count = Convert.ToUInt64(Get_Form_Value(this.Context.Param("UF")));       // Written_User_Unforbidden_Count - "User_Unforbidden" komutu ile cihaza yazılan kullanıcı sayısı.
                        Params.User_Count = Convert.ToUInt64(Get_Form_Value(this.Context.Param("UC")));                           // User_Count - Cihazdaki kullanıcı sayısı.
                        Params.Log_Count = Convert.ToUInt64(Get_Form_Value(this.Context.Param("LC")));                            // Log_Count - Cihazdaki log sayısı.
                        Params.Log_Event = (LogEvent)Convert.ToUInt32(Get_Form_Value(this.Context.Param("LE")));                  // Log_Event - Log türü.
                        Params.Log_Time = Convert_To_LogTime(Get_Form_Value(this.Context.Param("LT")));                           // Log_Time - İşlem zamanı.
                        Params.Tag_Type = (RFTagType)Convert.ToInt32(Get_Form_Value(this.Context.Param("TT")));                   // Tag_Type - Okutulan kartın türü. DESFire veya Mifare klasik..vb.
                        Params.Data = HttpUtility.UrlDecodeToBytes(Get_Form_Value(this.Context.Param("DT")));                     // Data - Gelen veri.
                        Params.Data_Count = Params.Data != null ? Params.Data.Length : 0;                                         // Data_Count - Data parametresi ile gelen verinin uzunluğu.
                        // Params.Data = ToBytes(HttpUtility.UrlDecode(Get_Form_Value(this.Context.Param("DT"))));                // Data - Gelen veri.
                        // Params.Data_Count = Convert.ToInt32(Get_Form_Value(this.Context.Param("DC")));                         // Data_Count - Data parametresi ile gelen verinin uzunluğu.


                        // Cihazın kullandığı ekran modeli.
                        Params.Screen.Model = (ScreenModel)Convert.ToUInt32(Get_Form_Value(this.Context.Param("SM")));            // Screen_Model - Cihazın kullandığı ekran modeli.
                        Params.Screen.Width = Convert.ToUInt32(Get_Form_Value(this.Context.Param("SW")));                         // Screen_Width - Ekran genişliği.
                        Params.Screen.Height = Convert.ToUInt32(Get_Form_Value(this.Context.Param("SH")));                        // Screen_Height - Ekran yüksekliği.

                        // Grup erişim bölgeleri.
                        Params.Access_Zones.Decode(Get_Form_Value(this.Context.Param("GA")));                                     // Group_Access_Zones

                        // Sensörlerin şu an ki sinyal verileri.
                        Params.Sensors.Unpack(Convert.ToUInt64(Get_Form_Value(this.Context.Param("SN"))), Params.Reader_Count);   // Sensors              - Sensörlerdeki sinyaller.
                    }
                }
            }
            else // GET metoduyla işlem yapılıyor.
            {
                if (this.Context.ParamExists("b")) // GET metodu ile kriptolu işlem yapılıyorsa
                {
                    // URL`deki "b" parametresi ile gelen kriptolu veriyi açarak "&" ile bir birinden ayrılmış olan cihaz parametrelerini al.
                    string s = this.Context.Param("b");

                    if (s != null)
                    {
                        byte[]? cb = new byte[0];
                        cb = ToBytes(s);

                        Split_Params(cb, true, Crypto_Enabled, false, true);
                    }
                }
                else // GET metodu ile kriptosuz işlem yapılıyor.
                if (this.Context.ParamExists("Session_Id")) // GET metodu ile kriptosuz işlem yapılıyorsa
                {
                    // URL`deki "&" ile bir birinden ayrılmış olan cihaz parametrelerini al.
                    Split_Params(null, false, Crypto_Enabled, false, true); // URL`deki "&" ile bir birinden ayrılmış olan cihaz parametrelerini al.
                }
            }


            /* TEST AMAÇLI KULLANIM
            Web_Context.Response_Write("Session_Id: "); Web_Context.Response_Write(Params.Session_Id.ToString());
            Web_Context.Response_Write(", Device_Id: "); Web_Context.Response_Write(Params.Device_Id.ToString());

            for (int i = 0; i < Params.User_Params.Count; i++)
            {
                Web_Context.Response_Write(Params.User_Params.GetKey(i));
                Web_Context.Response_Write(": ");
                Web_Context.Response_Write(Params.User_Params.Value(i));
                Web_Context.Response_Write(", ");
            }


            Web_Context.Response_Write(Params.Session_Id.ToString() + "   -   Sensörlerdeki Sinyaller<br>");
            Web_Context.Response_Write("--------------------------------------------------------------<br>");
            Web_Context.Response_Write("<br>");
            Web_Context.Response_Write("--------------------<br>");
            Web_Context.Response_Write("Lokal Sensörler<br>");
            Web_Context.Response_Write("--------------------<br>");
            Web_Context.Response_Write("  -> EmergencyExit     = " + Params.Sensors.EmergencyExit.ToString() + "<br>");
            Web_Context.Response_Write("  -> Fire              = " + Params.Sensors.Fire.ToString() + "<br>");
            Web_Context.Response_Write("  -> Alarm             = " + Params.Sensors.Alarm.ToString() + "<br>");
            Web_Context.Response_Write("  -> TamperSwitch      = " + Params.Sensors.TamperSwitch.ToString() + "<br>");
            Web_Context.Response_Write("  -> ExtraSensor1      = " + Params.Sensors.ExtraSensor1.ToString() + "<br>");
            Web_Context.Response_Write("  -> ExtraSensor2      = " + Params.Sensors.ExtraSensor2.ToString() + "<br>");
            Web_Context.Response_Write("<br>");
            Web_Context.Response_Write("--------------------<br>");
            Web_Context.Response_Write("Okuyucu Sensörleri<br>");
            Web_Context.Response_Write("--------------------<br>");

            for (int RNo = 0; RNo < Params.Reader_Count; RNo++)
            {
                Web_Context.Response_Write("  -> Reader" + (RNo + 1).ToString() + "  (Sensor1: " + Params.Sensors[RNo].Sensor1.ToString() + ", Sensör2: " + Params.Sensors[RNo].Sensor2.ToString() + ", Exit: " + Params.Sensors[RNo].Exit.ToString() + ")<br>");
            }

            Web_Context.Response_Write("<br>");
            // */
        }

        private string Get_Form_Value(string Value)
        {
#if USE_CRYPTO_DLL
            if (this.Crypto_Enable)
            {
                byte[]? cb = new byte[0];
                // cb = ToBytes(ToHex(ToBytes(Value)));  --> Eskisi gibi hex olarak göndermek istenirse buradaki kod açılmalı.
                cb = ToBytes(Value);
                Decrypto(Password, ref cb);
                return ToString(cb);
            }
            else
#endif
                return Value;
        }

        private void Split_Params(byte[]? cb, bool Do_Split, bool Crypto_Enabled, bool Get_User_Params, bool Get_Device_Params)
        {
            HttpParameters Req_Params = new HttpParameters();

            if (Do_Split)
            {
                Int32 i;
                string pn, pv;
                string[] q;
                string[] pa;

#if USE_CRYPTO_DLL
                if (Crypto_Enabled)
                    Decrypto(Password, ref cb);
#endif

                Req_Params.Clear();
                q = ToString(cb).Split('&');
                for (i = 0; i < q.Length; i++)
                {
                    pv = "";
                    pa = q[i].Split('=');
                    if (pa.Length > 0)
                    {
                        pn = pa[0];

                        if (pa.Length > 1)
                            pv = pa[1];

                        if (pn.Trim().Length > 0)
                            Req_Params.Add(pn, pv);
                    }
                }
            }
            else
                Req_Params.Assign(this.Context.Params());

            // Kullanıcı parametrelerini al.
            if (Get_User_Params)
            {
                string PrmName;

                List<string> PrmList = this.Context.Context.Request.Query.Keys.ToList();

                for (int i = 0; i < PrmList.Count; i++)
                {
                    PrmName = PrmList[i];
                    if (PrmName != null)
                        if (Params.User_Params.IndexOfName(PrmName) < 0) // Sıradaki parametre kullanıcı listesine daha önce eklenmemiş ise
                            if (Params.User_Params.Is_Device_Param(PrmName) == false) // Sıradaki parametre kullanıcı parametresi ise
                                Params.User_Params.Add(PrmName, this.Context.Param(PrmName)); // Ekle.
                }
            }

            // Cihaz parametrelerini al.
            if (Get_Device_Params) // Kullanıcı parametreleri dışındaki cihaz parametreleri de alınacak ise
            {
                Params.Session_Id = Convert.ToUInt64(Req_Params["Session_Id"]);                                     // Oturum id`si. Sunucu ile başlatılan her işlemde bir oturum id`si bulunur.
                Params.Message_Type = (HttpMessageType)Convert.ToUInt32(Req_Params["Message_Type"]);                // Mesaj türü. Sunucuya gönderilen her pakitn bir mesaj türü vardır.
                Params.Completed_Type = (HttpCompletedType)Convert.ToUInt32(Req_Params["Completed_Type"]);          // Completed komutunda kullanılır. Tamamlanan işlemin türünü bildirir.
                Params.Process_Id = Convert.ToUInt64(Req_Params["Process_Id"]);                                     // Sunucu tarafından oluşturulan bir kod`dur. Completed mesajı ile yapılan işlemin durumu bildirilirken tekrar sunucuya gönderilir.
                Params.SoftVer = Convert.ToUInt64(Req_Params["SoftVer"]);                                           // Cihazdaki yazılımın versiyonu.
                Params.Multi_User = (Params.SoftVer > 1083);                                                        // Bu alan true ise cihaza toplu şekilde kullanıcı gönderilebilir.
                Params.Device_Id = Convert.ToUInt32(Req_Params["Device_Id"]);                                       // Cihaz id`si. Sunucunun hangi cihaz ile çalıştığını anlaması için cihaz id gönderilir.
                Params.Reader_Num = Convert.ToInt32(Req_Params["Reader_Num"]);                                      // İşlem yapılan okuyucu nosu.
                Params.Wiegand_Num = Convert.ToInt32(Req_Params["Wiegand_Num"]);                                    // İşlem yapılan okuyucunun, işlem yapılan wiegand nosu.
                Params.Reader_Count = Convert.ToInt32(Req_Params["Reader_Count"]);                                  // Cihazın okuyucu portu sayısı.

                // Cihazın kullandığı ekran modeli.
                Params.Screen.Model = (ScreenModel)Convert.ToUInt32(Req_Params["Screen_Model"]);                    // Cihazın kullandığı ekran modeli.
                Params.Screen.Width = Convert.ToUInt32(Req_Params["Screen_Width"]);                                 // Ekran genişliği.
                Params.Screen.Height = Convert.ToUInt32(Req_Params["Screen_Height"]);                               // Ekran yüksekliği.

                Params.Card_Id = Convert.ToUInt64(Req_Params["Card_Id"]);                                           // Okutulan kartın bilgisi.
                Params.Password = Convert.ToInt64(Req_Params["Password"]);                                          // Kullanıcının girmiş olduğu şifre.
                Params.Registration_Number = Convert.ToInt64(Req_Params["Registration_Number"]);                    // İşlem yapan kullanıcının kayıt/sicil nosu.
                Params.Written_User_Count = Convert.ToUInt64(Req_Params["Written_User_Count"]);                     // "User" komutu ile cihaza yazılan kullanıcı sayısı.
                Params.Written_User_Forbidden_Count = Convert.ToUInt64(Req_Params["Written_User_Forbidden"]);       // "User_Forbidden" komutu ile cihaza yazılan kullanıcı sayısı.
                Params.Written_User_Unforbidden_Count = Convert.ToUInt64(Req_Params["Written_User_Unforbidden"]);   // "User_Unforbidden" komutu ile cihaza yazılan kullanıcı sayısı.
                Params.User_Count = Convert.ToUInt64(Req_Params["User_Count"]);                                     // Cihazdaki kullanıcı sayısı.
                Params.Log_Count = Convert.ToUInt64(Req_Params["Log_Count"]);                                       // Cihazdaki log sayısı.
                Params.Log_Event = (LogEvent)Convert.ToUInt32(Req_Params["Log_Event"]);                             // Log türü.
                Params.Log_Time = Convert_To_LogTime(Req_Params["Log_Time"]);                                       // İşlem zamanı.
                Params.Tag_Type = (RFTagType)Convert.ToInt32(Req_Params["Tag_Type"]);                               // Okutulan kartın türü. DESFire veya Mifare klasik..vb.
                Params.Data = HttpUtility.UrlDecodeToBytes(Req_Params["Data"]);                                     // Gelen veri.
                Params.Data_Count = Params.Data != null ? Params.Data.Length : 0;                                   // Data parametresi ile gelen verinin uzunluğu.
                // Params.Data = ToBytes(HttpUtility.UrlDecode(Req_Params["Data"]));                                // Gelen veri.
                // Params.Data_Count = Convert.ToInt32(Req_Params["Data_Count"]);                                   // Data parametresi ile gelen verinin uzunluğu.

                /*
                byte[]? B = ToBytes(Req_Params["Data"]);
                if (B != null)
                    Console.WriteLine("-> Data: " + Encoding.UTF8.GetString(B));
                // */

                // Grup erişim bölgeleri.
                Params.Access_Zones.Decode(Req_Params["Group_Access_Zones"]);

                // Sensörlerin şu an ki sinyal verileri.
                Params.Sensors.Unpack(Convert.ToUInt64(Req_Params["Sensors"]), Params.Reader_Count);
            }
        }

        public string GetResponse()
        {
            // Yanıt verisini ayarlayıp döndürür.
            if (Is_Session_Valid(Params.Session_Id)) // Session id geçerli ise
            {
                // Gelen mesajın oturum id`si olduğu gibi döndürülür. <Session_Id> komutunun kullanılması zorunludur.

                Response_Buffer = Response_Buffer.Trim();
                if (Response_Buffer.Length > 0)
                {
                    if (Response_Buffer[0] != '{')
                        Response_Buffer = "{" + Response_Buffer;

                    if (Response_Buffer[Response_Buffer.Length - 1] != '}')
                    {
                        if (Response_Buffer[Response_Buffer.Length - 1] == ',')
                            Response_Buffer = Response_Buffer.Substring(0, Response_Buffer.Length - 1);

                        Response_Buffer += "}";
                    }
                }

                string RetVal = "<Session_Id>" + Params.Session_Id.ToString() + "</>" + Response_Buffer;

#if USE_CRYPTO_DLL
                if (Crypto_Enable == true)
                {
                    byte[] Dst_Buffer = new byte[0];
                    WebAccess.Crypto(Password, ToBytes(RetVal), ref Dst_Buffer);
                    return ToHex(Dst_Buffer);
                }
                else
#endif
                {
                    return RetVal;
                }
            }
            else
                return "";
        }

        public void Send(string Value)
        {
            Value = Value.Trim();
            Response_Buffer = Response_Buffer.Trim();
            if (Response_Buffer.Length > 0)
                if (Value.Length > 0 && Value[0] != ',')
                {
                    char S = Response_Buffer[Response_Buffer.Length - 1];
                    if (S != '>' && S != '{' && S != ',')
                        Response_Buffer += ',';
                }

            Response_Buffer += Value;
        }
        
        public void SendCommand(string Key, string Value)
        {
            Response_Buffer = Response_Buffer.Trim();
            if (Response_Buffer.Length > 0)
                if (Response_Buffer[Response_Buffer.Length - 1] != ',')
                    Response_Buffer += ',';

            Response_Buffer += '"' + Key.Trim() + '"' + ':' + '"' + Value + '"';
        }
        
        public void SendCommand(string Key, UInt64 Value)
        {
            SendCommand(Key, Value.ToString());
        }

        public void SendCommand(string Key, Int64 Value)
        {
            SendCommand(Key, Value.ToString());
        }

        public void SendCommand(string Key, UInt32 Value)
        {
            SendCommand(Key, Value.ToString());
        }

        public void SendCommand(string Key, Int32 Value)
        {
            SendCommand(Key, Value.ToString());
        }

        public void SendCommand(string Key, bool Value)
        {
            SendCommand(Key, (Value ? "true" : "false"));
        }

        public void SendCommand(string Key, DateTime Value)
        {
            //                                                       YYYY-MM-DD HR:MN:SC
            // Cihaz zamanı formatı şu şekilde  ->  "Device_Time" : "2023-04-02 14:34:56"
            string Device_Time = Value.Year.ToString() + '-' + Value.Month.ToString() + '-' + Value.Day.ToString() + ' ' +
                                 Value.Hour.ToString() + ':' + Value.Minute.ToString() + ':' + Value.Second.ToString();

            SendCommand(Key, Device_Time);
        }

        public void SendArray(string Key, string Value)
        {
            Response_Buffer = Response_Buffer.Trim();
            if (Response_Buffer.Length > 0)
                if (Response_Buffer[Response_Buffer.Length - 1] != ',')
                    Response_Buffer += ',';

            Response_Buffer += '"' + Key.Trim() + '"' + ':' + '[' + Value + ']';
        }

        public void Send_Process_Id(UInt64 Process_Id)
        {
            SendCommand("Process_Id", Process_Id);
        }

        public void Send_I_Am_Here_Duration(UInt32 Duration)
        {
            SendCommand("I_Am_Here_Duration", Duration.ToString());
        }

        public void Send_Device_Time(DateTime Value)
        {
            //                                                       YYYY-MM-DD HR:MN:SC
            // Cihaz zamanı formatı şu şekilde  ->  "Device_Time" : "2023-04-02 14:34:56"
            string Device_Time = Value.Year.ToString() + '-' + Value.Month.ToString() + '-' + Value.Day.ToString() + ' ' +
                                 Value.Hour.ToString() + ':' + Value.Minute.ToString() + ':' + Value.Second.ToString();

            SendCommand("Device_Time", Device_Time);
        }

        public void Session_Id_Mode(WebSessionIdMode Session_Id_Mode)
        {
            switch (Session_Id_Mode)
            {
                case WebSessionIdMode.Random:
                    SendCommand("Session_Id_Mode", "Random");
                    break;

                // case WebSessionIdMode.DeviceId:
                default:
                    SendCommand("Session_Id_Mode", "DeviceId");
                    break;
            }
        }

        public void Check_Session_Id(bool Check)
        {
            SendCommand("Check_Session_Id", Check);
        }

        public void Send_Clear_Screen(Int32 Background_Color = -1)
        {
            // Ekranı temizler.
            SendCommand("Clear_Screen", ToHexColor((UInt32)Background_Color));
        }

        public void Send_Draw_Icon(IconType Icon_Type, Int32 X, Int32 Y, UInt32 Duration, Int32 Background_Color = -1)
        {
            // TFT ekranlı cihazlarda ekrana ikon çizer.

            string Value = '"' + Icon_Type.ToString() + "\"," + X.ToString() + ',' + Y.ToString() + ',' + Duration.ToString() + ",\"" + (Background_Color < 0 ? "0x1000000" : ToHexColor((UInt32)Background_Color)) + '"';
            SendArray("Draw_Icon", Value);
        }

        public void Send_Draw_Text(Int32 X, Int32 Y, string Text, UInt32 Duration, UInt32 Font_Size = 1, Int32 Font_Color = 0, Int32 Background_Color = -1, 
                                 TextAlignment Alignment = TextAlignment.Left, TextEncoding Text_Encoding = TextEncoding.UTF8)
        {
            // TFT ekranlı cihazlarda ekrana ikon çizer.

            string Value = X.ToString() + ',' + Y.ToString() + ",\"" + Text + "\"," + Duration.ToString() + ',' + Font_Size.ToString() + ",\"" +  Alignment.ToString() + "\",\"" +
                           ToHexColor((UInt32)Font_Color) + "\",\"" + (Background_Color < 0 ? "0x1000000" : ToHexColor((UInt32)Background_Color)) + "\",\"" + Text_Encoding.ToString() + '"';

            SendArray("Draw_Text", Value);
        }

        public enum DESFireKeyType
        {
            T_DES = 0,  // 56-bit DES (single DES, DES)
            T_2K3DES = 1,  // 112-bit 3DES (2 key triple DES, 2K3DES)
            T_3K3DES = 2,  // 168-bit 3DES (3 key triple DES, 3K3DES)
            T_AES = 3  // AES-128
        };

        public enum FileDataType
        {
            String = 0,
            Base64 = 1,
            Hex = 2
        };

        public void Send_DESFire_Set_Key(byte Key_No, DESFireKeyType Key_Type, byte Key_Version, string Key_Data)
        {
            string KT_Str;

            switch (Key_Type)
            {
                case DESFireKeyType.T_DES:
                    KT_Str = "DES";
                    break;

                case DESFireKeyType.T_2K3DES:
                    KT_Str = "3DES";
                    break;

                case DESFireKeyType.T_3K3DES:
                    KT_Str = "3K3DES";
                    break;

                // case DESFireKeyType.T_AES:
                default:
                    KT_Str = "AES";
                    break;
            }

            SendArray("DESFire_Set_Key", Key_No.ToString() + ',' + '"' + KT_Str + '"' + ',' + Key_Version.ToString() + ',' + '"' + Key_Data + '"');
        }

        public void Send_DESFire_Read_File(UInt32 Process_Id, UInt32 App_Id, byte File_No, UInt32 Offset, UInt32 Length, FileDataType Data_Type = FileDataType.String)
        {
            SendArray("DESFire_Read_File", Process_Id.ToString() + ',' + '"' + App_Id.ToString("X6") + '"' + ',' + File_No.ToString() + ',' + Offset.ToString() + ',' + Length.ToString() + ',' + Convert.ToString((UInt32)Data_Type));
        }

        public void Send_DESFire_Write_File(UInt32 Process_Id, UInt32 App_Id, byte File_No, UInt32 Offset, byte[] Data, FileDataType Data_Type = FileDataType.String)
        {
            string Str_Data;

            switch (Data_Type)
            {
                case FileDataType.Base64:
                    Str_Data = Convert.ToBase64String(Data);
                    break;

                case FileDataType.Hex:
                    Str_Data = Convert.ToHexString(Data);
                    break;

                // case FileDataType.String:
                default:
                    Str_Data = System.Text.Encoding.UTF8.GetString(Data);
                    break;
            }

            SendArray("DESFire_Write_File", Process_Id.ToString() + ',' + '"' + App_Id.ToString("X6") + '"' + ',' + File_No.ToString() + ',' + Offset.ToString() + ',' + '"' + Str_Data + '"' + ',' + Convert.ToString((UInt32)Data_Type));
        }

        public void Send_Mifare_Set_Key(string Key_Data, byte Mifare_Sector_No, UInt32 DESFire_App_Id = 0x201703, byte DESFire_File_No = 1)
        {
            // C++ : void Mifare_Set_Key(byte Key_Data[12], byte DESFire_App_Id[6], byte DESFire_File_No, byte Mifare_Sector_No, byte Key_Type);
            // SendArray("Mifare_Set_Key", '"' + Key_Data + '"' + ',' + '"' + DESFire_App_Id.ToString("X6") + '"' + ',' + DESFire_File_No.ToString() + ',' + Mifare_Sector_No.ToString() + ',' + '0');

            string Prm_Key_Data = "";

            for (int i = 0; i < Key_Data.Length; i++)
                if ((Key_Data[i] >= 'A' && Key_Data[i] <= 'F') || (Key_Data[i] >= 'a' && Key_Data[i] <= 'f') || (Key_Data[i] >= '0' && Key_Data[i] <= '9'))
                    Prm_Key_Data += Key_Data[i];

            if (Prm_Key_Data.Length > 0)
                SendArray("Mifare_Set_Key", '"' + Prm_Key_Data + '"' + ',' + '"' + DESFire_App_Id.ToString("X6") + '"' + ',' + DESFire_File_No.ToString() + ',' + Mifare_Sector_No.ToString() + ',' + '0');
        }

        public void Send_Read_Card_Data(UInt32 Process_Id, UInt32 Offset, UInt32 Length, byte Sector_No, FileDataType Data_Type = FileDataType.String)
        {
            SendArray("Read_Card_Data", Process_Id.ToString() + ',' + Offset.ToString() + ',' + Length.ToString() + ',' + Sector_No.ToString() + ',' + Convert.ToString((UInt32)Data_Type));
        }

        public void Send_Write_Card_Data(UInt32 Process_Id, UInt32 Offset, byte Sector_No, byte[] Data, FileDataType Data_Type = FileDataType.String)
        {
            string Str_Data;

            switch (Data_Type)
            {
                case FileDataType.Base64:
                    Str_Data = Convert.ToBase64String(Data);
                    break;

                case FileDataType.Hex:
                    Str_Data = Convert.ToHexString(Data);
                    break;

                // case FileDataType.String:
                default:
                    Str_Data = System.Text.Encoding.UTF8.GetString(Data);
                    break;
            }

            SendArray("Write_Card_Data", Process_Id.ToString() + ',' + Offset.ToString() + ',' + Sector_No.ToString() + ',' + '"' + Str_Data + '"' + ',' + Convert.ToString((UInt32)Data_Type));
        }

        public void Send_Sound(Int32 Reader_Num, SoundSignalType Sound_Signal_Type)
        {
            if (Reader_Num < 1)
                Reader_Num = 1;

            if (Reader_Num > 16)
                Reader_Num = 16;

            SendArray("Sound", Reader_Num.ToString() + ',' + ((Int32)Sound_Signal_Type).ToString());
        }

        public void Send_Trigger_Relay(Int32 Reader_Num, UInt32 Duration)
        {
            if (Reader_Num < 1)
                Reader_Num = 1;

            if (Reader_Num > 16)
                Reader_Num = 16;

            SendArray("Trigger_Relay", Reader_Num.ToString() + ',' + Duration.ToString());
        }

        public void Send_Erase_All_Logs()
        {
            SendCommand("Erase_All_Logs", true);
        }

        public void Send_Log_Rollback(UInt64 Log_Count)
        {
            SendCommand("Log_Rollback", Log_Count);
        }

        public void Send_Log_Rollforward(UInt64 Log_Count)
        {
            SendCommand("Log_Rollforward", Log_Count);
        }

        public void Send_Erase_All_Users()
        {
            SendCommand("Erase_All_Users", true);
        }

        public void Send_User_Forbidden(UInt64 Card_Id)
        {
            SendCommand("User_Forbidden", Card_Id);
        }

        public void Send_User_Unforbidden(UInt64 Card_Id)
        {
            SendCommand("User_Unforbidden", Card_Id);
        }

        public void Send_Local_Sensor_Sensibility(UInt16 Value_ms)
        {
            SendCommand("Local_Sensor_Sensibility", Value_ms);
        }

        public void Send_Door_Sensor_Sensibility(UInt16 Value_ms)
        {
            SendCommand("Door_Sensor_Sensibility", Value_ms);
        }

        private static string ToHexItem(byte Sayi)
        {
            if (Sayi >= 0 && Sayi <= 9)
                return Sayi.ToString();
            else
                if (Sayi >= 0x0A && Sayi <= 0x0F)
                return ((char)(Sayi + 55)).ToString();
            else
                return "";
        }

        private static string ToHex(byte[] Bytes)
        {
            string RetVal = "";

            for (int i = 0; i < Bytes.Length; i++)
                RetVal += ToHexItem(Convert.ToByte(Bytes[i] / 16)) + ToHexItem(Convert.ToByte(Bytes[i] % 16));

            return (RetVal);
        }

        private static string ToHexColor(UInt32 Color)
        {
            string RetVal = "";
            byte[] Buf = new byte[3];

            unsafe
            {
                byte* B = (byte*)&Color;

                for (int i = 0; i < 3; i++)
                    Buf[2 - i] = B[i];

            }

            bool First_Item = true;
            for (int i = 0; i < Buf.Length; i++)
                if (Buf[i] > 0 || First_Item == false)
                {
                    First_Item = false;
                    RetVal += ToHexItem(Convert.ToByte(Buf[i] / 16)) + ToHexItem(Convert.ToByte(Buf[i] % 16));
                }
                    

            return (RetVal);
        }

        private static byte[]? ToBytes(string? Str)
        {
            if (Str != null)
                if (Str.Length > 0)
                {
                    byte[] RetVal = new byte[Str.Length];

                    for (int i = 0; i < Str.Length; i++)
                        RetVal[i] += (byte)Str[i];

                    return RetVal;
                }

            return null;
        }

        unsafe public static byte[] ToBytes(byte* Bytes, int Count)
        {
            int i;
            byte[] RetVal = new byte[Count];

            for (i = 0; i < Count; i++)
                RetVal[i] = Bytes[i];

            return RetVal;
        }

        public static string ToString(byte[]? Bytes)
        {
            string RetVal = "";

            if (Bytes != null)
            {
                for (int i = 0; i < Bytes.Length; i++)
                    RetVal += (char)Bytes[i];
            }

            return RetVal;
        }

        private DateTime Convert_To_LogTime(string? Str_LogTime)
        {
            // Str_LogTime içindeki zamanı Datetime türüne çevirir.

            DateTime RetVal;

            if (!string.IsNullOrEmpty(Str_LogTime))
            {
                Int32 yr, mt, dy, hr, mn, sc, ms;
                Int32 StartIndex;

                StartIndex = 0;
                yr = Get_Number(Str_LogTime, ref StartIndex, 2000);
                mt = Get_Number(Str_LogTime, ref StartIndex, 1);
                dy = Get_Number(Str_LogTime, ref StartIndex, 1);
                hr = Get_Number(Str_LogTime, ref StartIndex);
                mn = Get_Number(Str_LogTime, ref StartIndex);
                sc = Get_Number(Str_LogTime, ref StartIndex);
                ms = Get_Number(Str_LogTime, ref StartIndex);

                try
                {
                    RetVal = new DateTime(yr, mt, dy, hr, mn, sc, ms);
                }
                catch
                {
                    RetVal = new DateTime(2000, 1, 1, 0, 0, 0, 0);
                }
            }
            else
                RetVal = new DateTime(2000, 1, 1, 0, 0, 0, 0);

            return RetVal;
        }

        private Int32 Get_Number(string S, ref Int32 StartIndex, Int32 Default = 0)
        {
            int i, Ln;
            string Str = "";

            if (S == null)
                return Default;

            if (StartIndex >= S.Length)
                return Default;

            Ln = S.Length;
            for (i = StartIndex; i < Ln; i++)
            {
                if (S[i] >= '0' && S[i] <= '9')
                    Str += S[i];
                else
                if (S[i] != 0x20 || (Str.Length > 0)) // İşlenen karakter boşluk değilse veya boşluksa ve rakam alınmış ise
                {
                    // Rakam olmayan karakterleri sonraki rakama kadar atla.
                    while (i < S.Length && (S[i] < '0' || S[i] > '9'))
                    {
                        i++;
                    }

                    break; // for i döngüsünün dışına çık.
                }
            }

            StartIndex = i;

            if (Str.Length > 0)
            {
                try { return Convert.ToInt32(Str); } catch { }
            }

            return Default;
        }

        public bool Is_Session_Valid(UInt64 Value)
        {
            Int32 i, sz; UInt64 mb; UInt64[] t = new UInt64[4];

            if (Value > 0)
            {
                mb = 0xFF;
                sz = 24;

                for (i = 3; i > -1; i--)
                {
                    t[i] = (Value >> sz) & mb;
                    sz -= 8;
                }

                return ((Value >> 32) & 0xFFFFFFFF) == (((t[0] ^ t[2]) << 24) | ((t[1] ^ t[3]) << 16) | ((t[2] ^ t[3]) << 8) | (t[1] ^ t[0]));
            }
            else
                return false;
        }

        static public string EventToString(LogEvent ALog_Event)
        {
            string RetVal = "";

            switch (ALog_Event)
            {
                case LogEvent.AccessSuccessful: RetVal = "İzin verildi"; break;
                case LogEvent.UserInside: RetVal = "İzin verildi (Giriş yaptı)"; break;
                case LogEvent.UserOutside: RetVal = "İzin verildi (Çıkış yaptı)"; break;
                case LogEvent.UnauthorizedAccess: RetVal = "İzin verilmedi (Kişi tanımlı değil)"; break;
                case LogEvent.UnauthorizedEntry: RetVal = "Giriş engellendi (Kişi tanımlı değil)"; break;
                case LogEvent.UnauthorizedExit: RetVal = "Çıkış engellendi (Kişi tanımlı değil)"; break;
                case LogEvent.OutOfTimeZone: RetVal = "İzin verilmedi (Zaman dilimi dışında)"; break;
                case LogEvent.TimeLimited: RetVal = "İzin verilmedi (Zaman kısıtlaması)"; break;
                case LogEvent.CreditDone: RetVal = "Kontör bitti"; break;
                case LogEvent.PasswordError: RetVal = "İzin verilmedi (Hatalı şifre girişi)"; break;
                case LogEvent.AntiPassbackEntry: RetVal = "İzin verildi (Giriş)"; break;
                case LogEvent.AntiPassbackExit: RetVal = "İzin verildi (Çıkış)"; break;
                case LogEvent.TryingExitWithoutEntry: RetVal = "Çıkış isteği engellendi (Giriş yapmadan çıkmaya çalıştı)"; break;
                case LogEvent.TryingEntryWithoutExit: RetVal = "Giriş isteği engellendi (Çıkış yapmadan girmeye çalıştı)"; break;
                case LogEvent.ForbiddenDoor: RetVal = "İzin verilmedi (Kapı hizmet dışı)"; break;
                case LogEvent.DoorOpenRemained: RetVal = "Kapı açık kaldı"; break;
                case LogEvent.UnauthorizedAccessToSubgroup: RetVal = "İzin verilmedi (Ana kapıdan geçmemiş)"; break;
                case LogEvent.DoorForciblyOpened: RetVal = "Kapı zorla açıldı"; break;
                case LogEvent.CapacityIsFull: RetVal = "Erişim bölgesine giriş limiti dolmuş"; break;
                case LogEvent.DoorSensorActive: RetVal = "Kapı açıldı"; break;
                case LogEvent.DoorSensorPassive: RetVal = "Kapı kapandı"; break;
                case LogEvent.DoorExitButton: RetVal = "Buton ile geçiş"; break;
                case LogEvent.EmergencyExitButton: RetVal = "Acil durum butonuna basılmış"; break;
                case LogEvent.FireSensor: RetVal = "Yangın sensörü aktif"; break;
                case LogEvent.AlarmSensor: RetVal = "Alarm sensörü aktif"; break;
                case LogEvent.TamperSensor: RetVal = "Pano kapağı açıldı"; break;
                case LogEvent.ExtraSensor1: RetVal = "Ekstra Sensör 1 aktif"; break;
                case LogEvent.ExtraSensor2: RetVal = "Ekstra Sensör 2 aktif"; break;
                case LogEvent.GDMNoTransition: RetVal = "Geçiş yapmadı (Uygun olmayan geçiş kontrolü)"; break;
                case LogEvent.Disconnect: RetVal = "Bağlantı koptu"; break;
                case LogEvent.Connect: RetVal = "Yeniden bağlandı"; break;
                case LogEvent.SystemStart: RetVal = "Cihaz açıldı"; break;
                case LogEvent.PGMLimited: RetVal = "İzin verilmedi (Kişinin grup limiti doldu)"; break;
                case LogEvent.ForbiddenDay: RetVal = "İzin verilmedi (Kişinin bugün geçiş izni yok)"; break;
                case LogEvent.FinishedAccessToTimePeriod: RetVal = "İzin verilmedi (Zaman dilimindeki geçiş hakkı bitti)"; break;
                case LogEvent.FinishedToDailyAccess: RetVal = "İzin verilmedi (Günlük geçiş hakkı bitti)"; break;
                case LogEvent.LifetimeExpired: RetVal = "İzin verilmedi (Kapıyı kullanım süresi bitti)"; break;
                case LogEvent.FinishedAccessToLifetime: RetVal = "İzin verilmedi (Kapıyı kullanım süresi içindeki geçiş hakkı bitti)"; break;
                case LogEvent.CounterSensor1In: RetVal = "Kapının 1.sensöründen giriş sinyali alındı"; break;
                case LogEvent.CounterSensor1Out: RetVal = "Kapının 1.sensöründen çıkış sinyali alındı"; break;
                case LogEvent.CounterSensor2In: RetVal = "Kapının 2.sensöründen giriş sinyali alındı"; break;
                case LogEvent.CounterSensor2Out: RetVal = "Kapının 2.sensöründen çıkış sinyali alındı"; break;
                case LogEvent.CounterSensorExitIn: RetVal = "Kapının exit sensöründen giriş sinyali alındı"; break;
                case LogEvent.CounterSensorExitOut: RetVal = "Kapının exit sensöründen çıkış sinyali alındı"; break;
                case LogEvent.ClosedTheForciblyOpenedDoor: RetVal = "Kapı kapatıldı (Zorla açılmıştı)"; break;
                case LogEvent.ClosedTheOpenRemainedDoor: RetVal = "Kapı kapatıldı (Açık unutulmuştu)"; break;
                case LogEvent.DoorAlarmSensorActive: RetVal = "Kapı alarm sensörü aktif"; break;
                case LogEvent.DoorAlarmSensorPassive: RetVal = "Kapı alarm sensörü pasif"; break;
                case LogEvent.RTCError: RetVal = "Cihaz saatinde sorun var"; break;
                case LogEvent.PassedWithCardAndPassword: RetVal = "İzin verildi (Kart ve şifre ile geçti)"; break;
                case LogEvent.PassedWithPassword: RetVal = "İzin verildi (Şifre ile geçti)"; break;
            }

            return RetVal;
        }

        public bool IsSuccessEvent(LogEvent Event)
        {
            // Başarılı bir geçiş log`u ise true döndürür.
            switch (Event)
            {
                case LogEvent.AccessSuccessful:
                case LogEvent.UserInside:
                case LogEvent.UserOutside:
                case LogEvent.AntiPassbackEntry:
                case LogEvent.AntiPassbackExit:
                case LogEvent.DoorExitButton:
                case LogEvent.DoorSensorActive:
                case LogEvent.DoorForciblyOpened:
                case LogEvent.CounterSensor1In:
                case LogEvent.CounterSensor1Out:
                case LogEvent.CounterSensor2In:
                case LogEvent.CounterSensor2Out:
                case LogEvent.CounterSensorExitIn:
                case LogEvent.CounterSensorExitOut:
                case LogEvent.PassedWithCardAndPassword:
                case LogEvent.PassedWithPassword:
                    return true;

                default:
                    return false;
            }
        }

#if USE_CRYPTO_DLL
        public static void Crypto(UInt64 Password, byte[]? Src_Buffer, ref byte[] Dst_Buffer)
        {
            IntPtr Dst_Buffer_Ptr = IntPtr.Zero;
            Int32 Dst_Len;

            if (Src_Buffer != null)
            {
                _Crypto(Password, Src_Buffer, Src_Buffer.Length, ref Dst_Buffer_Ptr, out Dst_Len);

                unsafe
                {
                    Dst_Buffer = ToBytes((byte*)Dst_Buffer_Ptr, Dst_Len);
                }
            }
        }

        public static void Decrypto(UInt64 Password, ref byte[]? Crypt_Data)
        {
            IntPtr Decrypted_Data_Ptr = IntPtr.Zero;
            Int32 Decrypted_Data_Size;
            Int32 Len;

            if (Crypt_Data != null)
                Len = Crypt_Data.Length;
            else
                Len = 0;

            _Decrypto(Password, Crypt_Data, Len, ref Decrypted_Data_Ptr, out Decrypted_Data_Size);

            unsafe
            {
                Crypt_Data = ToBytes((byte*)Decrypted_Data_Ptr, Decrypted_Data_Size);
            }
        }

        public static bool Get_Password(string Password_Str, string New_Password_Str, out UInt64 Password_64, out UInt64 New_Password_64)
        {
            byte[]? Password = null;
            byte[]? New_Password = null;

            Password_64 = 0;
            New_Password_64 = 0;

            if (Password_Str != null)
            {
                Password = ToBytes(Password_Str);
                if (Password != null)
                    Password_64 = (UInt64)Password.Length;
            }

            if (New_Password_Str != null)
            {
                New_Password = ToBytes(New_Password_Str);
                if (New_Password != null)
                    New_Password_64 = (UInt64)New_Password.Length;
            }

            return _Get_Password(Password, (int)Password_64, New_Password, (int)New_Password_64, out Password_64, out New_Password_64);
        }

        [DllImport(DLL_NAME, EntryPoint = "Crypto", CharSet = CharSet.None, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool _Crypto(UInt64 Password, byte[] Src_Buffer, Int32 Src_Len, ref IntPtr Dst_Buffer, out Int32 Dst_Len);

        [DllImport(DLL_NAME, EntryPoint = "Decrypto", CharSet = CharSet.None, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool _Decrypto(UInt64 Password, byte[]? Crypt_Data, Int32 Crypt_Data_Size, ref IntPtr Decrypted_Data, out Int32 Decrypted_Data_Size);

        [DllImport(DLL_NAME, EntryPoint = "Get_Password", CharSet = CharSet.None, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool _Get_Password(byte[]? Password_Buf, Int32 Password_Buf_Size, byte[]? New_Password_Buf, Int32 New_Password_Buf_Size, out UInt64 Password_64, out UInt64 New_Password_64);

        private const string DLL_NAME = @"PDeviceCrypt.dll";

        private UInt64 Password = 0; // Önceden cihaza verilmiş olan şifre.
        private bool Crypto_Enable = true; // Bu alan true ise cihaza gönderilecek olan veriler şifrelenir.
#endif

        public const UInt64 Empty_Card = 0x7FFFFFFFFFFFFFFF; // Geçerli bir kart no sıfır`dan büyük, buradaki değerden küçük olmalıdır.
        public readonly HttpParams Params;
        public ASPCoreContext Context;
    }

    public class SensorInfo
    {
        public SensorInfo()
        {
            Sensor1 = false;
            Sensor2 = false;
            Exit = false;
        }

        public bool Sensor1;
        public bool Sensor2;
        public bool Exit;
    }

    public class SensorSignals
    {
        public SensorSignals()
        {
            Sensor_Signals = new List<SensorInfo>();

            for (int i = 0; i < 16; i++)
                Sensor_Signals.Add(new SensorInfo());

            Clear();
        }

        public SensorInfo this[int Index]
        {
            get { return Sensor_Signals[Index]; }
        }

        public void Clear()
        {
            EmergencyExit = false;
            Fire = false;
            Alarm = false;
            TamperSwitch = false;
            ExtraSensor1 = false;
            ExtraSensor2 = false;

            for (int i = 0; i < Sensor_Signals.Count(); i++)
            {
                Sensor_Signals[i].Sensor1 = false;
                Sensor_Signals[i].Sensor2 = false;
                Sensor_Signals[i].Exit = false;

            }
        }

        public void Unpack(UInt64 Value, Int32 Reader_Count)
        {
            Int32 p;

            EmergencyExit = (((Value >> 63) & 0x01) == 1);
            Fire = (((Value >> 62) & 0x01) == 1);
            Alarm = (((Value >> 61) & 0x01) == 1);
            TamperSwitch = (((Value >> 60) & 0x01) == 1);
            ExtraSensor1 = (((Value >> 59) & 0x01) == 1);
            ExtraSensor2 = (((Value >> 58) & 0x01) == 1);

            p = 57; // 6 bitlik lokal sensör bilgisi en sonda bulunduğundan bu alanı atlıyoruz.

            for (int i = 0; i < Reader_Count; i++)
            {
                Sensor_Signals[i].Sensor1 = (((Value >> (p - 0)) & 0x01) == 1);
                Sensor_Signals[i].Sensor2 = (((Value >> (p - 1)) & 0x01) == 1);
                Sensor_Signals[i].Exit = (((Value >> (p - 2)) & 0x01) == 1);
                p -= 3; // Sensor verisi`nden alınan üç biti düş.
            }
        }

        public bool EmergencyExit;
        public bool Fire;
        public bool Alarm;
        public bool TamperSwitch;
        public bool ExtraSensor1;
        public bool ExtraSensor2;
        private List<SensorInfo> Sensor_Signals;
    }

    /* Yazı yönü. */
    public enum TextAlignment
    {
        Left = 0, // Sola yanaşık
        Right = 1, // Sağa yanaşık
        Center = 2 // Ortalı
    };

    /* Text karakter kodlaması. */
    public enum TextEncoding
    {
        Default = 0,
        UTF8 = 1
    };

    /* Ses sinyal kodları. */
    public enum SoundSignalType
    {
        None = 0,  // Sinyal kullanılmıyor.
        CardOk = 1,  // Kart onaylandı sinyali.
        CardError = 2,  // Kart onaylanmadı sinyali.
        DoorForciblyOpened = 3,  // Kapı zorla açıldı sinyali.
        DoorOpenRemained = 4,  // Kapı açık kaldı sinyali.
        DoubleOk = 5,  // Çift onay sinyali.
        Error = 6,  // Hata sinyali.
        CreditDone = 7   // Kontör bitti sinyali.
    };

    public enum IconType
    {
        None = 0,  // kullanılmıyor.
        Ok = 1,  // Onaylandı
        Error = 2  // Onaylanmadı.
    };

    /* Web Access modunda Session_Id oluşturulurken izlenecek yolu bildirir. Random ise rastgele, DeviceId ise cihazın id`sinden Session_Id üretilir. */
    public enum WebSessionIdMode
    {
        DeviceId = 0,
        Random = 1
    };

    /* Cihazdan gelen mesaj türleri. */
    public enum HttpMessageType
    {
        None = 0,       // Bilinmiyor.
        Read_Card = 1,  // Kart okutuldu.
        I_Am_Here = 2,  // Buradayım mesajı.
        Log_Record = 3, // Log kaydı.
        Completed = 4   // Son yapılan kayıt işleminin başarıyla bittiğini bildirir. (Yeni bir kişi eklemek gibi)
    }

    // Tamamlanan işlemin türü.
    public enum HttpCompletedType
    {
        None = 0,
        User_Inserted = 1,              // Kullanıcı eklendi.
        User_Updated = 2,               // Var olan kullanıcı güncellendi.
        User_Forbidden = 3,             // Kullanıcı yasaklandı.
        User_Unforbidden = 4,           // Kullanıcının yasağı kaldırıldı.
        User_Write_Data = 5,            // Kullanıcı kartına veri yazıldı.
        User_Read_Data = 6,             // Kullanıcı kartından veri okundu.
        User_Write_Data_Error = 7,      // Kullanıcı kartına veri yazma hatası.
        User_Read_Data_Error = 8,       // Kullanıcı kartından veri okuma hatası.
        User_Capacity_Reached = 9       // Cihaz kullanıcı kapasitesi doldu.
    }

    /* Cihazdan gelen parametreler. */
    public class HttpParameters : NameValueCollection
    {
        public void Assign(NameValueCollection? Http_Params)
        {
            if (Http_Params != null)
            {
                for (int i = 0; i < Http_Params.Count; i++)
                {
                    string[]? S = Http_Params.GetValues(i);
                    if (S != null)
                        Add(Http_Params.GetKey(i), S[0]);
                }
            }
        }

        public string Value(int Index)
        {
            string[]? S = GetValues(Index);
            if (S != null)
                return S[0];
            else
                return "";
        }

        public string Value(string Param_Name)
        {
            string[]? S = GetValues(Param_Name);
            if (S != null)
                return S[0];
            else
                return "";
        }

        public int IndexOfName(string Param_Name)
        {
            string? S;

            Param_Name = Param_Name.ToLower();

            for (int i = 0; i < Count; i++)
            {
                S = GetKey(i);
                if (S != null)
                    if (S.ToLower() == Param_Name)
                        return i;
            }

            return -1;
        }

        public bool Is_Device_Param(string Param_Name)
        {
            string[] No_Conversion = {"b", "Session_Id", "Message_Type", "Completed_Type", "Process_Id", "SoftVer", "Device_Id",
                                                       "Reader_Num", "Wiegand_Num", "Reader_Count", "Card_Id", "Password", "Registration_Number",
                                                       "Written_User_Count", "Written_User_Forbidden", "Written_User_Unforbidden",
                                                       "User_Count", "Log_Count", "Log_Event", "Log_Time",
                                                       "Data_Count", "Data", "Group_Access_Zones", "Sensors",
                                                       "Screen_Model", "Screen_Width", "Screen_Height" };

            Param_Name = Param_Name.ToLower();

            for (int i = 0; i < No_Conversion.Length; i++)
            {
                if (Param_Name == No_Conversion[i].ToLower())
                    return true;
            }

            return false;
        }
    }

    public class HttpParams
    {
        public class HttpParam
        {
            public string PN = "";
            public string PV = "";
        }

        public HttpParams()
        {
            Session_Id = 0;
            Message_Type = HttpMessageType.None;
            Completed_Type = HttpCompletedType.None;
            Process_Id = 0;
            SoftVer = 0;
            Multi_User = false;
            Device_Id = 0;
            Reader_Num = 0;
            Wiegand_Num = 0;
            Reader_Count = 0;
            Screen = new ScreenInfo();
            Card_Id = 0;
            Password = 0;
            Registration_Number = 0;
            Written_User_Count = 0;
            Written_User_Forbidden_Count = 0;
            Written_User_Unforbidden_Count = 0;
            User_Count = 0;
            Log_Count = 0;
            Log_Event = LogEvent.AccessSuccessful;
            Log_Time = DateTime.Now;
            Tag_Type = RFTagType.None;
            Data_Count = 0;
            Data = null;
            Access_Zones = new GroupAccessZones();
            Sensors = new SensorSignals();
            User_Params = new HttpParameters();
            List = new List<HttpParam>();
        }

    public string this[string pName]
        {
            get
            {
                pName = pName.Trim();

                for (int i = 0; i < List.Count; i++)
                    if (List[i].PN == pName)
                        return List[i].PV;

                // throw new IndexOutOfRangeException();
                return "0";
            }
        }

        public HttpParam Add(string PN, string PV)
        {
            List.Add(new HttpParams.HttpParam());
            List[List.Count() - 1].PN = PN.Trim();
            List[List.Count() - 1].PV = PV;
            return List[List.Count() - 1];
        }

        public UInt64 Session_Id;                       // Oturum id`si. Sunucu ile başlatılan her işlemde bir oturum id`si bulunur.
        public HttpMessageType Message_Type;            // Mesaj türü. Sunucuya gönderilen her paketin bir mesaj türü vardır.
        public HttpCompletedType Completed_Type;        // Completed komutunda kullanılır. Tamamlanan işlemin türünü bildirir.
        public UInt64 Process_Id;                       // Sunucu tarafından oluşturulan bir kod`dur. Completed mesajı ile yapılan işlemin durumu bildirilirken tekrar sunucuya gönderilir.
        public UInt64 SoftVer;                          // Cihazdaki yazılımın versiyonu.
        public bool Multi_User;                         // Bu alan true ise cihaza toplu şekilde kullanıcı gönderilebilir.
        public UInt32 Device_Id;                        // Cihaz id`si. Sunucunun hangi cihaz ile çalıştığını anlaması için cihaz id gönderilir.
        public Int32 Reader_Num;                        // İşlem yapılan okuyucu nosu.
        public Int32 Wiegand_Num;                       // İşlem yapılan okuyucunun, işlem yapılan wiegand nosu.
        public Int32 Reader_Count;                      // Cihazın okuyucu portu sayısı.
        public ScreenInfo Screen;                       // Cihazın kullandığı ekran modeli.
        public UInt64 Card_Id;                          // Okutulan kartın bilgisi.
        public Int64 Password;                          // Kullanıcının girmiş olduğu şifre.
        public Int64 Registration_Number;               // İşlem yapan kullanıcının kayıt/sicil nosu.
        public UInt64 Written_User_Count;               // JSON->"User" ile cihaza yazılan kullanıcı sayısı.
        public UInt64 Written_User_Forbidden_Count;     // JSON->"User_Forbidden" ile cihaza yazılan kullanıcı sayısı.
        public UInt64 Written_User_Unforbidden_Count;   // JSON->"User_Unforbidden" ile cihaza yazılan kullanıcı sayısı.
        public UInt64 User_Count;                       // Cihazdaki kullanıcı sayısı.
        public UInt64 Log_Count;                        // Cihazdaki log sayısı.
        public LogEvent Log_Event;                      // Log türü.
        public DateTime Log_Time;                       // İşlem zamanı.
        public RFTagType Tag_Type;                      // Okutulan kartın türü. DESFire veya Mifare klasik..vb.
        public Int32 Data_Count;                        // Data parametresi ile gelen verinin uzunluğu.
        public byte[]? Data;                            // Gelen veri.
        public GroupAccessZones Access_Zones;           // Grup erişim bölgeleri.
        public SensorSignals Sensors;                   // Sensörlerin şu an ki sinyal verileri.
        public HttpParameters User_Params;              // URL içinde kullanıcı tarafından tanımlanmış özel parametreler.
        public List<HttpParam> List;
    }

    public enum RFTagType
    {
        None = 0,
        TagUltralight = 1,
        Tag1K = 2,
        Tag4K = 3,
        TagDESFire = 4,
        TagProx = 5,
        TagEM = 6,
        TagMifarePlus = 7,
        TagSmartMX = 8,
        TagSmartMX_7UID = 9,
        TagMifareMini = 10,
        UHFCard = 11,
        Tag34bits = 12,
        Tag35bits = 13,
        Tag37bits = 14,
        Tag40bits = 15,
        Tag42bits = 16,
        Tag48bits = 17,
        Tag58bits = 18,
        Tag1K_7UID = 19,
        Tag4K_7UID = 20,
        TagMifarePlus_7UID = 21
    };

    /* Geçiş sırasında oluşan olaylar. */
    public enum LogEvent
    {
        AccessSuccessful = 0,    // Geçiş başarılı.
        UserInside = 1,    // Kullanıcı içeri girdi.
        UserOutside = 2,    // Kullanıcı dışarı çıktı.
        UnauthorizedAccess = 3,    // Yetkisiz kullanıcı erişimi.
        UnauthorizedEntry = 4,    // Yetkisiz kullanıcı girişi.
        UnauthorizedExit = 5,    // Yetkisiz kullanıcı çıkışı.
        OutOfTimeZone = 6,    // Zaman kuşağının dışında geçiş.
        TimeLimited = 7,    // Zaman kısıtlaması.
        CreditDone = 8,    // Kontör bitti.
        PasswordError = 9,    // Şifre hatalı girilmiş.
        AntiPassbackEntry = 10,   // Anti-Passback kullanılırken uygun bir şekil de giriş yapmış.
        AntiPassbackExit = 11,   // Anti-Passback kullanılırken uygun bir şekil de çıkış yapmış.
        TryingExitWithoutEntry = 12,   // Anti-Passback kullanıldığı halde girmeden çıkış yapılmaya çalışılmış. (Önceden izinsiz girmiş)
        TryingEntryWithoutExit = 13,   // Anti-Passback kullanıldığı halde çıkmadan giriş yapılmaya çalışılmış. (Giren birinin kartı ile tekrar girmeye çalışmış)
        ForbiddenDoor = 14,   // Kapıdan geçiş yasak.
        DoorOpenRemained = 15,   // Kapı açık kalmış. Geçiş yapıldıktan sonra kapı kapatılmamış.
        UnauthorizedAccessToSubgroup = 16,   // Alt gruba yetkisiz erişim. Parent okuyucudan giriş yapmamış.
        DoorForciblyOpened = 17,   // Kapı zorla açılmış.
        CapacityIsFull = 18,   // Erişim bölgesine giriş limiti dolmuş.
        DoorSensorActive = 19,   // Kapı sensörü aktif. (Kapı açıldı)
        DoorSensorPassive = 20,   // Kapı sensörü pasif oldu. (Kapı kapandı)
        DoorExitButton = 21,   // Kapının exit butonuna basılarak geçiş.
        EmergencyExitButton = 22,   // Acil çıkış butonuna basılmış.
        FireSensor = 23,   // Yangın sensöründen sinyal gelmiş.
        AlarmSensor = 24,   // Alarm sensöründen sinyal gelmiş.
        TamperSensor = 25,   // Tamper sensöründen sinyal gelmiş.
        ExtraSensor1 = 26,   // Yangın, Gaz, Hareket..vb sensörlerden gelen sinyaller.
        ExtraSensor2 = 27,   // Yangın, Gaz, Hareket..vb sensörlerden gelen sinyaller.
        GDMNoTransition = 28,   // Kapıya başarı ile erişmiş ancak turnike veya kapıdan geçmemiş.
        Disconnect = 29,   // Cihaz ile bağlantı koptu.
        Connect = 30,   // Cihaz ile bağlantı tekrar sağlandı.
        SystemStart = 31,   // Cihaz açıldığı zaman bu olay log olarak kaydedilir.
        PGMLimited = 32,   // PGM kısıtlamasına takıldı. PGM Grubundan başka kişiler içeri girdiğinden bu kullanıcı şimdilik giremez.
        ForbiddenDay = 33,   // Kapıdan geçiş bugün yasak.
        FinishedAccessToTimePeriod = 34,   // Zaman dilimindeki geçiş hakkı bitmiş.
        FinishedToDailyAccess = 35,   // Günlük geçiş hakkı bitmiş.
        LifetimeExpired = 36,   // Okuyucu kullanım süresi bitmiş.
        FinishedAccessToLifetime = 37,   // Okuyucuyu kullanım süresi içindeki geçiş hakkı bitmiş.
        CounterSensor1In = 38,   // Grup erişim modunda, okuyucunun 1.sensöründen giriş sinyali alındı.
        CounterSensor1Out = 39,   // Grup erişim modunda, okuyucunun 1.sensöründen çıkış sinyali alındı.
        CounterSensor2In = 40,   // Grup erişim modunda, okuyucunun 2.sensöründen giriş sinyali alındı.
        CounterSensor2Out = 41,   // Grup erişim modunda, okuyucunun 2.sensöründen çıkış sinyali alındı.
        CounterSensorExitIn = 42,   // Grup erişim modunda, okuyucunun exit sensöründen giriş sinyali alındı.
        CounterSensorExitOut = 43,   // Grup erişim modunda, okuyucunun exit sensöründen çıkış sinyali alındı.
        ClosedTheForciblyOpenedDoor = 44,   // Zorla açılan kapı kapatıldı.
        ClosedTheOpenRemainedDoor = 45,   // Açık kalan kapı kapatıldı.
        DoorAlarmSensorActive = 46,   // Kapı alarm sensörü aktif oldu.
        DoorAlarmSensorPassive = 47,    // Kapı alarm sensörü pasif oldu.
        RTCError = 48,    // Cihaz saatinde sorun var.
        PassedWithCardAndPassword = 49, // Kart ve şifre ile geçti.
        PassedWithPassword = 50  // Sadece şifre ile geçti.
    }

    /* Cihaz ekran modelleri. */
    public enum ScreenModel
    {
        None = 0,
        TextLCD = 1,
        GLCD = 2,
        TFT = 3
    };

    public class ScreenInfo
    {
        public ScreenInfo()
        {
            Model = ScreenModel.None;
            Width = 0;
            Height = 0;
        }

        public ScreenModel Model;
        public UInt32 Width;
        public UInt32 Height;
    }

    /* Bir erişim bölgesine ait bilgiler. */
    public class GroupAccessZone
    {
        public GroupAccessZone()
        {
            Capacity = 0; // Kapasite. (24 bits)
            AccessCount = 0; // Yapılan geçiş sayısı. (24 bits)
            Group_Id = 0; // Grup id`si. (16 bits) (Okuyuculara verilen grup id`si veya ağdaki başka bir cihazdaki okuyucunun grup id`si)
        }

        public UInt32 Group_Id; // Grup id`si. (16 bits) (Okuyuculara verilen grup id`si veya ağdaki başka bir cihazdaki okuyucunun grup id`si)
        public UInt32 Capacity; // Kapasite. (24 bits)
        public UInt32 AccessCount; // Yapılan geçiş sayısı. (24 bits)
    }

    /* Grup erişim bölgeleri. */
    public class GroupAccessZones : List<GroupAccessZone>
    {
        public GroupAccessZones()
        {
            Enable = false;

            for (int i = 0; i < GROUP_ACCESS_ZONES_COUNT; i++)
                Add(new GroupAccessZone());
        }

        public void Decode(string? S)
        {
            string[] Number_List;
            Int16 Number_Index = 0;

            if (String.IsNullOrEmpty(S) == false)
            {
                Number_List = S.Split(',');
                Enable = (Get_Number(Number_List, ref Number_Index) != 0);

                for (int i = 0; i < Count; i++)
                {
                    this[i].Group_Id = Get_Number(Number_List, ref Number_Index);
                    this[i].Capacity = Get_Number(Number_List, ref Number_Index);
                    this[i].AccessCount = Get_Number(Number_List, ref Number_Index);
                }
            }
        }

        private UInt32 Get_Number(string[] Number_List, ref Int16 Number_Index)
        {
            if (Number_Index < Number_List.Count())
                return Convert.ToUInt32(Number_List[Number_Index++]);
            else
                return 0;
        }

        public bool Enable; // Grup erişim bölgeleri modu. false= Devredışı, true= Etkin.
        private const UInt32 GROUP_ACCESS_ZONES_COUNT = 16; // Liste uzunluğu. Cihazın desteklediği bölge sayısı.

    }
}
