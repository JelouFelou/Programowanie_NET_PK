using System.Collections.Generic;
using System.Linq;
using LibraryApp.Domain;

namespace LibraryApp.Extensions
{
    // Klasa statyczna zawierająca metody rozszerzające
    public static class LibraryExtensions
    {
        // Metoda rozszerzająca filtrująca dostępne pozycje
        public static IEnumerable<T> Available<T>(this IEnumerable<T> items) where T : LibraryItem
            => items.Where(i => i.IsAvailable); // Wykorzystanie LINQ (Where)

        // Metoda rozszerzająca zwracająca 'take' najnowszych pozycji (wg ID)
        public static IEnumerable<LibraryItem> Newest(this IEnumerable<LibraryItem> items, int take)
            => items.OrderByDescending(i => i.Id).Take(take); // Wykorzystanie LINQ (OrderByDescending, Take)
    }
}