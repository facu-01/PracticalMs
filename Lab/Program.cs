using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        const string endpoint = "http://localhost:5066/register";
        const int numberOfRequests = 60;

        using (var client = new HttpClient())
        {
            var tasks = new Task[numberOfRequests];

            Console.WriteLine($"Iniciando {numberOfRequests} peticiones paralelas...\n");
            var posix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            for (int i = 0; i < numberOfRequests; i++)
            {
                int requestNumber = i + 1;
                tasks[i] = MakeRequest(client, endpoint, requestNumber, posix);
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("\n✓ Todas las peticiones completadas");
        }
    }

    static async Task MakeRequest(HttpClient client, string endpoint, int requestNumber, long posix)
    {
        try
        {
            var payload = new
            {
                email = $"test_concurrencia_{posix}@test.com",
                password = $"12345678_r{requestNumber}"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Petición {requestNumber} enviada...");

            var response = await client.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Petición {requestNumber} completada ✓ Status: {(int)response.StatusCode}");

            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Petición {requestNumber} error ✗ Status: {(int)response.StatusCode}");
            }

            // var responseContent = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(responseContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Petición {requestNumber} excepción: {ex.Message}");
        }
    }
}
