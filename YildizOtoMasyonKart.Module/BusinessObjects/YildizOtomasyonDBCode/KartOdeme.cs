using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Linq;
using System.Diagnostics;
using DevExpress.Persistent.Base;

namespace YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB
{
    [DefaultClassOptions]
    public partial class KartOdeme : XPObject
    {
        public KartOdeme(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            Tarih = DateTime.Now;
            base.AfterConstruction();
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (!IsSaving || IsDeleted) return;

            try
            {
                // KartBilgisi güncellemesi
                UpdateKartBilgisi(Session, this);

                // KasaDetay güncellemesi
                //UpdateKasaDetay(Session, this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Hata: " + ex.Message);
                throw;
            }
        }

        private void UpdateKartBilgisi(Session session, KartOdeme kartOdeme)
        {
            if (kartOdeme.KartBilgisi != null)
            {
                decimal krtytn = kartOdeme.KartBilgisi.KartOdemes?.Where(x => !x.iade).Sum(x => x.YatanTutar) ?? 0;
                decimal gcstpl = (kartOdeme.KartBilgisi.GirisCikislars?.Where(x => !x.iade).Sum(x => x.Tutar) ?? 0) +
                                 (kartOdeme.KartBilgisi.SatilanUrunlers?.Where(x => !x.iade).Sum(x => x.Fiyat) ?? 0);

                // KartBakiye'yi güncelle
                kartOdeme.KartBilgisi.KartBakiye = krtytn - gcstpl;
            }
        }
    }
}
