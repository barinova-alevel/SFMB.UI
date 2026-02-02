using BlazorApp.UI.Components.Components;
using BlazorApp.UI.Models;
using Bunit;
using FluentAssertions;

namespace Tests.Components
{
    public class DeleteConfirmationModalTests : BunitContext
    {
        [Fact]
        public void Modal_WhenVisibleIsFalse_ShouldNotRenderModal()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, false));

            // Assert
            cut.Markup.Should().BeEmpty();
        }

        [Fact]
        public void Modal_WhenVisibleIsTrue_ShouldRenderModalAndBackdrop()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Find(".modal").Should().NotBeNull();
            cut.Find(".modal-backdrop").Should().NotBeNull();
        }

        [Fact]
        public void Modal_WhenVisibleIsTrue_ShouldRenderConfirmDeleteTitle()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Find(".modal-title").TextContent.Should().Be("Confirm Delete");
        }

        [Fact]
        public void Modal_WhenModelIsOperationType_ShouldDisplayOperationTypeInMessage()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            cut.Markup.Should().Contain("Operation Type");
            cut.Markup.Should().Contain("Are you sure you want to delete");
        }

        [Fact]
        public void Modal_WhenModelIsOperation_ShouldDisplayOperationInMessage()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel()));

            // Assert
            cut.Markup.Should().Contain("Operation");
            cut.Markup.Should().NotContain("Operation Type");
        }

        [Fact]
        public void Modal_WhenVisibleIsTrue_ShouldRenderIrreversibleWarning()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationModel()));

            // Assert
            var warning = cut.Find(".text-danger");
            warning.TextContent.Should().Contain("cannot be undone");
        }

        [Fact]
        public void CloseButton_WhenClicked_ShouldInvokeCloseDeleteModal()
        {
            // Arrange
            var closeCalled = false;

            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel())
                .Add(p => p.CloseDeleteModal, () => closeCalled = true));

            // Act
            EventHandlerDispatchExtensions.Click(cut.Find(".btn-close"));

            // Assert
            closeCalled.Should().BeTrue();
        }

        [Fact]
        public void CancelButton_WhenClicked_ShouldInvokeCloseDeleteModal()
        {
            // Arrange
            var closeCalled = false;

            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.Model, new OperationTypeModel())
                .Add(p => p.CloseDeleteModal, () => closeCalled = true));

            // Act
            EventHandlerDispatchExtensions.Click(cut.Find(".btn.btn-secondary"));

            // Assert
            closeCalled.Should().BeTrue();
        }

        [Fact]
        public void DeleteButton_WhenClicked_ShouldInvokeConfirmDelete()
        {
            // Arrange
            var confirmCalled = false;

            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsDeleting, false)
                .Add(p => p.Model, new OperationTypeModel())
                .Add(p => p.ConfirmDelete, () => confirmCalled = true));

            // Act
            EventHandlerDispatchExtensions.Click(cut.Find(".btn.btn-danger"));

            // Assert
            confirmCalled.Should().BeTrue();
        }

        [Fact]
        public void DeleteButton_WhenIsDeletingIsTrue_ShouldBeDisabledAndShowSpinner()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsDeleting, true)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var deleteButton = cut.Find(".btn.btn-danger");
            deleteButton.HasAttribute("disabled").Should().BeTrue();
            deleteButton.TextContent.Should().Contain("Deleting...");
            cut.Find(".spinner-border").Should().NotBeNull();
        }

        [Fact]
        public void DeleteButton_WhenIsDeletingIsFalse_ShouldShowDeleteTextAndNotBeDisabled()
        {
            // Arrange & Act
            var cut = Render<DeleteConfirmationModal>(parameters => parameters
                .Add(p => p.Visible, true)
                .Add(p => p.IsDeleting, false)
                .Add(p => p.Model, new OperationTypeModel()));

            // Assert
            var deleteButton = cut.Find(".btn.btn-danger");
            deleteButton.HasAttribute("disabled").Should().BeFalse();
            deleteButton.TextContent.Should().Contain("Delete");
        }
    }
}
