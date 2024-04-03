﻿using Microsoft.AspNetCore.Mvc.Rendering;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.ViewModel
{
    public class Insp4ViewModel
    {
        public int CurrentStatusId { get; set; } // Hidden field
        public int Insp4Id { get; set; } // Hidden field
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        public int ReceivedQuantity { get; set; }
        public string PackageSize { get; set; }
        public string CustomerName { get; set; }
        public int PiecesPerBlank { get; set; }
        public string ProcessStatus { get; set; } // Hidden field

        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        public int Difference { get; set; } // Assuming this is an input. If it's calculated, it should be a double or decimal.

        // Text area for remarks
        public string Remarks { get; set; }

        //// Dropdown for status
        public Insp4ProcessStatus Status { get; set; }

        // Person in charge (PIC)
        public string CheckedBy { get; set; }

        public string ProcessCurrentStatus { get; set; }

    }
}

