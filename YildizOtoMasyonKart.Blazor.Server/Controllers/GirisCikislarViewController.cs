using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YildizOtomasyon.Module.BusinessObjects.YildizOtomasyonDB;

namespace YildizOtoMasyonKart.Blazor.Server.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class GirisCikislarViewController : ViewController
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
     
        public GirisCikislarViewController()
        {
            InitializeComponent();
            // Target Object Type
            TargetObjectType = typeof(GirisCikislar);

            // Action oluşturma
            SimpleAction markIadeAction = new SimpleAction(this, "MarkIadeAction", PredefinedCategory.Edit)
            {
                Caption = "İade Olarak İşaretle",
                ImageName = "Action_SimpleAction"
            };

            // Action tetiklendiğinde çalışacak metodun atanması
            markIadeAction.Execute += MarkIadeAction_Execute;
        }
        private void MarkIadeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // ObjectSpace kullanarak GirisCikislar tablosunu sorgulama
            var objectSpace = Application.CreateObjectSpace(typeof(GirisCikislar));
            var girisCikislarList = objectSpace.GetObjects<GirisCikislar>()
                .Where(gc => gc.Tarih >= DateTime.Now.AddHours(-2) && !gc.iade)
                .ToList();

            // 2 saatten eski olan kayıtları iade olarak işaretleme
            foreach (var girisCikis in girisCikislarList)
            {
                girisCikis.iade = true;
            }

            // Değişiklikleri kaydetme
            objectSpace.CommitChanges();

            // Bilgilendirme mesajı
            Application.ShowViewStrategy.ShowMessage($"{girisCikislarList.Count} kayıt iade olarak işaretlendi.", InformationType.Success, 4000, InformationPosition.Bottom);
        }
    }
}
