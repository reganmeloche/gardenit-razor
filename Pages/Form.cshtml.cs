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
    public class FormViewModel : PageModel
    {
        public string Mode { get; set; }
        public string PageTitle { get; set; }
        private string buttonText = "Save";

        private readonly ILogger<FormViewModel> _logger;
        private readonly PlantService _service;

        [BindProperty]
        public PlantFormModel Plant { get; set; }

        public FormViewModel(ILogger<FormViewModel> logger, PlantService service)
        {
            _logger = logger;
            _service = service;
        }

        public async Task OnGetAsync(string id)
        {
            Plant = new PlantFormModel() {
                Id = Guid.NewGuid(),
                Name = "Test plant",
                Type = "Plant type",
                Notes = "these are some notes",
                DaysBetweenWatering = 3,
                PollPeriodMinutes = 5,
                HasDevice = false
            };
            Mode = "create";
            PageTitle = "New Plant";

            if (!String.IsNullOrEmpty(id)) {
                Mode = "edit";
                PageTitle = "Edit Plant";
                var plant = await _service.GetPlant(Guid.Parse(id));
                // TODO: need a check if doesn't exist
                Plant.Id = plant.Id;
                Plant.Name = plant.Name;
                Plant.Notes = plant.Notes;
                Plant.Type = plant.Type;
                Plant.DaysBetweenWatering = plant.DaysBetweenWatering;
                Plant.ImageName = plant.ImageName;
                Plant.HasDevice = plant.HasDevice;
                Plant.PollPeriodMinutes = plant.PollPeriodMinutes;
            }
        }

        public async Task<IActionResult> OnPostAsync(string mode)
        {
            // if (!ModelState.IsValid)
            // {
            //     return Page();
            // }
            if (mode == "create") {
                await _service.CreatePlant(Plant);
                return RedirectToPage("Garden");
            } else {
                await _service.UpdatePlant(Plant.Id, Plant);
                return RedirectToPage("Plant", new { id = Plant.Id });
            }
        }
    }
}

