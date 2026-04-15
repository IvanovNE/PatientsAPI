using SeedConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        var apiUrl = "http://localhost:5000";
        var count = 100;

        Console.WriteLine($"API URL: {apiUrl}");
        Console.WriteLine($"Generating {count} patients...");
        Console.WriteLine();

        using var httpClient = new HttpClient();
        var apiClient = new PatientsApiClient(apiUrl, httpClient);
        var generator = new PatientGenerator();

        var successCount = 0;
        var failCount = 0;

        for (int i = 1; i <= count; i++)
        {
            try
            {
                var patient = generator.Generate();
                await apiClient.PatientsPOSTAsync(patient);
                successCount++;
                Console.WriteLine($"[{i}/{count}] Created: {patient.Name.Family} {string.Join(" ", patient.Name.Given)}");
            }
            catch (Exception ex)
            {
                failCount++;
                Console.WriteLine($"[{i}/{count}] ERROR: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Done. Success: {successCount}, Failed: {failCount}");
    }
}