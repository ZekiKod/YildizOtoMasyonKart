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
    public partial class GirisCikislar
    {
        public GirisCikislar(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnSaving()
        {
            base.OnSaving();

            // Örneğin, KartBilgisi'nin null olup olmadığını kontrol edelim
            if (KartBilgileri != null)
            {
                decimal krtytn = KartBilgileri.KartOdemes?.Where(x => x.iade == false).Sum(x => x.YatanTutar) ?? 0;
                decimal gcstpl = (KartBilgileri.GirisCikislars?.Where(x => x.iade == false).Sum(x => x.Tutar) ?? 0) + (KartBilgileri.SatilanUrunlers?.Where(x => x.iade == false).Sum(x => x.Fiyat) ?? 0);

                KartBilgileri.KartBakiye = krtytn - gcstpl;

                
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

            if (KartBilgileri != null)
            {
                decimal krtytn = KartBilgileri.KartOdemes?.Where(x => x.iade == false).Sum(x => x.YatanTutar) ?? 0;
                decimal gcstpl = (KartBilgileri.GirisCikislars?.Where(x => x.iade == false).Sum(x => x.Tutar) ?? 0) + (KartBilgileri.SatilanUrunlers?.Where(x => x.iade == false).Sum(x => x.Fiyat) ?? 0);

                KartBilgileri.KartBakiye = krtytn - gcstpl;
            }
        }



    }

}
