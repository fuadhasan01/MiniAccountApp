using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountApp.Models;
using MiniAccountApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
namespace MiniAccountApp.Pages.Reports
{
    [Authorize(Roles = "Admin,Accountant,Viewer")]
    public class TrialBalanceModel : PageModel
    {
        private readonly ReportService _reportService;

        public TrialBalanceModel(ReportService reportService)
        {
            _reportService = reportService;
        }

        public List<TrialBalanceEntry> Entries { get; set; } = new();

        [BindProperty]
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);

        [BindProperty]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public void OnGet()
        {
            Entries = _reportService.GetTrialBalance(StartDate, EndDate);
        }

        public void OnPost()
        {
            Entries = _reportService.GetTrialBalance(StartDate, EndDate);
        }
    }
}
