using BlazorApp.UI.Application.Interfaces;
using BlazorApp.UI.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorApp.UI.Presentation.Pages
{
    public partial class Operations
    {
        private List<OperationModel> operations = new();
        private bool isLoading = true;
        private bool loadError = false;
        private bool showModal = false;
        private bool showDeleteModal = false;
        private bool isEditing = false;
        private bool isSubmitting = false;
        private bool isDeleting = false;
        private bool hasRendered = false;
        private OperationModel currentOperation = new();
        private OperationModel? operationToDelete;
        private List<OperationTypeModel> OperationTypes { get; set; } = new();


        protected override async Task OnInitializedAsync()
        {
            try
            {
                OperationTypes = await OperationTypeService.GetAllAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                OperationTypes = new();
            }
            await LoadOperations();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !hasRendered)
            {
                hasRendered = true;
                await JSRuntime.InvokeVoidAsync("console.log", "Operations page initialized");
            }
        }

        private async Task LoadOperations()
        {
            try
            {
                isLoading = true;
                loadError = false;

                operations = await OperationService.GetAllAsync();

                if (operations.Any() && OperationTypes != null && OperationTypes.Any())
                {
                    MapOperationTypes();
                }
            }
            catch (Exception ex)
            {
                loadError = true;
                operations = new List<OperationModel>();
                Console.WriteLine($"Failed to load operations: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        public void ShowCreateModal()
        {
            isEditing = false;
            currentOperation = new OperationModel
            {
                Date = DateOnly.FromDateTime(DateTime.Today)
            };
            showModal = true;
        }

        public void ShowEditModal(OperationModel operation)
        {
            isEditing = true;
            currentOperation = new OperationModel
            {
                OperationId = operation.OperationId,
                Date = operation.Date,
                Amount = operation.Amount,
                Note = operation.Note,
                OperationTypeId = operation.OperationTypeId
            };
            showModal = true;
        }

        private void CloseModal()
        {
            showModal = false;
            currentOperation = new OperationModel();
        }

        private void ShowDeleteModal(OperationModel operation)
        {
            operationToDelete = operation;
            showDeleteModal = true;
        }

        private void CloseDeleteModal()
        {
            showDeleteModal = false;
            operationToDelete = null;
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
                    await OperationService.UpdateAsync(currentOperation.OperationId, currentOperation);
                }
                else
                {
                    await OperationService.CreateAsync(currentOperation);
                }

                CloseModal();
                await LoadOperations();
                foreach (var op in operations)
                {
                    op.OperationTypeModel = OperationTypes.FirstOrDefault(t => t.OperationTypeId == op.OperationTypeId);
                }
                await JSRuntime.InvokeVoidAsync("alert", $"Operation successfully {(isEditing ? "updated" : "created")}!");
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

        private void MapOperationTypes()
        {
            if (operations == null || OperationTypes == null) return;

            foreach (var op in operations)
            {
                op.OperationTypeModel = OperationTypes.FirstOrDefault(t => t.OperationTypeId == op.OperationTypeId);
            }
        }
        private async Task ConfirmDelete()
        {
            if (operationToDelete == null) return;

            try
            {
                isDeleting = true;
                await OperationService.DeleteAsync(operationToDelete.OperationId);

                CloseDeleteModal();
                await LoadOperations();
                foreach (var op in operations)
                {
                    op.OperationTypeModel = OperationTypes.FirstOrDefault(t => t.OperationTypeId == op.OperationTypeId);
                }
                await JSRuntime.InvokeVoidAsync("alert", "Operation successfully deleted!");
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
