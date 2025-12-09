using Microsoft.AspNetCore.Components;
using X00194620_Ca3.Services;

namespace X00194620_Ca3.Pages;

public class LaunchPhoto{
    public string ImageUrl{ get; set; } = string.Empty;
    public string LaunchId{ get; set; } = string.Empty;
    public string LaunchName{ get; set; } = string.Empty;
    public DateTime? LaunchDate{ get; set; }
}

public partial class LaunchPhotos : ComponentBase{
    [Inject] private SpaceXService SpaceX{ get; set; } = default!;

    private List<LaunchPhoto> photos = new();
    private bool loading = true;
    private string searchText = string.Empty;

    private int currentPage = 1;
    private int pageSize = 20;

    private bool imageDialogOpen = false;
    private LaunchPhoto? selectedPhoto = null;
    private int TotalItems => FilteredPhotos.Count;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)pageSize));
    private List<LaunchPhoto> PagedPhotos => FilteredPhotos.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

    private string SearchText{
        get => searchText;
        set {
            if (searchText == value) return;
            searchText = value;
            currentPage = 1;
        }
    }

    protected override async Task OnInitializedAsync(){
        loading = true;

        var launches = await SpaceX.GetLaunchesAsync();

        foreach (var launch in launches){
            if (launch.Links?.Flickr?.Original != null && launch.Links.Flickr.Original.Any()){
                foreach (var imageUrl in launch.Links.Flickr.Original){
                    photos.Add(new LaunchPhoto{
                        ImageUrl = imageUrl,
                        LaunchId = launch.Id,
                        LaunchName = launch.Name ?? "Unknown Mission",
                        LaunchDate = launch.DateUtc
                    });
                }
            }
        }

        loading = false;
    }

    private List<LaunchPhoto> FilteredPhotos{
        get {
            var query = photos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText)){
                var lower = SearchText.Trim().ToLowerInvariant();
                query = query.Where(p => p.LaunchName.ToLowerInvariant().Contains(lower));
            }

            return query.OrderByDescending(p => p.LaunchDate ?? DateTime.MinValue).ToList();
        }
    }
    
    private void ClearSearch(){
        SearchText = string.Empty;
        currentPage = 1;
    }

    private void OpenImageDialog(LaunchPhoto photo){
        selectedPhoto = photo;
        imageDialogOpen = true;
    }

    private void CloseImageDialog(){
        imageDialogOpen = false;
        selectedPhoto = null;
        StateHasChanged();
    }
}