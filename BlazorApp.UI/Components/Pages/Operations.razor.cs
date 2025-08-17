using System.Net;
using BlazorApp.UI.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;

namespace BlazorApp.UI.Components.Pages
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
            await LoadOperations();
            var client = HttpClientFactory.CreateClient("Api");
            var operationTypes = await client.GetFromJsonAsync<List<OperationTypeModel>>("api/operationtypes");
            OperationTypes = operationTypes ?? new List<OperationTypeModel>();

            foreach (var op in operations)
            {
                op.OperationTypeModel = OperationTypes.FirstOrDefault(t => t.OperationTypeId == op.OperationTypeId);
            }
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

                var client = HttpClientFactory.CreateClient("Api");
                operations = await client.GetFromJsonAsync<List<OperationModel>>("api/operations") ?? new();
            }
            catch (Exception ex)
            {
                loadError = true;
                Console.WriteLine($"Failed to load operations: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
            foreach (var op in operations)
            {
                op.OperationTypeModel = OperationTypes.FirstOrDefault(t => t.OperationTypeId == op.OperationTypeId);
            }
        }

        public void ShowCreateModal()
        {
            isEditing = false;
            currentOperation = new OperationModel
            {
                Date = DateTime.Today
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
                var client = HttpClientFactory.CreateClient("Api");

                if (isEditing)
                {
                    var response = await client.PutAsJsonAsync($"api/operations/" +
                        $"{currentOperation.OperationId}", currentOperation);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to update operation: {response.StatusCode}");
                    }
                }
                else
                {
                    var response = await client.PostAsJsonAsync("api/operations", currentOperation);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to create operation: {response.StatusCode}");
                    }
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

        private async Task ConfirmDelete()
        {
            if (operationToDelete == null) return;

            try
            {
                isDeleting = true;
                var client = HttpClientFactory.CreateClient("Api");
                var response = await client.DeleteAsync($"api/operations/{operationToDelete.OperationId}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to delete operation: {response.StatusCode}");
                }

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
