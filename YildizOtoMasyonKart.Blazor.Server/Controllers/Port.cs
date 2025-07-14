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
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YildizOtoMasyonKart.Blazor.Server.Controllers
{
    public partial class Port : ViewController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Port(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
            {
                ASPNetCore_WebAccess.WebAccessRun WebAccess_Run = new ASPNetCore_WebAccess.WebAccessRun(_httpContextAccessor.HttpContext);
                var response = WebAccess_Run.WebAccess.GetResponse();
                // response ile işlemler...
            }
        }

        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
    }
}
