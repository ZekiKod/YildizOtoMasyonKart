// WebAccess_Lib/WebAccessRun.cs

using DevExpress.ExpressApp;
using DevExpress.Xpo;
using YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB;
using Microsoft.Extensions.Logging;
using DevExpress.Data.Filtering;
using System.Linq; // LINQ metodları için gerekli.

namespace ASPNetCore_WebAccess
{
    public class WebAccessRun
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;
        private readonly ILogger<WebAccessRun> _logger;
        public WebAccess WebAccess;

        public WebAccessRun(HttpContext context, IObjectSpaceFactory objectSpaceFactory, ILogger<WebAccessRun> logger)
        {
            _objectSpaceFactory = objectSpaceFactory;
            _logger = logger;
            WebAccess = new WebAccess("", false, context);

            if (!WebAccess.Is_Session_Valid(WebAccess.Params.Session_Id))
            {
                _logger.LogWarning("Geçersiz Session ID'li bir istek alındı: {SessionId}", WebAccess.Params.Session_Id);
                return;
            }

            switch (WebAccess.Params.Message_Type)
            {
                case HttpMessageType.Read_Card:
                    HandleReadCard();
                    break;
                case HttpMessageType.I_Am_Here:
                    HandleIAmHere();
                    break;
                case HttpMessageType.Log_Record:
                    HandleLogRecord();
                    break;
                case HttpMessageType.Completed:
                    _logger.LogInformation("Completed mesajı alındı. İşlem Tipi: {CompletedType}", WebAccess.Params.Completed_Type);
                    break;
            }
        }

        private void HandleReadCard()
        {
            _logger.LogInformation("Read_Card isteği alındı. Cihaz: {DeviceId}, Kart: {CardId}", WebAccess.Params.Device_Id, WebAccess.Params.Card_Id);

            using (var objectSpace = _objectSpaceFactory.CreateObjectSpace<KartBilgileri>())
            {
                string kartNo = WebAccess.Params.Card_Id.ToString().PadLeft(10, '0');
                var kartBilgisi = objectSpace.FirstOrDefault<KartBilgileri>(k => k.KartNo == kartNo);

                if (kartBilgisi == null)
                {
                    _logger.LogWarning("Tanımsız kart okutuldu: {KartNo}", kartNo);
                    DenyAccess("Tanimsiz Kart!", $"Kart No:\n{kartNo}");
                    return;
                }

                // HATA DÜZELTMESİ: FindObject ve FirstOrDefault kullanımı düzeltildi.
                var gecisUcreti = objectSpace.GetObjectsQuery<GecisUcretleri>().OrderByDescending(g => g.Tarih).FirstOrDefault()?.Ucret ?? 0m;

                decimal uygulanacakUcret = gecisUcreti;
                if (kartBilgisi.indirimli != null)
                {
                    uygulanacakUcret = gecisUcreti - ((gecisUcreti / 100m) * kartBilgisi.indirimli.indirimOrani);
                }

                if (kartBilgisi.SinirsizGecis)
                {
                    _logger.LogInformation("Yetki VERİLDİ (Sınırsız). Kart: {KartNo}", kartNo);
                    AllowAccess(kartBilgisi.AdiSoyadi, "Sinirsiz Gecis");
                }
                else if (kartBilgisi.KartBakiye >= uygulanacakUcret)
                {
                    _logger.LogInformation("Yetki VERİLDİ. Kart: {KartNo}", kartNo);
                    string bakiyeStr = $"Bakiye: {kartBilgisi.KartBakiye:C2}";
                    AllowAccess(kartBilgisi.AdiSoyadi, bakiyeStr);
                }
                else
                {
                    _logger.LogWarning("Yetki REDDEDİLDİ (Bakiye Yetersiz). Kart: {KartNo}", kartNo);
                    string bakiyeStr = $"Bakiye: {kartBilgisi.KartBakiye:C2}";
                    DenyAccess("Bakiye Yetersiz!", bakiyeStr);
                }
            }
        }

