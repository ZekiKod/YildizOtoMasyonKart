using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp;

namespace YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB
{
    public partial class KasaDetay : XPObject
    {
        public KasaDetay(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();

            // Oturum açan kullanıcının bilgilerini al ve Giseci alanına ata
            if (SecuritySystem.CurrentUserId != null)
            {
                Giseci = Session.GetObjectByKey<PermissionPolicyUser>(SecuritySystem.CurrentUserId);
            }
        }
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            if (propertyName== "NakitToplam"|| propertyName == "KrediKartiToplam")
            {
                ToplamKazanc = NakitToplam + KrediKartiToplam;
            }
            base.OnChanged(propertyName, oldValue, newValue);
        }
        protected override void OnSaving()
        {
            base.OnSaving();

            if (Kasa != null && Kasa.Session != Session)
            {
                // Eğer Kasa nesnesi farklı bir session'a aitse, bu session'dan yeni bir Kasa nesnesi oluşturulmalı
                var existingKasa = Session.GetObjectByKey<Kasa>(Kasa.Oid);

                if (existingKasa != null)
                {
                    Kasa = existingKasa;
                }
                else
                {
                    // Eğer Kasa nesnesi bu session'da yoksa, bu session için yeni bir Kasa nesnesi oluşturun
                    Kasa = new Kasa(Session)
                    {
                        KasaAdi = Kasa.KasaAdi,
                        KrediKarti = Kasa.KrediKarti,
                        Nakit = Kasa.Nakit
                    };
                }
            }

            if (Kasa != null)
            {
                Kasa.KrediKarti = Kasa.KasaDetays?.Where(x => x.TeslimAlindi == false).Sum(x => x.KrediKartiToplam) ?? 0;
                Kasa.Nakit = Kasa.KasaDetays?.Where(x => x.TeslimAlindi == false).Sum(x => x.NakitToplam) ?? 0;
            }
        }
    }
}
