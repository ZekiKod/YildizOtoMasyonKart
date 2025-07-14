using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Linq;
using System.Collections.Generic;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB;

namespace YildizOtoMasyonKart.Blazor.Server.Controllers
{
    public partial class KasaViewController : ViewController
    {
        public KasaViewController()
        {
            SimpleAction updateKartOdemeAction = new SimpleAction(this, "UpdateKartOdemeAction", PredefinedCategory.Edit)
            {
                Caption = "Kart Ödeme Eşle",
                ImageName = "Action_Update"
            };
            updateKartOdemeAction.Execute += UpdateKartOdemeAction_Execute;
        }

        private void UpdateKartOdemeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace(typeof(KasaDetay));
            var currentKasa = (Kasa)View.CurrentObject;

            if (currentKasa != null)
            {
                var session = ((XPObjectSpace)objectSpace).Session;

                // Kasa nesnesini mevcut session'dan tekrar yükle
                currentKasa = session.GetObjectByKey<Kasa>(currentKasa.Oid);

                // Tüm KartOdeme kayıtlarını al ve tarihine göre gruplandır
                var kartOdemeGruplar = session.Query<KartOdeme>()
                    .GroupBy(k => k.Tarih.Date)
                    .ToList();

                foreach (var grup in kartOdemeGruplar)
                {
                    var tarih = grup.Key;
                    var toplamKrediKarti = grup.Where(k => k.KrediKarti).Sum(k => k.YatanTutar);
                    var toplamNakit = grup.Where(k => !k.KrediKarti).Sum(k => k.YatanTutar);

                    // Belirli bir tarih için mevcut KasaDetay kaydını bulun
                    var kasaDetay = session.FindObject<KasaDetay>(
                        CriteriaOperator.And(
                            new BinaryOperator("Tarih", tarih),
                            new BinaryOperator("Kasa", currentKasa)
                        )
                    );

                    if (kasaDetay == null)
                    {
                        // Yeni KasaDetay kaydı oluştur
                        kasaDetay = new KasaDetay(session)
                        {
                            Tarih = tarih,
                            Kasa = currentKasa,
                            KrediKartiToplam = toplamKrediKarti,
                            NakitToplam = toplamNakit
                        };
                    }
                    else
                    {
                        // Mevcut KasaDetay kaydını güncelle
                        kasaDetay.KrediKartiToplam = toplamKrediKarti;
                        kasaDetay.NakitToplam = toplamNakit;
                    }

                    session.Save(kasaDetay); // KasaDetay'ı kaydet
                }

                objectSpace.CommitChanges(); // Tüm değişiklikleri kaydet
            }
        }
    }
}