        private void HandleLogRecord()
        {
            if (WebAccess.Params.Log_Event == LogEvent.DoorSensorActive || WebAccess.Params.Log_Event == LogEvent.AccessSuccessful)
            {
                _logger.LogInformation("Başarılı geçiş teyidi alındı. Kart: {CardId}", WebAccess.Params.Card_Id);

                using (var objectSpace = _objectSpaceFactory.CreateObjectSpace<KartBilgileri>())
                {
                    string kartNo = WebAccess.Params.Card_Id.ToString().PadLeft(10, '0');
                    var kartBilgisi = objectSpace.FirstOrDefault<KartBilgileri>(k => k.KartNo == kartNo);

                    if (kartBilgisi == null)
                    {
                        _logger.LogError("KRİTİK HATA: Geçiş logu gelen kart veritabanında bulunamadı: {KartNo}", kartNo);
                        return;
                    }

                    if (!kartBilgisi.SinirsizGecis)
                    {
                        var gecisUcreti = objectSpace.GetObjectsQuery<GecisUcretleri>().OrderByDescending(g => g.Tarih).FirstOrDefault()?.Ucret ?? 0m;
                        decimal uygulanacakUcret = gecisUcreti;
                        if (kartBilgisi.indirimli != null)
                        {
                            uygulanacakUcret = gecisUcreti - ((gecisUcreti / 100m) * kartBilgisi.indirimli.indirimOrani);
                        }

                        // Bakiye düşme işlemi SADECE burada yapılır
                        kartBilgisi.KartBakiye -= uygulanacakUcret;

                        var giris = objectSpace.CreateObject<GirisCikislar>();
                        giris.Tarih = WebAccess.Params.Log_Time;
                        giris.Tutar = uygulanacakUcret;
                        giris.KartBilgileri = kartBilgisi;

                        objectSpace.CommitChanges();
                        _logger.LogInformation("Bakiye düşüldü ve geçiş kaydı oluşturuldu. Kart: {KartNo}, Yeni Bakiye: {YeniBakiye}", kartNo, kartBilgisi.KartBakiye);
                    }
                    else
                    {
                        // Sınırsız kartlar için sadece geçiş kaydı oluşturulur
                        var giris = objectSpace.CreateObject<GirisCikislar>();
                        giris.Tarih = WebAccess.Params.Log_Time;
                        giris.Tutar = 0; // Sınırsız olduğu için ücret 0 yazılabilir.
                        giris.KartBilgileri = kartBilgisi;
                        objectSpace.CommitChanges();
                        _logger.LogInformation("Sınırsız kart için geçiş kaydı oluşturuldu. Kart: {KartNo}", kartNo);
                    }
                }
            }
        }

        private void HandleIAmHere()
        {
            _logger.LogInformation("I_Am_Here mesajı alındı. Cihaz: {DeviceId}", WebAccess.Params.Device_Id);
            WebAccess.Send_Device_Time(DateTime.Now);
        }

        // Yardımcı Metotlar
        private void AllowAccess(string line1, string line2)
        {
            WebAccess.Send_Trigger_Relay(WebAccess.Params.Reader_Num, 1000);
            WebAccess.Send_Sound(WebAccess.Params.Reader_Num, SoundSignalType.CardOk);
            WebAccess.Send_Clear_Screen(0x00FF00); // Ekranı yeşil yap
            WebAccess.Send_Draw_Icon(IconType.Ok, 70, 40, 5, 0x00FF00);
            WebAccess.Send_Draw_Text(0, 150, line1, 5, 28, 0xFFFFFF, 0x00FF00, TextAlignment.Center);
            WebAccess.Send_Draw_Text(0, 190, line2, 5, 28, 0xFFFFFF, 0x00FF00, TextAlignment.Center);
        }

        private void DenyAccess(string line1, string line2)
        {
            WebAccess.Send_Sound(WebAccess.Params.Reader_Num, SoundSignalType.CardError);
            WebAccess.Send_Clear_Screen(0xFF0000); // Ekranı kırmızı yap
            WebAccess.Send_Draw_Icon(IconType.Error, 70, 40, 5, 0xFF0000);
            WebAccess.Send_Draw_Text(0, 150, line1, 5, 28, 0xFFFFFF, 0xFF0000, TextAlignment.Center);
            WebAccess.Send_Draw_Text(0, 190, line2, 5, 28, 0xFFFFFF, 0xFF0000, TextAlignment.Center);
        }
    }
}