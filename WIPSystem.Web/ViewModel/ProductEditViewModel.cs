using WIPSystem.Web.Models; // Adjust to the namespace where Product and ProductProcessMapping are defined
using System.Collections.Generic;

namespace WIPSystem.Web.ViewModel
{
        public class ProductEditViewModel
        {
            public Product Product { get; set; }
            public List<ProductProcessMapping> ProductProcessMappings { get; set; }
            public List<Process> AvailableProcesses { get; set; } // You need to ensure that this is populated
        }
   

}
