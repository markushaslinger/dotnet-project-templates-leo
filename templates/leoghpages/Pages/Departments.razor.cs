using System.Net.Http.Json;

namespace LeoPages.Pages;

public sealed partial class Departments
{
    private bool _initialized;
    private Department[]? _departments;
    
    protected override async Task OnInitializedAsync()
    {
        _initialized = false;
        
        await base.OnInitializedAsync();
        await LoadData();
        
        _initialized = true;
    }

    private async Task LoadData()
    {
        try
        {
            // fake some delay to simulate network latency
            await Task.Delay(1000);
            
            _departments = await Http.GetFromJsonAsync<Department[]>("data/departments.json");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Failed to load data via HTTP: {e.StatusCode}");
        }
    }

    private sealed record Department(int Id, string Name, string[] Programs);
}

