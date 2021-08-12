using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Forms;

using gardenit_razor.Api;
using gardenit_api_classes.Plant;
using gardenit_api_classes.Water;
using gardenit_api_classes.Moisture;

namespace gardenit_razor.Data
{
    // TODO: Lots of conversions here... Can we simplify...
    public class PlantService
    {
        private readonly IApi _api;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly static string DEFAULT_IMAGE_NAME = "default.png"; 

        public PlantService(IApi api, IWebHostEnvironment hostEnv) {
            _api = api;
            _hostEnv = hostEnv;
        }

        public async Task<PlantSummary> GetPlant(Guid id) {
            var result = await _api.GetAsync<PlantResponse>($"plant/{id}");
            return Convert(result);
        }

        public async Task<List<PlantSummary>> GetAll() {
            var results = await _api.GetAsync<List<PlantResponse>>("plant");
            return results.Select(Convert).ToList();
        }

        public async Task CreatePlant(PlantFormModel plant) {
            var imageName = await SaveAndGenerateImageName(plant.ImageFile);
            var req = new NewPlantRequest() {
                Name = plant.Name,
                Notes = plant.Notes,
                Type = plant.Type,
                DaysBetweenWatering = plant.DaysBetweenWatering,
                ImageName = imageName,
                PollPeriodMinutes = plant.PollPeriodMinutes,
                HasDevice = plant.HasDevice,
            };

            _api.Post("plant", req);
        }

        public async Task UpdatePlant(Guid plantId, PlantFormModel plant) {
            var imageName = await SaveAndGenerateImageName(plant.ImageFile);
            var req = new UpdatePlantRequest() {
                Name = plant.Name,
                Notes = plant.Notes,
                Type = plant.Type,
                DaysBetweenWatering = plant.DaysBetweenWatering,
                PollPeriodMinutes = plant.PollPeriodMinutes,
                HasDevice = plant.HasDevice,
                ImageName = (imageName == DEFAULT_IMAGE_NAME) ? plant.ImageName : imageName
            };

            _api.Put($"plant/{plantId}", req);
        }

        public async Task PlantActivation(Guid id, bool activate) {
            var plant = await _api.GetAsync<PlantResponse>($"plant/{id}");
            var req = new UpdatePlantRequest() {
                Name = plant.Name,
                Notes = plant.Notes,
                Type = plant.Type,
                DaysBetweenWatering = plant.DaysBetweenWatering,
                PollPeriodMinutes = plant.PollPeriodMinutes,
                HasDevice = activate,
                ImageName = plant.ImageName
            };
            _api.Put($"plant/{id}", req);
        }


        public async Task WaterPlant(Guid plantId, int seconds) {
            var req = new WateringRequest() {
                PlantId = plantId,
                Seconds = seconds
            };
            await _api.PostAsync("watering", req);
        }

        public async Task RequestMoistureReading(Guid plantId) {
            var req = new MoistureReadingRequest() {
                PlantId = plantId
            };
            await _api.PostAsync("moisture/request", req);
        }

        public async Task DeletePlant(Guid plantId) {
            await _api.Delete($"plant/{plantId}");
        }

        private PlantSummary Convert(PlantResponse plant) {
            return new PlantSummary() {
                Id = plant.Id,
                Name = plant.Name,
                Notes = plant.Notes,
                Type = plant.Type,
                DaysBetweenWatering = plant.DaysBetweenWatering,
                ImageName = plant.ImageName,
                CreateDate = plant.CreateDate,
                HasDevice = plant.HasDevice,
                PollPeriodMinutes = plant.PollPeriodMinutes,
                Waterings = plant.Waterings.Select(x => new Watering() {
                    WateringDate = x.WateringDate,
                    Seconds = x.Seconds
                }).ToList(),
                MoistureReadings = plant.MoistureReadings.Select(x => new MoistureReading() {
                    ReadDate = x.ReadDate,
                    Value = x.Value
                }).ToList()
            };
        }

        private async Task<string> SaveAndGenerateImageName(IFormFile image) {
            string wwwRootPath = _hostEnv.WebRootPath;

            if (image != null) {
                string filename = Path.GetFileNameWithoutExtension(image.FileName);
                string dateString = DateTime.Now.ToString("yymmssfff");
                string ext = Path.GetExtension(image.FileName);
                string imageName = filename + dateString + ext;
                string path = Path.Combine(wwwRootPath + "/image/", imageName);
                var fileStream = new FileStream(path, FileMode.Create);
                await image.CopyToAsync(fileStream);
                return imageName;
            } else {
                return DEFAULT_IMAGE_NAME;
            }
        }

    }
}