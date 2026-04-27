using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Application.DTOs;
using Application.Interfaces;
using Presentation.WPF.Models;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

/// <summary>
/// ViewModel for the main task management window.
/// Handles displaying, filtering, and managing 5W2H tasks.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFiveW2HTaskService _taskService;

    [ObservableProperty]
    private ObservableCollection<TaskModel> tasks = new();

    [ObservableProperty]
    private TaskModel? selectedTask;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private DateTime? filterStartDate;

    [ObservableProperty]
    private DateTime? filterEndDate;

    [ObservableProperty]
    private string? filterResponsible;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MainViewModel(IFiveW2HTaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    [RelayCommand]
    public async Task LoadTasks()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading tasks...";

            var taskDtos = await _taskService.GetAllTasksAsync();
            var models = taskDtos.Select(MapToModel).ToList();

            Tasks = new ObservableCollection<TaskModel>(models);
            StatusMessage = $"Loaded {models.Count} tasks";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchTasks()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Searching...";

            var taskDtos = await _taskService.SearchTasksAsync(
                SearchText,
                FilterStartDate,
                FilterEndDate,
                FilterResponsible
            );

            var models = taskDtos.Select(MapToModel).ToList();
            Tasks = new ObservableCollection<TaskModel>(models);
            StatusMessage = $"Found {models.Count} tasks";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ResetFilters()
    {
        SearchText = string.Empty;
        FilterStartDate = null;
        FilterEndDate = null;
        FilterResponsible = null;
        await LoadTasks();
    }

    [RelayCommand]
    public async Task DeleteSelectedTask()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Please select a task to delete";
            return;
        }

        if (MessageBox.Show(
            $"Are you sure you want to delete '{SelectedTask.What}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            IsLoading = true;
            var result = await _taskService.DeleteTaskAsync(SelectedTask.Id);

            if (result)
            {
                StatusMessage = "Task deleted successfully";
                await LoadTasks();
            }
            else
            {
                StatusMessage = "Failed to delete task";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static TaskModel MapToModel(FiveW2HTaskDto dto)
    {
        return new TaskModel
        {
            Id = dto.Id,
            What = dto.What,
            Why = dto.Why,
            Where = dto.Where,
            When = dto.When,
            Who = dto.Who,
            How = dto.How,
            HowMuch = dto.HowMuch,
            Status = dto.Status.ToString(),
            Priority = dto.Priority.ToString(),
            Notes = dto.Notes,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}
