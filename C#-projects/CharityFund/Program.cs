using CharityFund.Forms;
using CharityFund.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CharityFund;

internal static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();
        var dbService = host.Services.GetRequiredService<DatabaseService>();

        bool isConnected = false;
        string errorMessage = "";

        try
        {
            await dbService.InitializeAsync();
            isConnected = true;
        }
        catch (Exception ex)
        {
            isConnected = false;
            errorMessage = ex.Message;
        }

        var mainForm = host.Services.GetRequiredService<MainForm>();

        mainForm.SetDatabaseConnectionStatus(isConnected, errorMessage);

        Application.Run(mainForm);

        if (isConnected)
        {
            await dbService.DisposeAsync();
        }
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var connectionString = "Host=localhost;" +
                    "Port=****;" +
                    "Database=fund;" +
                    "Username=postgres;" +
                    "Password=*****";
                services.AddSingleton(new DatabaseService(connectionString));
                services.AddSingleton<MainForm>();
                services.AddTransient<ChildrenForm>();
                services.AddTransient<ActiveFundraisingsForm>();
                services.AddTransient<DiseaseStatisticForm>();
                services.AddTransient<ForeignDonationsForm>();
            });
    }
}