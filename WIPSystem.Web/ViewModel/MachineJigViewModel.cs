using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class MachineJigViewModel
{
    // Add this property
    public int MachineId { get; set; }

    public IEnumerable<SelectListItem> Processes { get; set; }
    public int SelectedProcessId { get; set; }

    [Display(Name = "Machine Name")]
    public string MachineName { get; set; }

    [Display(Name = "Machine Number")]
    public string MachineNumber { get; set; }

    [Display(Name = "Machine Quantity")]
    public int MachineQuantity { get; set; }

    [Display(Name = "Jig Name")]
    public string JigName { get; set; }

    [Display(Name = "Jig Life Span")]
    public int JigLifeSpan { get; set; }
}
