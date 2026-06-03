namespace HttpServer;

public class BookService
{
    private int _maxId = 0;
    private readonly List<Book> _books;

    public BookService()
    {
        _books =
        [
            new()
            {
                Id = ++_maxId,
                Name = "Мастер и Маргарита",
                Author = "Михаил Булгаков",
                Description = "Роман о дьяволе, посетившем советскую Москву."
            },
            new()
            {
                Id = ++_maxId,
                Name = "Преступление и наказание",
                Author = "Фёдор Достоевский",
                Description = "История студента Раскольникова и его теории о праве на кровь."
            },
            new()
            {
                Id = ++_maxId,
                Name = "Война и мир",
                Author = "Лев Толстой",
                Description = "Эпопея о судьбах людей на фоне наполеоновских войн."
            },
        ];
    }

    public List<Book> GetAll()
    { 
        return _books;
    }

    public Book GetById(int id)
    {
        return _books.FirstOrDefault(x => x.Id == id);
    }

    public void Create(Book book)
    {
        book.Id = ++_maxId;

        _books.Add(book);
    }

    public void Update(int id, Book book)
    {
        var updatedBook = GetById(id);

        updatedBook.Name = book.Name;
        updatedBook.Author = book.Author;
        updatedBook.Description = book.Description;
    }

    public void Delete(int id)
    {
        var deletedBook = GetById(id);
        _books.Remove(deletedBook);
    }
}