using HttpServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<List<Book>>();
builder.Services.AddSingleton<BookService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/book", (BookService bookService) =>
{
    var book = bookService.GetAll();
    return Results.Ok(book);
});

app.MapGet("/book/{id}", (int id, BookService bookService) =>
{
    var book = bookService.GetById(id);
    return Results.Ok(book);
});

app.MapPost("/book/", (Book book, BookService bookService) =>
{
    bookService.Create(book);
    return Results.Created();
});

app.MapPut("/book/{id}", (int id, Book book, BookService bookService) =>
{
    bookService.Update(id, book);
    return Results.Ok();
});

app.MapDelete("/book/{id}", (int id, BookService bookService) =>
{
    bookService.Delete(id);
    return Results.Ok();
});

app.Run();