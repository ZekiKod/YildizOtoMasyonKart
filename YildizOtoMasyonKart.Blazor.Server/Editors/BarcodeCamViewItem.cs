using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor; // IComponentContentHolder için
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using Microsoft.AspNetCore.Components; // RenderFragment ve builder API için
using System;
// BarcodeCamInterface component'inin bulunduğu namespace'i ekleyin
using YildizOtoMasyonKart.Blazor.Server.RazorComponents; // VEYA component'in bulunduğu doğru namespace

namespace YildizOtoMasyonKart.Module.Blazor.Editors // veya .Blazor.Server.Editors
{
    public interface IModelBarcodeCamViewItem : IModelViewItem { }

    [ViewItem(typeof(IModelBarcodeCamViewItem))]
    public class BarcodeCamViewItem : ViewItem
    {
        public class DxButtonHolder : IComponentContentHolder
        {
            private readonly View currentView; // Saklanan XAF View'ı

            public DxButtonHolder(View _currentView)
            {
                // XAF View'ını constructor'da alıp saklayalım
                this.currentView = _currentView;
            }

            // ComponentContent property'si RenderFragment döndürmeli
            RenderFragment IComponentContentHolder.ComponentContent => builder =>
            {
                // BarcodeCamInterface component'ini oluştur
                builder.OpenComponent<BarcodeCamInterface>(0);

                // BarcodeCamInterface'deki [Parameter] public View View { get; set; }
                // parametresine XAF View'ını ata.
                // Parametre adının "View" olduğundan emin olun.
                builder.AddAttribute(1, nameof(BarcodeCamInterface.View), this.currentView);

                // Component'i kapat
                builder.CloseComponent();
            };
        }

        public BarcodeCamViewItem(IModelViewItem model, Type objectType) : base(objectType, model.Id) { }

        // Kontrol olarak DxButtonHolder örneğini döndür
        protected override object CreateControlCore() => new DxButtonHolder(this.View);
    }
}