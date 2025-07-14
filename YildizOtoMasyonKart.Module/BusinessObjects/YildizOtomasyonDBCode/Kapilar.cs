using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB
{

    public partial class Kapilar
    {
        public Kapilar(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
