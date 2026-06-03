using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1;

await RunBookClient();

async Task RunBookClient()
{
    using var client = new BookHttpClient();

    while (true)
    {
        Console.WriteLine("\n=== Управление книгами ===");
        Console.WriteLine("1. Показать все книги");
        Console.WriteLine("2. Показать книгу по ID");
        Console.WriteLine("3. Добавить новую книгу");
        Console.WriteLine("4. Обновить книгу");
        Console.WriteLine("5. Удалить книгу");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                await DisplayAllBooks(client);
                break;
            case "2":
                await DisplayBookById(client);
                break;
            case "3":
                await AddNewBook(client);
                break;
            case "4":
                await UpdateBook(client);
                break;
            case "5":
                await DeleteBook(client);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Неверный выбор. Попробуйте снова.");
                break;
        }
    }
}

async Task DisplayAllBooks(BookHttpClient client)
{
    var books = await client.GetAllBooksAsync();
    if (books != null && books.Count > 0)
    {
        Console.WriteLine("\nСписок всех книг:");
        foreach (var book in books)
        {
            DisplayBook(book);
        }
    }
    else
    {
        Console.WriteLine("Книги не найдены.");
    }
}

async Task DisplayBookById(BookHttpClient client)
{
    Console.Write("Введите ID книги: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var book = await client.GetBookByIdAsync(id);
        if (book != null)
        {
            DisplayBook(book);
        }
    }
    else
    {
        Console.WriteLine("Неверный формат ID.");
    }
}

async Task AddNewBook(BookHttpClient client)
{
    Console.Write("Название: ");
    var name = Console.ReadLine() ?? string.Empty;

    Console.Write("Автор: ");
    var author = Console.ReadLine() ?? string.Empty;

    Console.Write("Описание: ");
    var description = Console.ReadLine() ?? string.Empty;

    var newBook = new Book
    {
        Name = name,
        Author = author,
        Description = description
    };

    if (await client.CreateBookAsync(newBook))
    {
        Console.WriteLine("Книга успешно добавлена!");
    }
}

async Task UpdateBook(BookHttpClient client)
{
    Console.Write("Введите ID книги для обновления: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        Console.Write("Новое название: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Новый автор: ");
        var author = Console.ReadLine() ?? string.Empty;

        Console.Write("Новое описание: ");
        var description = Console.ReadLine() ?? string.Empty;

        var updatedBook = new Book
        {
            Name = name,
            Author = author,
            Description = description
        };

        if (await client.UpdateBookAsync(id, updatedBook))
        {
            Console.WriteLine("Книга успешно обновлена!");
        }
    }
    else
    {
        Console.WriteLine("Неверный формат ID.");
    }
}

async Task DeleteBook(BookHttpClient client)
{
    Console.Write("Введите ID книги для удаления: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        if (await client.DeleteBookAsync(id))
        {
            Console.WriteLine("Книга успешно удалена!");
        }
    }
    else
    {
        Console.WriteLine("Неверный формат ID.");
    }
}

void DisplayBook(Book? book)
{
    if (book == null)
    {
        Console.WriteLine("Книга не найдена.");
        return;
    }

    Console.WriteLine($"ID: {book.Id}");
    Console.WriteLine($"Название: {book.Name}");
    Console.WriteLine($"Автор: {book.Author}");
    Console.WriteLine($"Описание: {book.Description}");
    Console.WriteLine("---");
}