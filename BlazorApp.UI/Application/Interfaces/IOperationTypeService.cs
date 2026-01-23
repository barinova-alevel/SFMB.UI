using BlazorApp.UI.Domain.Models;

namespace BlazorApp.UI.Application.Interfaces
{
    public interface IOperationTypeService
    {
        Task<List<OperationTypeModel>> GetAllAsync();
        Task<OperationTypeModel?> GetByIdAsync(int id);
        Task<OperationTypeModel> CreateAsync(OperationTypeModel operationType);
        Task UpdateAsync(int id, OperationTypeModel operationType);
        Task DeleteAsync(int id);
    }
}
