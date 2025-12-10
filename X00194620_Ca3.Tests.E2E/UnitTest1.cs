using Microsoft.Playwright;
using NUnit.Framework;

namespace X00194620_Ca3.Tests.E2E;

[Parallelizable(ParallelScope.Self)]
public class E2E_LaunchNavigationTests
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private const string BaseUrl = "https://adriandonnelly.github.io/CA3_SpaceXLaunches/";

    [SetUp]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Test]
    public async Task HomePage_Should_Load_And_Display_Title()
    {
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForSelectorAsync("text=SpaceX Launch Timeline", new() { Timeout = 60000 });

        Assert.IsTrue(await page.IsVisibleAsync("text=SpaceX Launch Timeline"));
    }

    [Test]
    public async Task LaunchCards_Should_Render()
    {
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        var card = await page.WaitForSelectorAsync(".mud-paper", new() { Timeout = 60000 });

        Assert.IsNotNull(card);
    }

    [Test]
    public async Task Clicking_A_Launch_Navigates_To_Details_Page()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync(BaseUrl);

        await page.WaitForSelectorAsync(".mud-paper", new() { Timeout = 60000 });
        await page.ClickAsync(".mud-paper >> nth=0");
        await page.WaitForURLAsync(url => url.ToString().Contains("/launch/"), new() { Timeout = 60000 });

        StringAssert.Contains("/launch/", page.Url);
    }
}
