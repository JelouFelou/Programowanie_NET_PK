using System;
using TextAnalytics.Services;
using Microsoft.Extensions.DependencyInjection;
using TextAnalytics.Core;

namespace TextAnalytics.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Konfiguracja kontenera Dependency Injection
                var services = new ServiceCollection()
                    // Rejestracja usług warstwy Services
                    .AddSingleton<ILoggerService, ConsoleLogger>()

                    // Rejestracja TextAnalyzer i TextAnalyticsApp
                    .AddSingleton<TextAnalyzer>()
                    .AddSingleton<TextAnalyticsApp>() // Główna klasa aplikacji, wstrzyga usługi

                    // UWAGA: IInputProvider nie jest rejestrowany tutaj,
                    // ponieważ wybór dostawcy (Console/File) zależy od argumentów CLI.
                    // Dostawca jest tworzony dynamicznie w TextAnalyticsApp.

                    .BuildServiceProvider();

                // Uruchomienie aplikacji z wstrzykniętymi zależnościami
                var app = services.GetRequiredService<TextAnalyticsApp>();
                app.Run(args);
            }
            catch (Exception ex)
            {
                // Logowanie błędów na najwyższym poziomie
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Krytyczny błąd podczas inicjalizacji DI: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}