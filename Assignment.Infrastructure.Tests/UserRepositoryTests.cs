namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);

        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        context.AddRange(new User("Poul Poulsen","poul@thepoul.dk"){Id = 1},
                         new User("Jens Jensen","jens@thejensen.dk"){Id = 2});
        context.SaveChanges();

        _context = context;
        _repository = new UserRepository(_context);
    }

    [Fact]
    public void Create_returns_response_created_and_user_id_given_new_user() 
    {
        // Arrange
        var user = new UserCreateDTO("Jens", "jens@jensen.dk");

        // Act
        var actual = _repository.Create(user);
        
        // Assert
        actual.Should().Be((Response.Created, 3));
    }

    [Fact]
    public void Create_returns_response_conflict_and_user_id_given_used_email() 
    {
        // Arrange
        var user = new UserCreateDTO("Henrik", "poul@thepoul.dk");

        // Act
        var actual = _repository.Create(user);

        // Assert
        actual.Should().Be((Response.Conflict, 0));
    }

    [Fact]
    public void Delete_returns_response_deleted_given_user_id() 
    {
        // Arrange
        var userid = 1;

        // Act
        var actual = _repository.Delete(userid);

        // Assert
        actual.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Find_returns_user_given_user_id() 
    {
        // Arrange
        var expected = new UserDTO(1, "Poul Poulsen", "poul@thepoul.dk");

        // Act
        var actual = _repository.Find(1);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Read_returns_all_users() 
    {
        // Arrange
        var expected = new[] {
            new UserDTO(1,"Poul Poulsen", "poul@thepoul.dk"), new UserDTO(2, "Jens Jensen", "jens@thejensen.dk")
        };

        // Act
        var actual = _repository.Read();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Update_returns_response_updated_given_updated_user() 
    {
        // Arrange
        var newUser = new UserUpdateDTO(1, "Poul Poulsen", "poulcool@thepoul.dk");

        // Act
        var actual = _repository.Update(newUser);

        // Assert
        actual.Should().Be(Response.Updated);
    }

    [Fact]
    public void Delete_returns_response_notfound_given_maxvalue_id() 
    {
        // Arrange


        // Act    
        var actual = _repository.Delete(int.MaxValue);

        // Assert
        actual.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Update_returns_response_notfound_given_maxvalue_id() 
    {
        // Arrange
        var newUser = new UserUpdateDTO(int.MaxValue, "Poul Poulsen", "poulcool@thepoul.dk");

        // Act
        var actual = _repository.Update(newUser);

        // Assert
        actual.Should().Be(Response.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}