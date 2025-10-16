using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAnalytics.Core
{
    /// <summary>
    /// Interfejs definiujący kontrakt dla usługi analizy tekstu.
    /// Umożliwia wstrzykiwanie i zastępowanie głównej logiki analitycznej.
    /// </summary>
    public interface ITextAnalyzer
    {
        /// <summary>
        /// Przeprowadza pełną analizę statystyczną podanego tekstu.
        /// </summary>
        /// <param name="text">Tekst do analizy.</param>
        /// <returns>Obiekt TextStatistics zawierający wszystkie metryki.</returns>
        TextStatistics Analyze(string text);
    }
}