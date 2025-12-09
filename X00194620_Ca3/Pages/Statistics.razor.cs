using Microsoft.AspNetCore.Components;
using MudBlazor;
using X00194620_Ca3.Services;

namespace X00194620_Ca3.Pages;

public class LaunchSiteStats{
    public string SiteName{ get; set; } = string.Empty;
    public int LaunchCount{ get; set; }
    public int SuccessCount{ get; set; }
    public double SuccessRate{ get; set; }
}

public partial class Statistics : ComponentBase{
    [Inject] private SpaceXService SpaceX{ get; set; } = default!;

    private List<Launch>? launches;
    private bool loading = true;
    private int TotalLaunches => launches?.Count ?? 0;
    private int SuccessfulLaunches => launches?.Count(l => l.Success == true) ?? 0;
    private int FailedLaunches => launches?.Count(l => l.Success == false) ?? 0;
    private int UnknownLaunches => launches?.Count(l => l.Success == null) ?? 0;

    private double SuccessRate => TotalLaunches > 0
        ? Math.Round((double)SuccessfulLaunches / TotalLaunches * 100, 1)
        : 0;

    private double[] OutcomeData => new[]{
        (double)SuccessfulLaunches,
        (double)FailedLaunches,
        (double)UnknownLaunches
    };

    private string[] OutcomeLabels => new[]{ "Success", "Failed", "Unknown" };

    private ChartOptions outcomeChartOptions = new(){
        ChartPalette = new[]{ "#00C853", "#F44336", "#FFA726" }
    };

    private ChartOptions barChartOptions = new(){
        YAxisTicks = 10,
        YAxisLines = true,
        XAxisLines = false
    };

    private List<ChartSeries> LaunchesPerYearSeries{
        get {
            if (launches == null || !launches.Any())
                return new List<ChartSeries>();

            var launchesPerYear = launches
                .Where(l => l.DateUtc.HasValue)
                .GroupBy(l => l.DateUtc!.Value.Year)
                .OrderBy(g => g.Key)
                .Select(g => (double)g.Count())
                .ToArray();

            return new List<ChartSeries>{
                new ChartSeries{
                    Name = "Launches",
                    Data = launchesPerYear
                }
            };
        }
    }

    private string[] YearLabels{
        get {
            if (launches == null || !launches.Any())
                return Array.Empty<string>();

            return launches
                .Where(l => l.DateUtc.HasValue)
                .GroupBy(l => l.DateUtc!.Value.Year)
                .OrderBy(g => g.Key)
                .Select(g => g.Key.ToString())
                .ToArray();
        }
    }

    private List<LaunchSiteStats> TopLaunchSites{
        get {
            if (launches == null || !launches.Any())
                return new List<LaunchSiteStats>();
            var siteStats = launches
                .Where(l => !string.IsNullOrWhiteSpace(l.Id)) // Basic validation
                .GroupBy(l => GetLaunchSiteName(l))
                .Select(g => new LaunchSiteStats{
                    SiteName = g.Key,
                    LaunchCount = g.Count(),
                    SuccessCount = g.Count(l => l.Success == true),
                    SuccessRate = g.Count() > 0
                        ? Math.Round((double)g.Count(l => l.Success == true) / g.Count() * 100, 1)
                        : 0
                })
                .OrderByDescending(s => s.LaunchCount)
                .Take(5)
                .ToList();

            return siteStats;
        }
    }

    protected override async Task OnInitializedAsync(){
        loading = true;
        launches = await SpaceX.GetLaunchesAsync();
        loading = false;
    }

    private string GetLaunchSiteName(Launch launch){
        if (!string.IsNullOrWhiteSpace(launch.Name)){
            return launch.Id ?? "Unknown Site";
        }

        return "Unknown Site";
    }
}