using BlazorApp.UI.Application.Interfaces;
using BlazorApp.UI.Domain.Models;

namespace BlazorApp.UI.Application.Services
{
    public class OperationTypeService : IOperationTypeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OperationTypeService> _logger;

        public OperationTypeService(IHttpClientFactory httpClientFactory, ILogger<OperationTypeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<OperationTypeModel>> GetAllAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                return await client.GetFromJsonAsync<List<OperationTypeModel>>("api/operationtypes") ?? new List<OperationTypeModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operation types");
                throw;
            }
        }

        public async Task<OperationTypeModel?> GetByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                return await client.GetFromJsonAsync<OperationTypeModel>($"api/operationtypes/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operation type {OperationTypeId}", id);
                throw;
            }
        }

        public async Task<OperationTypeModel> CreateAsync(OperationTypeModel operationType)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PostAsJsonAsync("api/operationtypes", operationType);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create operation type: {response.StatusCode}");
                }

                return await response.Content.ReadFromJsonAsync<OperationTypeModel>() 
                    ?? throw new Exception("Failed to deserialize created operation type");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create operation type");
                throw;
            }
        }

        public async Task UpdateAsync(int id, OperationTypeModel operationType)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.PutAsJsonAsync($"api/operationtypes/{id}", operationType);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to update operation type: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update operation type {OperationTypeId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.DeleteAsync($"api/operationtypes/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to delete operation type: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete operation type {OperationTypeId}", id);
                throw;
            }
        }
    }
}
