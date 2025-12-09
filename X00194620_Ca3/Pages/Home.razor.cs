using Microsoft.AspNetCore.Components;
using MudBlazor;
using X00194620_Ca3.Services;

namespace X00194620_Ca3.Pages;

public partial class Home : ComponentBase
{
    [Inject] private SpaceXService SpaceX { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private List<Launch>? launches;
    private bool loading = true;

    // Filters (backing fields)
    private string searchText = string.Empty;
    private string selectedStatus = "All";

    // Pagination
    private int currentPage = 1;
    private int pageSize = 10;

    // Exposed bindable properties for fields so changing filters resets page
    private string SearchText
    {
        get => searchText;
        set
        {
            if (searchText == value) return;
            searchText = value;
            currentPage = 1; // reset page when searching
        }
    }

    private string SelectedStatus
    {
        get => selectedStatus;
        set
        {
            if (selectedStatus == value) return;
            selectedStatus = value;
            currentPage = 1; // reset page when changing status
        }
    }

    protected override async Task OnInitializedAsync()
    {
        launches = await SpaceX.GetLaunchesAsync();
        loading = false;
    }

    private List<Launch> FilteredLaunches
    {
        get
        {
            if (launches is null) return new List<Launch>();

            var query = launches.AsEnumerable();

            // text search (name)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.Trim().ToLowerInvariant();
                query = query.Where(l => (l.Name ?? string.Empty).ToLowerInvariant().Contains(lower));
            }

            // status filter
            query = SelectedStatus switch
            {
                "Success" => query.Where(l => l.Success == true),
                "Failed" => query.Where(l => l.Success == false),
                "Unknown" => query.Where(l => l.Success == null),
                _ => query
            };

            // final ordering - return as List to avoid multiple enumerations
            return query.OrderBy(x => x.DateUtc ?? DateTime.MinValue).ToList();
        }
    }

    private int TotalItems => FilteredLaunches.Count;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)pageSize));
    private List<Launch> PagedLaunches => FilteredLaunches.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

    private void GoToPage(int p)
    {
        Console.WriteLine($"GoToPage called with page={p}, currentPage={currentPage}, TotalPages={TotalPages}, TotalItems={TotalItems}");

        // Validate the page number
        if (p < 1 || p > TotalPages)
        {
            Console.WriteLine($"Page {p} is out of range (1-{TotalPages})");
            return;
        }

        // Update the current page
        currentPage = p;
        Console.WriteLine($"Updated currentPage to {currentPage}");

        // Force re-render
        StateHasChanged();
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatus = "All";
        currentPage = 1;
    }

    private void OpenLaunch(string id)
    {
        Nav.NavigateTo($"/launch/{id}");
    }

    private Color StatusColor(bool? success) =>
        success == true ? Color.Success :
        success == false ? Color.Error :
        Color.Warning;

    private string StatusText(bool? success) =>
        success == true ? "Success" :
        success == false ? "Failed" :
        "Unknown";
}

