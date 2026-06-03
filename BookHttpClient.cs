using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConsoleApp1;

public class BookHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5161";

    public BookHttpClient()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    // GET /book — получить все книги
    public async Task<List<Book>?> GetAllBooksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/book");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Book>>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при получении книг: {ex.Message}");
            return null;
        }
    }

    // GET /book/{id} — получить книгу по ID
    public async Task<Book?> GetBookByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/book/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Книга с ID {id} не найдена.");
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Book>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при получении книги: {ex.Message}");
            return null;
        }
    }

    // POST /book — создать новую книгу
    public async Task<bool> CreateBookAsync(Book book)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/book/", book);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при создании книги: {ex.Message}");
            return false;
        }
    }

    // PUT /book/{id} — обновить книгу
    public async Task<bool> UpdateBookAsync(int id, Book book)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/book/{id}", book);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Книга с ID {id} не найдена для обновления.");
                return false;
            }
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при обновлении книги: {ex.Message}");
            return false;
        }
    }

    // DELETE /book/{id} — удалить книгу
    public async Task<bool> DeleteBookAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/book/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Книга с ID {id} не найдена для удаления.");
                return false;
            }
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка при удалении книги: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

