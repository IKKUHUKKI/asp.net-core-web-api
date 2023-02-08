using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using my_books.Data.Models;
using my_books.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace my_books.Data.Services
{
    public interface ICrud
    {
        string ConnectionString { get; set; }

        void AddBook(BookVM book);
        void DeleteBookById(int bookId);
        List<Book> GetAllBooks();
        Book GetBookById(int bookId);
        Book UpdateBookById(int bookId, BookVM book);
    }

    public class BooksService : ICrud
    {
        private readonly ILogger<ICrud> _logger;
        public string ConnectionString { get; set; }
        private IConfiguration Configuration { get; }
        public BooksService(IConfiguration configuration, ILogger<ICrud> logger)
        {
            Configuration = configuration;
            _logger = logger;
            ConnectionString = Configuration.GetConnectionString("DefaultConnectionString");
        }

        public void AddBook(BookVM book)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string commandText =
                "INSERT INTO Books (Title, Description, IsRead, DateRead, Rate, Genre, Author, CoverUrl, DateAdded)" +
                "VALUES " +
                "(@Title, @Description, @IsRead, @DateRead, @Rate, @Genre, @Author, @CoverUrl, @DateAdded)";

                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", book.Title);
                    cmd.Parameters.AddWithValue("@Description", book.Description);
                    cmd.Parameters.AddWithValue("@IsRead", book.IsRead);
                    cmd.Parameters.AddWithValue("@DateRead", (book.IsRead ? book.DateRead : SqlDateTime.Null));
                    cmd.Parameters.AddWithValue("@Rate", (book.IsRead ? book.Rate : SqlInt32.Null));
                    cmd.Parameters.AddWithValue("@Genre", book.Genre);
                    cmd.Parameters.AddWithValue("@Author", book.Author);
                    cmd.Parameters.AddWithValue("@CoverUrl", book.CoverUrl);
                    cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    _logger.LogInformation("Created a new book entry.");
                    connection.Close();
                }
            }
        }

        public Book UpdateBookById(int bookId, BookVM book)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string commandText =
                @"UPDATE Books
                SET
                    Title = @Title, Description = @Description, IsRead = @IsRead, DateRead = @DateRead, Rate = @Rate, Genre = @Genre, Author = @Author, CoverUrl = @CoverUrl
                WHERE
                    Id = @Id";

                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", book.Title);
                    cmd.Parameters.AddWithValue("@Description", book.Description);
                    cmd.Parameters.AddWithValue("@IsRead", book.IsRead);
                    cmd.Parameters.AddWithValue("@DateRead", (book.IsRead ? book.DateRead : SqlDateTime.Null));
                    cmd.Parameters.AddWithValue("@Rate", (book.IsRead ? book.Rate : SqlInt32.Null));
                    cmd.Parameters.AddWithValue("@Genre", book.Genre);
                    cmd.Parameters.AddWithValue("@Author", book.Author);
                    cmd.Parameters.AddWithValue("@CoverUrl", book.CoverUrl);
                    cmd.Parameters.AddWithValue("@Id", bookId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    _logger.LogInformation($"Updated the book with id: {bookId}");
                    connection.Close();
                }

                return GetBookById(bookId);
            }
        }

        public void DeleteBookById(int bookId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string commandText =
                @"DELETE FROM Books WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", bookId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    _logger.LogInformation($"Delete the book with id: {bookId}");
                    connection.Close();
                }
            }
        }


        public List<Book> GetAllBooks()
        {
            _logger.LogInformation("Retrieving all books in database");
            List<Book> books = new List<Book>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Books";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader sqlDataReader = cmd.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            books.Add(new Book
                            {
                                Id = int.Parse(sqlDataReader["Id"].ToString()),
                                Title = sqlDataReader["Title"].ToString(),
                                Description = sqlDataReader["Description"].ToString(),
                                IsRead = bool.Parse(sqlDataReader["IsRead"].ToString()),
                                DateRead = bool.Parse(sqlDataReader["IsRead"].ToString()) ?
                                    DateTime.Parse(sqlDataReader["DateRead"].ToString()) : null,
                                Rate = bool.Parse(sqlDataReader["IsRead"].ToString()) ?
                                    int.Parse(sqlDataReader["Rate"].ToString()) : null,
                                Genre = sqlDataReader["Genre"].ToString(),
                                Author = sqlDataReader["Author"].ToString(),
                                CoverUrl = sqlDataReader["CoverUrl"].ToString(),
                                DateAdded = DateTime.Parse(sqlDataReader["DateAdded"].ToString())
                            });
                        }
                    }
                    connection.Close();
                }
            }
            return books;
        }

        public Book GetBookById(int bookId)
        {
            _logger.LogInformation($"Retrieving the book with id: {bookId} from the database");
            Book book;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = $"SELECT * FROM Books WHERE Id = {bookId}";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.Read())
                        {
                            book = new Book
                            {
                                Id = int.Parse(sqlDataReader["Id"].ToString()),
                                Title = sqlDataReader["Title"].ToString(),
                                Description = sqlDataReader["Description"].ToString(),
                                IsRead = bool.Parse(sqlDataReader["IsRead"].ToString()),
                                DateRead = bool.Parse(sqlDataReader["IsRead"].ToString()) ?
                                    DateTime.Parse(sqlDataReader["DateRead"].ToString()) : null,
                                Rate = bool.Parse(sqlDataReader["IsRead"].ToString()) ?
                                    int.Parse(sqlDataReader["Rate"].ToString()) : null,
                                Genre = sqlDataReader["Genre"].ToString(),
                                Author = sqlDataReader["Author"].ToString(),
                                CoverUrl = sqlDataReader["CoverUrl"].ToString(),
                                DateAdded = DateTime.Parse(sqlDataReader["DateAdded"].ToString())
                            };
                        }
                        else
                            book = null;
                    }
                    connection.Close();
                }
            }
            return book;
        }
    }
}
