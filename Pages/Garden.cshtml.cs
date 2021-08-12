using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using gardenit_razor.Data;

namespace gardenit_razor.Pages
{
    public class IndexViewModel : PageModel
    {
        public List<PlantSummary> Plants { get; set; }
        private readonly ILogger<IndexViewModel> _logger;
        private readonly PlantService _service;

        public IndexViewModel(ILogger<IndexViewModel> logger, PlantService service)
        {
            _logger = logger;
            _service = service;
        }

        public async Task<IActionResult> OnGet()
        {
            try {
                Plants = await _service.GetAll();
                return Page();
            } catch (UnauthorizedAccessException) {
                return RedirectToPage("Account/Login");
            }
        }
    }
}
