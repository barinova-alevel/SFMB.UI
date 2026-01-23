using BlazorApp.UI.Application.Interfaces;
using BlazorApp.UI.Domain.Models;

namespace BlazorApp.UI.Application.Services
{
    public class OperationService : IOperationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OperationService> _logger;

        public OperationService(IHttpClientFactory httpClientFactory, ILogger<OperationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<OperationModel>> GetAllAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                return await client.GetFromJsonAsync<List<OperationModel>>("api/operations") ?? new List<OperationModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operations");
                throw;
            }
        }

        public async Task<OperationModel?> GetByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                return await client.GetFromJsonAsync<OperationModel>($"api/operations/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operation {OperationId}", id);
                throw;
            }
        }

        public async Task<OperationModel> CreateAsync(OperationModel operation)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsJsonAsync("api/operations", operation);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create operation: {response.StatusCode}");
                }

                return await response.Content.ReadFromJsonAsync<OperationModel>() 
                    ?? throw new Exception("Failed to deserialize created operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create operation");
                throw;
            }
        }

        public async Task UpdateAsync(int id, OperationModel operation)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PutAsJsonAsync($"api/operations/{id}", operation);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to update operation: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update operation {OperationId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.DeleteAsync($"api/operations/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to delete operation: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete operation {OperationId}", id);
                throw;
            }
        }
    }
}
