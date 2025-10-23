using System;

namespace LibraryApp.Domain
{
    // Klasa dziedzicząca po Book, demonstrująca wielopoziomowe dziedziczenie.
    public class EBook : Book
    {
        public string FileFormat { get; } // np. PDF, EPUB

        public EBook(int id, string title, string author, string isbn, string fileFormat)
            : base(id, title, author, isbn)
        {
            FileFormat = fileFormat ?? throw new ArgumentNullException(nameof(fileFormat));
        }

        // Nadpisanie metody polimorficznej (dodanie formatu)
        public override void DisplayInfo()
        {
            Console.WriteLine($"[E-Book] ID: {Id}, Tytuł: {Title}, Autor: {Author}, ISBN: {Isbn}, Format: {FileFormat}, Dostępna: {IsAvailable}");
        }
    }
}