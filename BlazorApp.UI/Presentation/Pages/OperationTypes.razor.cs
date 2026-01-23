using BlazorApp.UI.Application.Interfaces;
using BlazorApp.UI.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorApp.UI.Presentation.Pages
{
    public partial class OperationTypes
    {
        private List<OperationTypeModel> operationTypes = new();
        private List<OperationTypeModel> incomeTypes = new();
        private List<OperationTypeModel> expenseTypes = new();
        private bool isLoading = true;
        private bool loadError = false;
        private bool showModal = false;
        private bool showDeleteModal = false;
        private bool isEditing = false;
        private bool isSubmitting = false;
        private bool isDeleting = false;
        private bool hasRendered = false;
        private OperationTypeModel currentOperationType = new();
        private OperationTypeModel? operationTypeToDelete;

        protected override async Task OnInitializedAsync()
        {
            await LoadOperationTypes();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !hasRendered)
            {
                hasRendered = true;
                await JSRuntime.InvokeVoidAsync("console.log", "OperationTypes page initialized");
            }
        }

        private async Task LoadOperationTypes()
        {
            try
            {
                isLoading = true;
                loadError = false;

                operationTypes = await OperationTypeService.GetAllAsync();

                incomeTypes = operationTypes.Where(t => t.IsIncome).ToList();
                expenseTypes = operationTypes.Where(t => !t.IsIncome).ToList();
            }
            catch (Exception ex)
            {
                loadError = true;
                Console.WriteLine($"Failed to load operation types: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private void ShowCreateModal()
        {
            isEditing = false;
            currentOperationType = new OperationTypeModel();
            showModal = true;
        }

        private void ShowEditModal(OperationTypeModel operationType)
        {
            isEditing = true;
            currentOperationType = new OperationTypeModel
            {
                OperationTypeId = operationType.OperationTypeId,
                Name = operationType.Name,
                Description = operationType.Description,
                IsIncome = operationType.IsIncome
            };
            showModal = true;
        }

        private void CloseModal()
        {
            showModal = false;
            currentOperationType = new OperationTypeModel();
        }

        private void ShowDeleteModal(OperationTypeModel operationType)
        {
            operationTypeToDelete = operationType;
            showDeleteModal = true;
        }

        private void CloseDeleteModal()
        {
            showDeleteModal = false;
            operationTypeToDelete = null;
        }

        private async Task OnValidSubmit(EditContext context)
        {
            await HandleSubmit();
        }

        private async Task HandleSubmit()
        {
            try
            {
                isSubmitting = true;

                if (isEditing)
                {
                    await OperationTypeService.UpdateAsync(currentOperationType.OperationTypeId, currentOperationType);
                }
                else
                {
                    await OperationTypeService.CreateAsync(currentOperationType);
                }

                CloseModal();
                await LoadOperationTypes();
                await JSRuntime.InvokeVoidAsync("alert", $"Operation type successfully {(isEditing ? "updated" : "created")}!");
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Error: {ex.Message}");
            }
            finally
            {
                isSubmitting = false;
            }
        }

        private async Task ConfirmDelete()
        {
            if (operationTypeToDelete == null) return;

            try
            {
                isDeleting = true;
                await OperationTypeService.DeleteAsync(operationTypeToDelete.OperationTypeId);

                CloseDeleteModal();
                await LoadOperationTypes();
                await JSRuntime.InvokeVoidAsync("alert", "Operation type successfully deleted!");
            }
            catch (Exception ex)
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Error: {ex.Message}");
            }
            finally
            {
                isDeleting = false;
            }
        }
    }
}
