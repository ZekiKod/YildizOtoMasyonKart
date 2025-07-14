/*
 * Author: İstanbul Yazılım Elektronik Sanayi
 */

using System.ComponentModel.Design;
using System.Text;
using static ASPNetCore_WebAccess.UserLib;

namespace ASPNetCore_WebAccess
{
    public class WebAccessRun
    {
        public WebAccessRun(HttpContext Context)
        {
            /* Password     : Şifreli işlemler`de kullanılır. Cihaza verilen password değeri ile aynı olmalıdır.
             * Crypto_Enable: Cihaza gönderilecek olan JSON komutlarının kriptolu şekilde gönderilmesini sağlar.
             *                Değeri false geçilirse cihaza gönderilen veriler görülebilir, okunabilir şekildedir.
             *
             * DİKKAT!!
             * ------------
             *                Bu uygulamada varsayılan olarak kriptosuz kullanım esas alınmıştır.
             *                Güvenli bir iletişim için cihazınıza şifre verip ardından kriptolu iletişimi etkinleştirin.
             *                Cihazınıza verdiğiniz şifre değerini burada kullanın ve Crypto_Enable paramteresini true geçin.
             *                Örnek Kullanım;
             *                  WebAccess = new WebAccess("ABC_1234", true, Context);
            */
            WebAccess = new WebAccess("", false, Context);

            /* Cihaza Yanıt İşlemleri */
            if (WebAccess.Is_Session_Valid(WebAccess.Params.Session_Id)) // Session id geçerli ise
            {
                // Cihazın çalışma şekliyle ilgili ayarları yap.
                Set_Device();

                // DESFire ve mifare kartlar ile yapılan okuma/yazma işlemlerinde kullanılan veri türü.
                // WebAccess.FileDataType File_Data_Type = WebAccess.FileDataType.Base64;

                /* Gelen mesajın türüne göre yanıt işlemi gerçekleştir. */
                switch (WebAccess.Params.Message_Type)
                {
                    case HttpMessageType.Read_Card: // Kart okutuldu ise
                        {
                            /* Kullanıcı kart okutunca bu mesaj gelir.
                             * Röle çekme, ekrana mesaj yazdırma ve ses sinyalleri üreterek online geçiş kontrol işlemlerinizi burada yapabilirsiniz. */

                            if (WebAccess.IsSuccessEvent(WebAccess.Params.Log_Event) == true) // Başarılı bir geçiş ise
                                if (WebAccess.Params.Reader_Num > 0 && WebAccess.Params.Reader_Num <= WebAccess.Params.Reader_Count && ((WebAccess.Params.Card_Id > 0 && WebAccess.Params.Card_Id < WebAccess.Empty_Card) || WebAccess.Params.Data_Count > 0)) // Geçerli bir kart okutulmuş ise
                                    if (Online_Access_Control()) // Geçiş yetkisi varsa.
                                    {
                                        
                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("Read_Card ->     Reader Count: " + WebAccess.Params.Reader_Count + ",    Time : " + DateTime.Now.ToString());
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("     - Device Id  : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("     - Reader_Num : " + WebAccess.Params.Reader_Num);
                                        Console.WriteLine("     - Card_Id    : " + WebAccess.Params.Card_Id);
                                        Console.WriteLine("     - QR İçin");
                                        Console.WriteLine("     - Data Count : " + WebAccess.Params.Data_Count);
                                        Console.WriteLine("     - Data       : " + (WebAccess.Params.Data_Count > 0 && WebAccess.Params.Data != null ? Encoding.UTF8.GetString(WebAccess.Params.Data) : ""));
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");


                                        switch (WebAccess.Params.Screen.Model)
                                        {
                                            case ScreenModel.TFT:  // Ekran modeli TFT ise
                                                {
                                                    WebAccess.Send_Clear_Screen(0xFF00); // Ekranı yeşil renk ile temizle.
                                                    WebAccess.Send_Draw_Icon(IconType.Ok, 70, 40, 5, 0xFF00); // TFT Ekrana ikon çiz.
                                                    WebAccess.Send_Draw_Text(0, 170, "Hoş Geldiniz", 5, 28, 0x7F1F7F, 0xFF00, TextAlignment.Center, TextEncoding.UTF8); // TFT Ekrana mesaj yaz.
                                                    WebAccess.Send_Sound(WebAccess.Params.Reader_Num, SoundSignalType.CardOk); // İşlem yapılan okuyucuya ses sinyali vermesini söyle.
                                                    WebAccess.Send_Trigger_Relay(WebAccess.Params.Reader_Num, 1000); // İşlem yapılan okuyucunun rölesini 1000ms(1 saniye) tetikle.
                                                    break;
                                                }

                                            default: // Ekransız model ise
                                                {
                                                    WebAccess.Send_Sound(WebAccess.Params.Reader_Num, SoundSignalType.CardOk); // İşlem yapılan okuyucuya ses sinyali vermesini söyle.
                                                    WebAccess.Send_Trigger_Relay(WebAccess.Params.Reader_Num, 1000); // İşlem yapılan okuyucunun rölesini 1000ms(1 saniye) tetikle.
                                                    break;
                                                }
                                        }
                                    }
                                    else // Kişinin geçiş yetkisi yok.
                                    {
                                        switch (WebAccess.Params.Screen.Model)
                                        {
                                            case ScreenModel.TFT:  // Ekran modeli TFT ise
                                                {
                                                    WebAccess.Send_Clear_Screen(0xFF0000); // Ekranı kırmızı renk ile temizle.
                                                    WebAccess.Send_Draw_Icon(IconType.Error, 70, 40, 5, 0xFF0000); // TFT Ekrana hata ikonu çiz.
                                                    WebAccess.Send_Draw_Text(0, 170, "Yetkisiz Kart", 5, 28, 0xFFFFFF, 0xFF0000, TextAlignment.Center, TextEncoding.UTF8); // TFT Ekrana mesaj yaz.
                                                    WebAccess.Send_Sound(WebAccess.Params.Reader_Num, SoundSignalType.CardError); // İşlem yapılan okuyucuya ses sinyali vermesini söyle.
                                                    break;
                                                }

                                            default: // Ekransız model ise
                                                {
                                                    WebAccess.Send_Sound(WebAccess.Params.Reader_Num, SoundSignalType.CardError); // İşlem yapılan okuyucuya ses sinyali vermesini söyle.
                                                    break;
                                                }
                                        }
                                    }

                            break;
                        }

                    case HttpMessageType.Completed: // Son yapılan kayıt işlemi başarıyla tamamlandı ise (Yeni bir kişi eklemek gibi)
                        {
                            /* Son yapılan kayıt işleminin başarıyla tamamlandığını bildirir. (Yeni bir kişi eklemek gibi)
                             * Eğer, cihaza bir kullanıcı eklediyseniz, eklenen kullanıcının başarıyla cihaza tanıtıldığından emin olmuş olursunuz.
                             * Bu durumda veritabanınızda değişiklik yaparak kullanıcının her seferinde tekrar tekrar cihaza gönderilmesini engelleyebilirsiniz.
                             *
                             * Ardından bu cihaza gönderilecek kullanıcı varsa sıradakini hemen burada gönderebilirsiniz.
                             */

                            // Tamamlanan işlemin türü.
                            switch (WebAccess.Params.Completed_Type)
                            {
                                case HttpCompletedType.User_Inserted: // Kullanıcı eklendi.
                                case HttpCompletedType.User_Updated: // Var olan kullanıcı güncellendi.
                                    {
                                        /* Cihaza gönderilecek yeni kullanıcı varsa burada gönderebilirsiniz. */

                                        /*
                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("Completed ->   User Count : " + WebAccess.Params.User_Count + ",  Reader Count: " + WebAccess.Params.Reader_Count + ",  Log Count: " + WebAccess.Params.Log_Count + ",  Time : " + DateTime.Now.ToString());
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("     - Device Id                     : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("     - Process Id                    : " + WebAccess.Params.Process_Id);
                                        Console.WriteLine("     - Card Id                       : " + WebAccess.Params.Card_Id);
                                        Console.WriteLine("     - Multi User                    : " + WebAccess.Params.Multi_User);
                                        Console.WriteLine("     - Written User Count            : " + WebAccess.Params.Written_User_Count);
                                        Console.WriteLine("     - Written User Forbidden Count  : " + WebAccess.Params.Written_User_Forbidden_Count);
                                        Console.WriteLine("     - Written User Unforbidden Count: " + WebAccess.Params.Written_User_Unforbidden_Count);
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");

                                        // Gönderilmeyi bekleyen kullanıcıları cihaza gönder. Buradaki işlemler cihaza kullanıcı gönderilmesine örnek olması içindir.
                                        Send_Users();
                                        // */

                                        break;
                                    }

                                case HttpCompletedType.User_Capacity_Reached: // Cihaz kullanıcı kapasitesi dolmuş.
                                    {
                                        /* Cihazın kullanıcı kapasitesi dolduğundan yollanan kişi kaydedilemedi.. */

                                        /*
                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("User_Capacity_Reached ->   User Count : " + WebAccess.Params.User_Count + ",  Reader Count: " + WebAccess.Params.Reader_Count + ",  Log Count: " + WebAccess.Params.Log_Count + ",  Time : " + DateTime.Now.ToString());
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("     - Device Id                     : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("     - Process Id                    : " + WebAccess.Params.Process_Id);
                                        Console.WriteLine("     - Card Id                       : " + WebAccess.Params.Card_Id);
                                        Console.WriteLine("     - Multi User                    : " + WebAccess.Params.Multi_User);
                                        Console.WriteLine("     - Written User Count            : " + WebAccess.Params.Written_User_Count);
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        // */

                                        break;
                                    }

                                case HttpCompletedType.User_Forbidden: // Kullanıcı yasaklandı.
                                    {
                                        /* Geçişi yasaklanacak olan başka kullanıcı varsa burada yasaklayabilirsiniz. */
                                        // WebAccess.Send_User_Forbidden(35478); // Kart id`si 35478 olan kullanıcının geçişini yasakla.
                                        break;
                                    }

                                case HttpCompletedType.User_Unforbidden: // Kullanıcının yasağı kaldırıldı.
                                    {
                                        /* Geçiş yasağı kaldırılacak olan başka kullanıcı varsa burada kaldırabilirsiniz. */
                                        // WebAccess.Send_User_Unforbidden(35478); // Kart id`si 35478 olan kullanıcının geçiş yasağını kaldır.
                                        break;
                                    }

                                case HttpCompletedType.User_Write_Data:  // Kullanıcı kartına veri yazıldı.
                                    {
                                        /* DESFire veya mifare karta veri yazıldı. */

                                        /*
                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("User_Write_Data ->     Device Id : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        // */

                                        /*
                                        if (WebAccess.Params.Card_Id > 0 && WebAccess.Params.Card_Id < WebAccess.Empty_Card) // Cihaz kartı görüyor ise
                                            if (Continue_User_Write_Data(WebAccess.Params.Process_Id)) // Yazmaya devam edilecek ise
                                            {
                                                // Bu satır örnektir. Process_Id ile bildirilen son işleme ait bilgileri veritabanından yükleyip sonraki yazılacak bilgileri tekrar karta yazmak için kullanabilirsiniz.
                                                Load_User_From_DB(WebAccess.Params.Process_Id);

                                                WebAccess.Send_Write_Card_Data(1234, 0, Sector_No, System.Text.Encoding.UTF8.GetBytes("< Log Time: " + DateTime.Now.ToString() + ">   "), File_Data_Type);
                                            }

                                        // Bu satır örnektir. Process_Id ile bildirilen son işlem başarıyla tamamlandığı için bu görevi listeden siliyoruz.
                                        Delete_Process(WebAccess.Params.Process_Id);
                                        */

                                        break;
                                    }

                                case HttpCompletedType.User_Read_Data:  // Kullanıcı kartından veri okundu.
                                    {
                                        /* DESFire veya mifare karttan veri okundu. */

                                        /*
                                        string S = "";
                                        if (WebAccess.Params.Data != null)
                                            S = Encoding.UTF8.GetString(WebAccess.Params.Data);

                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("User_Read_Data");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("     - Device Id  : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("     - Data Count : " + WebAccess.Params.Data_Count);
                                        Console.WriteLine("     - Data       : " + S);

                                        switch (File_Data_Type)
                                        {
                                            case WebAccess.FileDataType.Base64:
                                                Console.WriteLine("     - Data       : " + (WebAccess.Params.Data_Count > 0 && WebAccess.Params.Data != null ? Encoding.UTF8.GetString(Convert.FromBase64String(S)) : ""));
                                                break;

                                            case WebAccess.FileDataType.Hex:
                                                Console.WriteLine("     - Data       : " + (WebAccess.Params.Data_Count > 0 && WebAccess.Params.Data != null ? Encoding.UTF8.GetString(Convert.FromHexString(S)) : ""));
                                                break;
                                        }
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        // */

                                        /*
                                        if (WebAccess.Params.Card_Id > 0 && WebAccess.Params.Card_Id < WebAccess.Empty_Card) // Cihaz kartı görüyor ise
                                            if (Continue_User_Read_Data(WebAccess.Params.Process_Id)) // Okumaya devam edilecek ise
                                            {
                                                // Bu satır örnektir. Process_Id ile bildirilen son işleme ait bilgileri veritabanından yükleyip sonraki okunacak alanları tekrar karttan okumak için kullanabilirsiniz.
                                                Load_User_From_DB(WebAccess.Params.Process_Id);

                                                WebAccess.Send_Read_Card_Data(5678, 0, 256, Sector_No, File_Data_Type);
                                            }

                                        // Bu satır örnektir. Process_Id ile bildirilen son işlem başarıyla tamamlandığı için bu görevi listeden siliyoruz.
                                        Delete_Process(WebAccess.Params.Process_Id);
                                        */

                                        break;
                                    }

                                case HttpCompletedType.User_Write_Data_Error:  // Kullanıcı kartına veri yazma hatası.
                                    {
                                        /* DESFire veya mifare karta veri yazılırken hata oluştu. */

                                        /*
                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("User_Write_Data_Error ->     Device Id : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        // */

                                        /*
                                        if (WebAccess.Params.Card_Id > 0 && WebAccess.Params.Card_Id < WebAccess.Empty_Card) // Cihaz kartı görüyor ise
                                            if (Continue_User_Write_Data(WebAccess.Params.Process_Id)) // Yazmaya devam edilecek ise
                                            {
                                                // Bu satır örnektir. Process_Id ile bildirilen son işleme ait bilgileri veritabanından yükleyip sonraki yazılacak bilgileri tekrar karta yazmak için kullanabilirsiniz.
                                                Load_User_From_DB(WebAccess.Params.Process_Id);

                                                WebAccess.Send_Write_Card_Data(1234, 0, Sector_No, System.Text.Encoding.UTF8.GetBytes("< Log Time: " + DateTime.Now.ToString() + ">   "), File_Data_Type);
                                            }

                                        // Bu satır örnektir. Process_Id ile bildirilen son işlem başarıyla tamamlandığı için bu görevi listeden siliyoruz.
                                        Delete_Process(WebAccess.Params.Process_Id);
                                        */

                                        break;
                                    }

                                case HttpCompletedType.User_Read_Data_Error:  // Kullanıcı kartından veri okuma hatası.
                                    {
                                        /* DESFire veya mifare klasik karttan veri okunurken hata oluştu. */

                                        /*
                                        Console.WriteLine("");
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        Console.WriteLine("User_Read_Data_Error ->     Device Id : " + WebAccess.Params.Device_Id);
                                        Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                                        // */

                                        /*
                                        if (WebAccess.Params.Card_Id > 0 && WebAccess.Params.Card_Id < WebAccess.Empty_Card) // Cihaz kartı görüyor ise
                                            if (Continue_User_Read_Data(WebAccess.Params.Process_Id)) // Okumaya devam edilecek ise
                                            {
                                                // Bu satır örnektir. Process_Id ile bildirilen son işleme ait bilgileri veritabanından yükleyip sonraki okunacak alanları tekrar karttan okumak için kullanabilirsiniz.
                                                Load_User_From_DB(WebAccess.Params.Process_Id);

                                                WebAccess.Send_Read_Card_Data(5678, 0, 256, Sector_No, File_Data_Type);
                                            }

                                        // Bu satır örnektir. Process_Id ile bildirilen son işlem başarıyla tamamlandığı için bu görevi listeden siliyoruz.
                                        Delete_Process(WebAccess.Params.Process_Id);
                                        */

                                        break;
                                    }
                            }

                            break;
                        }

                    case HttpMessageType.I_Am_Here: // Buradayım mesajı ise
                        {
                            /* Cihaz hazır da beklediğini bildiriyor.
                             * Buradaki değerleri döndürmek zorunda değilsiniz. Örnek olarak verilmiştir. */

                            /*
                            WebAccess.Send_Erase_All_Logs();                    // Tüm log`ları sil.
                            WebAccess.Send_Log_Rollback(2000);                  // 2000 adet log kaydını geri al.
                            WebAccess.Send_Log_Rollforward(50);                 // İlk 50 adet log kaydını sil.

                            WebAccess.Send_Local_Sensor_Sensibility(100);       // Yangın, Acil Çıkış, Alarm ve Tamper Switch sensörlerinin sinyal hassasiyetini 100ms olarak ayarla.
                            WebAccess.Send_Door_Sensor_Sensibility(25);         // Reader portlarındaki S1, S2, ve Exit sensörlerinin sinyal hassasiyetini 25ms olarak ayarla.

                            WebAccess.Send_Erase_All_Users();                   // Tüm kullanıcıları sil.
                            WebAccess.Send_User_Forbidden(35478);               // Kart id`si 35478 olan kullanıcının geçişini tüm okuyucularda engelle.
                            WebAccess.Send_User_Unforbidden(35478);             // Kart id`si 35478 olan kullanıcının geçiş yasağını kaldır.
                            //*/

                            //*
                            Console.WriteLine("");
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                            Console.WriteLine("I_Am_Here ->   User Count : " + WebAccess.Params.User_Count + ",  Reader Count: " + WebAccess.Params.Reader_Count + ",  Log Count: " + WebAccess.Params.Log_Count + ",  Time : " + DateTime.Now.ToString());
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                            Console.WriteLine("     - Device Id  : " + WebAccess.Params.Device_Id);
                            Console.WriteLine("     - Multi User : " + WebAccess.Params.Multi_User);
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");

                            // Gönderilmeyi bekleyen kullanıcıları cihaza gönder. Buradaki işlemler cihaza kullanıcı gönderilmesine örnek olması içindir.
                            // Send_Users();

                            Console.WriteLine("End-> I_Am_Here ->   Device_Id : " + WebAccess.Params.Device_Id + ",  Time : " + DateTime.Now.ToString());
                            Console.WriteLine("------------------------------------------------------------------------------------------");
                            // */

                            break;
                        }

                    case HttpMessageType.Log_Record: // Log mesajı ise
                        {
                            /* Cihaz tarafından bir log(geçiş kaydı) gönderilmiş. Log`u veritabanına kaydetme işlemleri burada yapılır. */

                            //*
                            Console.WriteLine("");
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                            Console.WriteLine("Log_Record ->   User Count : " + WebAccess.Params.User_Count + ",  Reader Count: " + WebAccess.Params.Reader_Count + ",  Log Count: " + WebAccess.Params.Log_Count + ",  Time : " + DateTime.Now.ToString());
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                            Console.WriteLine("     - Device Id  : " + WebAccess.Params.Device_Id);
                            Console.WriteLine("     - Multi User : " + WebAccess.Params.Multi_User);
                            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");

                            // Gönderilmeyi bekleyen kullanıcıları cihaza gönder. Buradaki işlemler cihaza kullanıcı gönderilmesine örnek olması içindir.
                            // Send_Users();

                            Console.WriteLine("End-> Log_Record ->   Device_Id : " + WebAccess.Params.Device_Id + ",  Time : " + DateTime.Now.ToString());
                            Console.WriteLine("------------------------------------------------------------------------------------------");
                            // */

                            break;
                        }
                }
            }
        }

        private void Set_Device()
        {
            /* Çalışma şeklini ve davranış biçimini cihaza bildiren işlemleri buradaki gibi yapabilirsiniz. Buradaki kodlar sadece örnek olması içindir. */

            WebAccess.Send_I_Am_Here_Duration(15);              // Buradayım süresini değiştir.
            WebAccess.Send_Device_Time(DateTime.Now);           // Cihaz zamanını ayarla.

            // WebAccess.Session_Id_Mode(WebSessionIdMode.Random); // Session Id oluşturulurken izlenecek yolu bildirir. Random ise rastgele, DeviceId ise cihazın id`sinden Session_Id üretilir.
            // WebAccess.Check_Session_Id(true);                   // Session_Id doğrulamasını etkinleştirmek için kullanılır. Değeri false olursa session id paket içinde aranır ancak doğrulaması yapılmaz.
        }
        
        private bool Online_Access_Control()
        {
            // Bu fonksiyonda WebAccess.Params.Card_Id veya WebAccess.Params.Data parametresi kullanılarak yetkilendirme yapılır. Kişinin yetkisi varsa true döner.

            if (WebAccess.Params.Data_Count > 0) // QR okutulmuş ise
            {
                // WebAccess.Params.Data  parametresinde ki veriyi kullanın. Data alanı harf, sayı ve özel karakterler içerebilir.
            }
            else
            {
                // Card_Id  parametresinde ki veriyi kullan. Bu alan max 16 haneli bir sayı içerir.
            }

            return true;
        }

        private void Send_Users()
        {
            /* Bu metod cihaza kullanıcı gönderir.
             * Buradaki işlemler cihaza kullanıcı göndermeye örnek olması içindir.
             * Siz, cihaza gönderilecek kullanıcıları kendi veritabanınızdan listeleyip göndermelisiniz.
             * Bu işlem için "WebAccess.Params.Device_Id" parametresini kullanarak cihazı kullanan personelleri tespit etmelisiniz.
             */


            // Cihaza benzersiz bir işlem id`si gönder.
            // Bu değer sizin tarafınızdan belirlenecek olan bir unique id`dir ve cihaz tarafından "WebAccess.Params.Process_Id" parametresi ile tekrar geri gönderilecektir.
            WebAccess.Send_Process_Id((ulong)DateTime.Now.Ticks);

            if (WebAccess.Params.Multi_User) // Bu cihaz toplu kullanıcı ekleme özelliğini destekliyorsa
            {
                // Cihaza 10 tane personel gönder.
                // Buradaki kod sadece örnek olması için yazılmıştır.
                // Card_Id değerinin benzersiz(unique) olması için DateTime.Now.Ticks kullanılmıştır.
                // Kullanıcı adı ve sicil no alanları ise burada kullanılmamıştır.
                for (UInt64 Card_Id = 1; Card_Id < 10 + 1; Card_Id++)
                    WebAccess.Send(New_User(Card_Id + (ulong)DateTime.Now.Ticks, "", 0, WebAccess.Params.Reader_Count));
            }
            else // Cihaz toplu kullanıcı göndermeyi desteklemiyor.
            {
                // Tek kişi ekle. ->  Card_Id: Benzersiz(unique) olması için DateTime.Now.Ticks kullanılmıştır.
                WebAccess.Send(New_User((ulong)DateTime.Now.Ticks, "", 0, WebAccess.Params.Reader_Count));
            }
        }

        private string New_User(UInt64 Card_Id, string User_Name = "", UInt64 Registration_Number = 0, Int32 Device_Reader_Count = UserLib.MAX_READER_CAPACITY)
        {
            // Card_Id ile bildirilen kullanıcıyı cihaza ekler veya update eder.

            UserLib.UserReader Reader;
            UserLib.User User = new UserLib.User();

            /*
            // Korunması istenen geçiş bilgileri için buradaki ilgili alanları true yapın.
            User.Protected.Anti_Passback_Status = true; // Anti_Passback durum bilgisini koru.
            User.Protected.Credit = true; // Kontörü koru.
            User.Protected.Daily_Access_Count = true; // Günlük geçiş sayısını koru.
            User.Protected.LifeTime_Access_Count = true; // Okuyucuyu kullanma süresi içindeki geçiş sayısını koru.
            User.Protected.Last_Access_Time = true; // Son erişim zamanını koru.
            */

            User.Card_Info = Card_Id;
            User.User_Name = User_Name;
            User.Registration_Number = (Int64)Registration_Number;

            for (int i = 0; i < Device_Reader_Count; i++)
                User.Readers[i].Status.Enabled = true;


            //--------------------------------------------------------------------------------------------------------------------------

            Reader = User.Readers[0]; // 1`nolu reader`a konumlan. Listede ki ilk kayıt 0`dan başladığı için 1`nolu reader 0. sıradadır.

            // Eklenecek/Güncellenecek personel için 1.okuyucuyudan geçiş yapabilir.
            Reader.Status.Enabled = true;

            // Dakika türünden zaman kısıtlaması 10 dakika olsun. Personel bu okuyucudan geçiş yaptıktan sonra 10 dakika boyunca tekrar geçemesin.
            Reader.Time_Limit = 10;

            // Okuyucuyu kullanım şekli. Anti-Passback yapılması isteniyorsa "None" yerine "InPoint", "OutPoint" veya "FreeOutPoint" değerlerinden birini kullanılır.
            Reader.Status.Reader_Direction = UserLib.UserReader.ReaderDirection.InPoint;
            Reader.Status.Anti_Passback_Status = UserLib.AntiPassbackStatus.None; // Kullanıcının bu kapıdaki içeride/dışarıda durumu.

            // 1.Period geçiş saatleri ( 07:30 ~ 08:00 arasında maks. 1 geçiş hakkı. )
            Reader.Time_Zones.Period1.SetValue(new DateTime(1, 1, 1, 7, 30, 2), new DateTime(1, 1, 1, 8, 0, 0), 1);

            // 2.Period geçiş saatleri ( 12:00 ~ 13:00 arasında maks. 3 geçiş hakkı. )
            Reader.Time_Zones.Period2.SetValue(new DateTime(1, 1, 1, 12, 0, 4), new DateTime(1, 1, 1, 13, 0, 0), 3);

            //--------------------------------------------------------------------------------------------------------------------------

            Reader = User.Readers[1]; // 2`nolu reader`a konumlan. Listede ki ilk kayıt 0`dan başladığı için 2`nolu reader 1. sıradadır.

            // Eklenecek/Güncellenecek personel için 1.okuyucuyudan geçiş yapabilir.
            Reader.Status.Enabled = true;

            // Dakika türünden zaman kısıtlaması 3 dakika olsun. Personel bu okuyucudan geçiş yaptıktan sonra 3 dakika boyunca tekrar geçemesin.
            Reader.Time_Limit = 3;

            // Okuyucuyu kullanım şekli. Anti-Passback yapılması isteniyorsa "None" yerine "InPoint", "OutPoint" veya "FreeOutPoint" değerlerinden birini kullanılır.
            Reader.Status.Reader_Direction = UserLib.UserReader.ReaderDirection.OutPoint;
            Reader.Status.Anti_Passback_Status = UserLib.AntiPassbackStatus.None; // Kullanıcının bu kapıdaki içeride/dışarıda durumu.

            //--------------------------------------------------------------------------------------------------------------------------

            // Console.WriteLine(User.ToJSON(Device_Reader_Count));

            return User.ToJSON(Device_Reader_Count);
        }

        public WebAccess WebAccess;
    }
}
