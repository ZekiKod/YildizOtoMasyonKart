using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solution3.Module.Blazor.Editors
{
    public class ProductInvokeHelper
    {
        private Action<SearchItemProduct> action;

        public ProductInvokeHelper(Action<SearchItemProduct> action)
        {
            this.action = action;
        }
        [JSInvokable]
        public void SearchRecordCaller(SearchItemProduct param)
        {
            this.action.Invoke(param);
        }
    }

    public class ItemProduct
    {
        public Guid Oid { get; set; }
        public string DirectoryUrl { get; set; }
    }
    public class SearchItemProduct
    {
        public string Barcode { get; set; }
    }
}
