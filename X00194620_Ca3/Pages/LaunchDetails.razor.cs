using Microsoft.AspNetCore.Components;
using MudBlazor;
using X00194620_Ca3.Services;

namespace X00194620_Ca3.Pages;

public partial class LaunchDetails : ComponentBase
{
    [Parameter] public string Id { get; set; } = string.Empty;
    
    [Inject] private SpaceXService SpaceX { get; set; } = default!;

    private Launch? launch;
    private bool loading = true;
    
    private bool imageDialogOpen = false;
    private string selectedImageUrl = string.Empty;
    private DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Large, FullWidth = true };

    protected override async Task OnParametersSetAsync()
    {
        loading = true;
        launch = await SpaceX.GetLaunchAsync(Id);
        loading = false;
    }

    private Color StatusColor => launch?.Success switch
    {
        true => Color.Success,
        false => Color.Error,
        null => Color.Warning
    };

    private string StatusText => launch?.Success switch
    {
        true => "Success",
        false => "Failed",
        null => "Unknown"
    };

    private void OpenImageDialog(string imageUrl)
    {
        selectedImageUrl = imageUrl;
        imageDialogOpen = true;
    }

    private void CloseImageDialog()
    {
        imageDialogOpen = false;
        selectedImageUrl = string.Empty;
    }
}

