using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Запускаем TCP‑сервер в отдельном потоке
var tcpServer = new TcpServer();
Task.Run(() => tcpServer.Start());

// HTTP‑эндпоинты
app.MapGet("/books", () => tcpServer._bookService.GetAll());
app.MapGet("/books/{id}", (int id) => tcpServer._bookService.GetById(id));
app.MapPost("/books", (Book book) => tcpServer._bookService.Create(book));
app.MapPut("/books/{id}", (int id, Book book) => tcpServer._bookService.Update(id, book));
app.MapDelete("/books/{id}", (int id) => tcpServer._bookService.Delete(id));

app.Run();

// Перечисление статусов
enum DictionaryResponse { OK, BadRequest, NotFoundResource }

// Унифицированный ответ
record ApiResponse<T>(DictionaryResponse Status, string? Message = null, T? Data = default);

// Модель книги
record Book(int Id, string Name, string Author, string Description);

// Модель TCP‑запроса
record TcpRequest(string Action, int Id = 0, Book? Book = null);

class BookService
{
    private int _nextId = 1;
    private readonly List<Book> _books = new()
    {
        new(1, "Мастер и Маргарита", "Михаил Булгаков", "Роман о дьяволе в Москве")
    };

    public ApiResponse<List<Book>> GetAll() => new(DictionaryResponse.OK, Data: _books);

    public ApiResponse<Book> GetById(int id)
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        return book != null
            ? new(DictionaryResponse.OK, Data: book)
            : new(DictionaryResponse.NotFoundResource, $"Книга с ID {id} не найдена");
    }

    public ApiResponse<Book> Create(Book book)
    {
        // Простая валидация
        if (book.Name.Length < 2)
            return new(DictionaryResponse.BadRequest, "Название должно быть ≥ 2 символов");
        if (book.Author.Length < 2)
            return new(DictionaryResponse.BadRequest, "Автор должен быть ≥ 2 символов");
        if (book.Description.Length < 10)
            return new(DictionaryResponse.BadRequest, "Описание должно быть ≥ 10 символов");

        var newBook = book with { Id = _nextId++ };
        _books.Add(newBook);
        return new(DictionaryResponse.OK, Data: newBook);
    }

    public ApiResponse<Book> Update(int id, Book book)
    {
        var existing = _books.FindIndex(b => b.Id == id);
        if (existing == -1)
            return new(DictionaryResponse.NotFoundResource, $"Книга с ID {id} не найдена");

        // Валидация перед обновлением
        if (book.Name.Length < 2)
            return new(DictionaryResponse.BadRequest, "Название должно быть ≥ 2 символов");
        if (book.Author.Length < 2)
            return new(DictionaryResponse.BadRequest, "Автор должен быть ≥ 2 символов");
        if (book.Description.Length < 10)
            return new(DictionaryResponse.BadRequest, "Описание должно быть ≥ 10 символов");

        _books[existing] = book with { Id = id };
        return new(DictionaryResponse.OK, Data: _books[existing]);
    }

    public ApiResponse<string> Delete(int id)
    {
        var book = _books.FindIndex(b => b.Id == id);
        if (book == -1)
            return new(DictionaryResponse.NotFoundResource, $"Книга с ID {id} не найдена");

        _books.RemoveAt(book);
        return new(DictionaryResponse.OK, Data: "Удалено");
    }
}

class TcpServer
{
    public readonly BookService _bookService = new();

    public void Start()
    {
        var listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("TCP‑сервер запущен на порту 8080");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Парсим запрос
            var request = JsonSerializer.Deserialize<TcpRequest>(requestJson);
            ApiResponse<object>? response = null;

            if (request?.Action == "GET_ALL")
            {
                var result = _bookService.GetAll();
                response = new ApiResponse<object>(result.Status, result.Message, result.Data);
            }
            else if (request?.Action == "GET_BY_ID")
            {
                var result = _bookService.GetById(request.Id);
                response = new ApiResponse<object>(result.Status, result.Message, result.Data);
            }
            else if (request?.Action == "CREATE" && request.Book != null)
            {
                var result = _bookService.Create(request.Book);
                response = new ApiResponse<object>(result.Status, result.Message, result.Data);
            }
            else if (request?.Action == "UPDATE" && request.Book != null)
            {
                var result = _bookService.Update(request.Id, request.Book);
                response = new ApiResponse<object>(result.Status, result.Message, result.Data);
            }
            else if (request?.Action == "DELETE")
            {
                var result = _bookService.Delete(request.Id);
                response = new ApiResponse<object>(result.Status, result.Message, result.Data);
            }
            else
            {
                response = new ApiResponse<object>(DictionaryResponse.BadRequest, "Неизвестное действие");
            }

            // Отправляем ответ
            var responseJson = JsonSerializer.Serialize(response);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await stream.WriteAsync(responseBytes);
        }
        catch (Exception ex)
        {
            var error = new ApiResponse<object>(DictionaryResponse.BadRequest, ex.Message);
            var errorJson = JsonSerializer.Serialize(error);
            var errorBytes = Encoding.UTF8.GetBytes(errorJson);
            await client.GetStream().WriteAsync(errorBytes);
        }
        finally
        {
            client.Close();
        }
    }
}
