using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WIPSystem.Web.Models
{
    public class BreakingViewModel
    {
        // Other properties...

        public int BreakingId { get; set; }
        // Hidden fields
        public int CurrentStatusId { get; set; }
        public string ProcessStatus { get; set; }

        // Basic properties
        public string PartNo { get; set; }
        public string LotNo { get; set; }
        public int ReceivedQuantity { get; set; }
        public string PackageSize { get; set; }
        public string CustomerName { get; set; }
        public int PiecesPerBlank { get; set; }

        // Dropdown for selecting the machine
        public int SelectedMachineId { get; set; }
        public string SelectedMachineOption { get; set; }
        public IEnumerable<SelectListItem> MachineOptions { get; set; }

        // Machine details
        public int MachineId { get; set; }
        public string MachineName { get; set; }

        [Required(ErrorMessage = "Please select a machine type")]
        [Display(Name = "Machine Type")]
        public string MachineType { get; set; }

        // Dropdown for selecting machine types
        public IEnumerable<SelectListItem> MachineTypes { get; set; }

        // Start and end times
        public DateTime MachineStartTime { get; set; }
        public DateTime MachineEndTime { get; set; }

        // Calculated or entered quantities
        public double Hour { get; set; }
        public int OutputQty { get; set; }
        public int RejectQty { get; set; }
        public int ChippingQty { get; set; }
        public int SheetBreakQty { get; set; }
        public int CrackQty { get; set; }
        public int OthersQty { get; set; }
        public int DifferencesQty { get; set; }
        public string FirstBreak { get; set; }
        public string SecondBreak { get; set; }
        public string TargetOne { get; set; }
        public string TargetTwo { get; set; }

        // Text area for remarks
        public string Remarks { get; set; }

        // Dropdown for status
        public BreakingProcessStatus Status { get; set; }

        // Person in charge (PIC)
        public string CheckedBy { get; set; }

        // Additional property
        public string ProcessCurrentStatus { get; set; }

        public BreakingViewModel()
        {
            MachineOptions = new List<SelectListItem>();
            MachineTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Auto", Value = "Auto" },
                new SelectListItem { Text = "Manual", Value = "Manual" }
            };
        }
    }
}
