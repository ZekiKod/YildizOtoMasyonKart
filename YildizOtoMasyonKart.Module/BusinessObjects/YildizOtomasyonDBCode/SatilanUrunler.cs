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
    public partial class SatilanUrunler
    {
        public SatilanUrunler(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnSaving()
        {
            base.OnSaving();

            // Örneğin, KartBilgisi'nin null olup olmadığını kontrol edelim
            if (KartBilgisi != null)
            {
                decimal krtytn = KartBilgisi.KartOdemes?.Where(x => x.iade == false).Sum(x => x.YatanTutar) ?? 0;
                decimal gcstpl = (KartBilgisi.GirisCikislars?.Where(x => x.iade == false).Sum(x => x.Tutar) ?? 0) + (KartBilgisi.SatilanUrunlers?.Where(x => x.iade == false).Sum(x => x.Fiyat) ?? 0);

                KartBilgisi.KartBakiye = krtytn - gcstpl;
            }
            else
            {
                // KartBilgisi null ise burada uygun bir işlem yapın, örneğin bir log kaydı
                // veya özel bir durum fırlatabilirsiniz.
            }
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();

            if (KartBilgisi != null)
            {
                decimal krtytn = KartBilgisi.KartOdemes?.Where(x => x.iade == false).Sum(x => x.YatanTutar) ?? 0;
                decimal gcstpl = (KartBilgisi.GirisCikislars?.Where(x => x.iade == false).Sum(x => x.Tutar) ?? 0) + (KartBilgisi.SatilanUrunlers?.Where(x => x.iade == false).Sum(x => x.Fiyat) ?? 0);

                KartBilgisi.KartBakiye = krtytn - gcstpl;
            }
        }

    }

}
