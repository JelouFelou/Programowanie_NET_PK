using TextAnalytics.Core;
using Microsoft.Extensions.DependencyInjection;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Klasa statyczna zawierająca metody rozszerzające (Extension Methods)
    /// dla uproszczenia rejestracji usług do kontenera DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Rejestruje wszystkie kluczowe usługi i logikę analityczną do kolekcji DI.
        /// </summary>
        public static IServiceCollection AddTextAnalyticsServices(this IServiceCollection services)
        {
            // Rejestracja usług warstwy Services
            services.AddSingleton<ILoggerService, ConsoleLogger>();

            // Rejestracja domyślnego dostawcy wejścia (Konsola)
            services.AddSingleton<IInputProvider, ConsoleInputProvider>();

            // Rejestracja logiki warstwy Core
            // Rejestracja TextAnalyzer pod interfejsem ITextAnalyzer (zgodnie z zasadami DI)
            services.AddSingleton<ITextAnalyzer, TextAnalyzer>();

            return services;
        }
    }
}