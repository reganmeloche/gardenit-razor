using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using ChartJSCore.Models;
using ChartJSCore.Helpers;

using gardenit_razor.Data;

namespace gardenit_razor.Pages
{
    public class PlantViewModel : PageModel
    {
        private readonly ILogger<PlantViewModel> _logger;
        private readonly PlantService _service;
        public PlantSummary Plant { get; set; }

        public Chart MoistureChart { get; set; }

        public PlantViewModel(ILogger<PlantViewModel> logger, PlantService service)
        {
            _logger = logger;
            _service = service;
        }

        public async Task OnGetAsync(string id)
        {
            Plant = await _service.GetPlant(Guid.Parse(id));

            Plant.MoistureReadings = new List<MoistureReading>() {
                new MoistureReading() {
                    ReadDate = new DateTime(2021, 8, 9, 2, 7, 4),
                    Value = 333
                },
                new MoistureReading() {
                    ReadDate = new DateTime(2021, 8, 9, 4, 9, 4),
                    Value = 222
                },
                new MoistureReading() {
                    ReadDate = new DateTime(2021, 8, 9, 6, 27, 0),
                    Value = 319
                }
            };

            InitChart();
        }

        // public IActionResult OnPostDelete(string id) {
        //     _service.DeletePlant(Guid.Parse(id));
        //     return RedirectToPage("Index");
        // }

        public async Task<IActionResult> OnPostDeleteAsync(string id) {
            await _service.DeletePlant(Guid.Parse(id));
            return RedirectToPage("Garden");
        }

        public async Task<IActionResult> OnPostWaterAsync(string id, int seconds) {
            await _service.WaterPlant(Guid.Parse(id), seconds);
            return RedirectToPage("Plant", new { id });
        }

        public async Task<IActionResult> OnPostMoistureAsync(string id) {
            await _service.RequestMoistureReading(Guid.Parse(id));
            return RedirectToPage("Plant", new { id });
        }

        public async Task<IActionResult> OnPostActivateAsync(string id, bool activate) {
            await _service.PlantActivation(Guid.Parse(id), activate);
            return RedirectToPage("Plant", new { id });
        }

        private void InitChart() {
            MoistureChart = new Chart() {
                Type = Enums.ChartType.Line,
                Data = new ChartJSCore.Models.Data() {
                    Datasets = new List<Dataset>(),
                    Labels = new List<string>(),
                },
                Options = new Options() {
                    Responsive = true,
                    Title = new Title() {
                        Text = "Plant Readings",
                        Display = true
                    },
                    Tooltips = new ToolTip()
                    {
                        Mode = "nearest", // ??
                        Intersect = true
                    },
                    Hover = new Hover
                    {
                        Mode = "nearest",
                        Intersect = true
                    },
                    Scales = new Scales
                    {
                        
                        XAxes = new List<Scale>
                        {
                            new TimeScale
                            {
                                ScaleLabel = new ScaleLabel
                                {
                                    LabelString = "Date"
                                },
                            }
                        }
                    }
                }
            };

            var moistureData = new LineDataset()
            {
                Label = "Moisture Readings",
                BackgroundColor = ChartColor.FromRgb(0,0,0),
                BorderColor = ChartColor.FromRgb(0,0,0),
                Fill = "false",
                Data = new List<double?>()
            };
            
            foreach (var reading in Plant.MoistureReadings) {
                moistureData.Data.Add(reading.Value);
                MoistureChart.Data.Labels.Add(reading.ReadDate.ToString());
            }
            
            MoistureChart.Data.Datasets.Add(moistureData);


            // _config = new LineConfig
            // {
            //     Options = new LineOptions
            //     {
            //         Responsive = true,
            //         Title = new OptionsTitle
            //         {
            //             Display = true,
            //             Text = "Plant Moisture Data"
            //         },
            //         Tooltips = new Tooltips
            //         {
            //             Mode = InteractionMode.Nearest,
            //             Intersect = true
            //         },
            //         Hover = new Hover
            //         {
            //             Mode = InteractionMode.Nearest,
            //             Intersect = true
            //         },
            //         Scales = new Scales
            //         {
            //             XAxes = new List<CartesianAxis>
            //             {
            //                 new TimeAxis
            //                 {
            //                     ScaleLabel = new ScaleLabel
            //                     {
            //                         LabelString = "Date"
            //                     },
            //                     Time = new TimeOptions
            //                     {
            //                         TooltipFormat = "ll HH:mm"
            //                     },
            //                 }
            //             },
            //             YAxes = new List<CartesianAxis>
            //             {
            //                 new LinearCartesianAxis
            //                 {
            //                     ScaleLabel = new ScaleLabel
            //                     {
            //                         LabelString = "Value"
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // };

            // //_config.Data.Labels.AddRange(GetNextDays(InitalCount).Select(d => d.ToString("o")));
            // //_config.Data.Labels.AddRange()

            // IDataset<TimePoint> moistureData = new LineDataset<TimePoint>()
            // {
            //     Label = "Moisture Readings",
            //     BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Black),
            //     BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Black),
            //     Fill = FillingMode.Disabled
            // };
            // foreach (var reading in Model.MoistureReadings) {
            //     moistureData.Add(new TimePoint(reading.ReadDate, reading.Value));
            // }


            // IDataset<TimePoint> wateringData = new LineDataset<TimePoint>()
            // {
            //     Label = "Waterings",
            //     BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Blue),
            //     BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Blue),
            //     Fill = FillingMode.Disabled,
            //     ShowLine = false
            // };
            // foreach (var watering in Model.Waterings) {
            //     wateringData.Add(new TimePoint(watering.WateringDate, 0));
            // }

            // _config.Data.Datasets.Add(moistureData);
            // _config.Data.Datasets.Add(wateringData);
        }
    
    }
}
