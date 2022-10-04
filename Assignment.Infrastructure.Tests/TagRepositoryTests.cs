namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);

        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        context.AddRange(new Tag("Teddybear"){Id = 1}, 
                         new Tag("Balloon"){Id = 2});
        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Create_returns_response_created_and_tag_id_given_tag() 
    {
        //Arrange
        var tag = new TagCreateDTO("Monster");

        //Act
        var actual = _repository.Create(tag);

        //Assert
        actual.Should().Be((Response.Created, 3));
    }

    [Fact]
    public void Create_returns_response_conflict_and_tag_id_given_used_name() 
    {
        //Arrange
        var tag = new TagCreateDTO("Teddybear");

        //Act
        var actual = _repository.Create(tag);

        //Assert
        actual.Should().Be((Response.Conflict, 0));
    }

    [Fact]
    public void Delete_returns_response_deleted_given_tag_id() 
    {
        // Arrange
        var tagid = 1;

        // Act
        var actual = _repository.Delete(tagid);

        // Assert
        actual.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Find_returns_tag_given_tag_id()
    {
        // Arrange
        var expected = new TagDTO(1, "Teddybear");

        // Act
        var actual = _repository.Find(1);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Read_returns_all_tags() 
    {
        // Arrange
        var expected = new[] { new TagDTO(1, "Teddybear"), new TagDTO(2, "Balloon") };

        // Act
        var actual = _repository.Read();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Update_returns_response_updated_given_updated_tag() 
    {
        // Arrange
        var newTag = new TagUpdateDTO(1, "Candyfloss");

        // Act
        var actual = _repository.Update(newTag);

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
        var newTag = new TagUpdateDTO(int.MaxValue, "Teddybear");

        // Act
        var actual = _repository.Update(newTag);

        // Assert
        actual.Should().Be(Response.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}