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
                    // Rejestracja wszystkich kluczowych usług (Logger, InputProvider, TextAnalyzer)
                    .AddTextAnalyticsServices()

                    // Rejestracja głównej klasy aplikacji
                    .AddSingleton<TextAnalyticsApp>()

                    .BuildServiceProvider();

                // Uruchomienie aplikacji z wstrzykniętymi zależnościami
                var app = services.GetRequiredService<TextAnalyticsApp>();
                app.Run(args);
            }
            catch (Exception ex)
            {
                // Logowanie błędów na najwyższym poziomie
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Krytyczny blad podczas inicjalizacji aplikacji: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}