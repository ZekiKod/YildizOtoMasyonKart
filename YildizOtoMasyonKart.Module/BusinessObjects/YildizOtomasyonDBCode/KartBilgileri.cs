using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;

namespace YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB
{
    [DefaultClassOptions]
    public partial class KartBilgileri
    {
        public KartBilgileri(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Tarih = DateTime.Now;

            // "Geçiş Kartı" ürünü Urunler tablosunda aranıyor
            Urunler gecisKartiUrunu = Session.FindObject<Urunler>(
                CriteriaOperator.Parse("UrunAdi == ?", "Geçiş Kartı")
            );

            // Eğer "Geçiş Kartı" ürünü yoksa, yeni bir ürün oluşturuluyor
            if (gecisKartiUrunu == null)
            {
                gecisKartiUrunu = new Urunler(Session)
                {
                    UrunAdi = "Geçiş Kartı",
                    Barkodu = Guid.NewGuid().ToString().Substring(0, 5), // Rastgele bir barkod oluşturulabilir
                    Kategori = null, // Gerekirse bir kategori set edilebilir
                    Fiyat = 20m
                };
                gecisKartiUrunu.Save(); // Ürünü veritabanına kaydet
            }

            // Yeni bir SatilanUrunler nesnesi oluşturuluyor
            SatilanUrunler satilanUrun = new SatilanUrunler(Session)
            {
                Tarih = DateTime.Now,
                Barkodu = gecisKartiUrunu.Barkodu,
                UrunAdi = gecisKartiUrunu.UrunAdi,
                Kategori = gecisKartiUrunu.Kategori,
                Fiyat = gecisKartiUrunu.Fiyat,
                KartBilgisi = this,
                iade = false
            };

            // SatilanUrunlers koleksiyonuna ekleniyor
            SatilanUrunlers.Add(satilanUrun);
        }
    }
}
