using Xunit;
using System.Linq;
using System.Collections.Generic;
using LibraryApp.Domain;
using LibraryApp.Extensions;

namespace LibraryApp.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void Available_FiltersCorrectly()
        {
            var item1 = new Book(1, "Available Book", "A", "1"); // Dostępna
            // Musimy zmienić stan IsAvailable. Użyjemy refleksji, jak w LibraryService.
            var item2 = new Book(2, "Unavailable Book", "B", "2");
            var propInfo = typeof(LibraryItem).GetProperty(nameof(LibraryItem.IsAvailable));
            propInfo.SetValue(item2, false);

            var item3 = new EBook(3, "Available EBook", "C", "3", "PDF"); // Dostępna

            var items = new List<LibraryItem> { item1, item2, item3 };

            // Użycie metody rozszerzającej
            var availableItems = items.Available().ToList();

            Assert.Equal(2, availableItems.Count);
            Assert.Contains(item1, availableItems);
            Assert.Contains(item3, availableItems);
            Assert.DoesNotContain(item2, availableItems);
        }

        [Fact]
        public void Newest_SortsAndTakesCorrectly()
        {
            var item1 = new Book(1, "Oldest", "A", "1");
            var item2 = new Book(3, "Newer", "B", "2");
            var item3 = new Book(2, "Middle", "C", "3");

            var items = new List<LibraryItem> { item1, item2, item3 };

            // Użycie metody rozszerzającej
            var newestItems = items.Newest(2).ToList();

            Assert.Equal(2, newestItems.Count);
            // Sprawdzenie kolejności (sortowanie po ID malejąco: 3, 2)
            Assert.Equal(item2, newestItems[0]); // ID 3
            Assert.Equal(item3, newestItems[1]); // ID 2
            Assert.DoesNotContain(item1, newestItems); // ID 1
        }
    }
}