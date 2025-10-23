using System;

namespace LibraryApp.Domain
{
    // Abstrakcyjna klasa bazowa dla wszystkich zasobów biblioteki.
    public abstract class LibraryItem
    {
        public int Id { get; }
        public string Title { get; protected set; }
        public bool IsAvailable { get; protected set; } = true;

        protected LibraryItem(int id, string title)
        {
            // Walidacja w konstruktorze
            if (id <= 0) throw new ArgumentException("ID musi być dodatnie.", nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Id = id;
        }

        public abstract void DisplayInfo(); // Metoda polimorficzna
    }
}