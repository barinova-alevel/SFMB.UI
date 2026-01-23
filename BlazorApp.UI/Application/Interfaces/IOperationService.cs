using BlazorApp.UI.Domain.Models;

namespace BlazorApp.UI.Application.Interfaces
{
    public interface IOperationService
    {
        Task<List<OperationModel>> GetAllAsync();
        Task<OperationModel?> GetByIdAsync(int id);
        Task<OperationModel> CreateAsync(OperationModel operation);
        Task UpdateAsync(int id, OperationModel operation);
        Task DeleteAsync(int id);
    }
}
