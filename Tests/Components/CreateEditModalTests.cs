using BlazorApp.UI.Components.Components;
using BlazorApp.UI.Models;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;

namespace Tests.Components
{
    public class CreateEditModalTests : BunitContext
    {
        [Fact]
        public void Modal_WhenVisibleIsFalse_ShouldNotRenderModal()
        {
            // Arrange & Act
            var cut = Render<CreateEditModal>(parameters => parameters
        .Add(p => p.Visible, false));

            // Assert
            cut.Markup.Should().BeEmpty();
        }

        [Fact]
        public void Modal_WhenVisibleIsTrue_ShouldRenderModal()
        {
            // Arrange & Act
            var cut = Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Find(".modal").Should().NotBeNull();
            cut.Find(".modal-backdrop").Should().NotBeNull();
        }

        [Fact]
        public void Modal_WhenIsEditingIsTrue_ShouldDisplayEditTitle()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsEditing, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Find(".modal-title").TextContent.Should().Contain("Edit");
        }

        [Fact]
        public void Modal_WhenIsEditingIsFalse_ShouldDisplayCreateTitle()
        {
            // Arrange & Act
            var cut = Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsEditing, false)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Find(".modal-title").TextContent.Should().Contain("Create");
        }

        [Fact]
        public void Modal_WhenModelIsOperationType_ShouldDisplayOperationTypeTitle()
        {
            // Arrange & Act
            var cut = Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Find(".modal-title").TextContent.Should().Contain("Operation Type");
        }

        [Fact]
        public void Modal_WhenModelIsOperation_ShouldDisplayOperationTitle()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel()));

            // Assert
            cut.Find(".modal-title").TextContent.Should().Contain("Operation");
            cut.Find(".modal-title").TextContent.Should().NotContain("Operation Type");
        }

        [Fact]
        public void OperationTypeForm_ShouldRenderNameField()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var nameInput = cut.Find("#name");
            nameInput.Should().NotBeNull();
            cut.Find("label[for='name']").TextContent.Should().Contain("Name");
        }

        [Fact]
        public void OperationTypeForm_ShouldRenderDescriptionField()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var descriptionInput = cut.Find("#description");
            descriptionInput.Should().NotBeNull();
            cut.Find("label[for='description']").TextContent.Should().Contain("Description");
        }

        [Fact]
        public void OperationTypeForm_ShouldRenderIsIncomeCheckbox()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var isIncomeCheckbox = cut.Find("#isIncome");
            isIncomeCheckbox.Should().NotBeNull();
            cut.Find("label[for='isIncome']").TextContent.Should().Contain("This is an income type");
        }

        [Fact]
        public void OperationForm_ShouldRenderOperationTypeDropdown()
        {
            // Arrange
            var operationTypes = new List<OperationTypeModel>
            {
                new OperationTypeModel { OperationTypeId = 1, Name = "Salary" },
                new OperationTypeModel { OperationTypeId = 2, Name = "Groceries" }
            };

            // Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel())
                .Add(p => p.OperationTypes, operationTypes));

            // Assert
            var typeSelect = cut.Find("#type");
            typeSelect.Should().NotBeNull();
            cut.Find("label[for='type']").TextContent.Should().Contain("Operation Type");
            typeSelect.QuerySelectorAll("option").Length.Should().Be(3); // Including default option
        }

        [Fact]
        public void OperationForm_WhenNoOperationTypes_ShouldDisplayWarning()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel())
                .Add(p => p.OperationTypes, new List<OperationTypeModel>()));

            // Assert
            var alert = cut.Find(".alert-warning");
            alert.Should().NotBeNull();
            alert.TextContent.Should().Contain("No operation types found");
        }

        [Fact]
        public void OperationForm_ShouldRenderDateField()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel()));

            // Assert
            var dateInput = cut.Find("#date");
            dateInput.Should().NotBeNull();
            cut.Find("label[for='date']").TextContent.Should().Contain("Date");
        }

        [Fact]
        public void OperationForm_ShouldRenderAmountField()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel()));

            // Assert
            var amountInput = cut.Find("#amount");
            amountInput.Should().NotBeNull();
            cut.Find("label[for='amount']").TextContent.Should().Contain("Amount");
        }

        [Fact]
        public void OperationForm_ShouldRenderNoteField()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel()));

            // Assert
            var noteInput = cut.Find("#note");
            noteInput.Should().NotBeNull();
            cut.Find("label[for='note']").TextContent.Should().Contain("Note");
        }

        [Fact]
        public void CloseButton_WhenClicked_ShouldInvokeOnClose()
        {
            // Arrange
            var onCloseCalled = false;
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel())
                .Add(p => p.OnClose, () => onCloseCalled = true));

            // Act
            //cut.Find(".btn-close").Click();
            Bunit.EventHandlerDispatchExtensions.Click(cut.Find(".btn-close"));

            // Assert
            onCloseCalled.Should().BeTrue();
        }

        [Fact]
        public void CancelButton_WhenClicked_ShouldInvokeOnClose()
        {
            // Arrange
            var onCloseCalled = false;
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel())
                .Add(p => p.OnClose, () => onCloseCalled = true));

            // Act
            //cut.Find(".btn-secondary").Click();
            Bunit.EventHandlerDispatchExtensions.Click(cut.Find(".btn-secondary"));

            // Assert
            onCloseCalled.Should().BeTrue();
        }

        [Fact]
        public void SubmitButton_WhenNotSubmitting_ShouldDisplayCorrectText()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsEditing, false)
                .Add(p => p.IsSubmitting, false)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var submitButton = cut.Find(".btn-primary");
            submitButton.TextContent.Should().Contain("Create");
            submitButton.HasAttribute("disabled").Should().BeFalse();
        }

        [Fact]
        public void SubmitButton_WhenIsEditingTrue_ShouldDisplayUpdateText()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsEditing, true)
                .Add(p => p.IsSubmitting, false)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var submitButton = cut.Find(".btn-primary");
            submitButton.TextContent.Should().Contain("Update");
        }

        [Fact]
        public void SubmitButton_WhenSubmitting_ShouldBeDisabledAndShowSpinner()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsSubmitting, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var submitButton = cut.Find(".btn-primary");
            submitButton.HasAttribute("disabled").Should().BeTrue();
            submitButton.TextContent.Should().Contain("Saving...");
            cut.Find(".spinner-border").Should().NotBeNull();
        }

        [Fact]
        public void Modal_WhenUnsupportedModelType_ShouldDisplayWarning()
        {
            // Arrange & Act
            var cut = base.Render<CreateEditModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new object()));

            // Assert
            var alert = cut.Find(".alert-warning");
            alert.Should().NotBeNull();
            alert.TextContent.Should().Contain("Unsupported model type");
        }
    }
}
