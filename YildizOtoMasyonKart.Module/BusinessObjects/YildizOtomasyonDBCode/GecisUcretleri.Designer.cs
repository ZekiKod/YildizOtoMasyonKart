﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB
{

    public partial class GecisUcretleri : XPObject
    {
        DateTime fTarih;
        public DateTime Tarih
        {
            get { return fTarih; }
            set { SetPropertyValue<DateTime>(nameof(Tarih), ref fTarih, value); }
        }
        decimal fUcret;
        public decimal Ucret
        {
            get { return fUcret; }
            set { SetPropertyValue<decimal>(nameof(Ucret), ref fUcret, value); }
        }
    }

}
