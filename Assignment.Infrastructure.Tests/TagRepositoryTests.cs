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

        var tag = new Tag("Teddybear"){Id = 1};
        var tag2 = new Tag("Balloon"){Id = 2};
        context.Add(tag);
        context.Add(tag2);
        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Create_returns_name_and_created_response() 
    {
        //Arrange
        var tag = new TagCreateDTO("Monster");

        //act
        var actual = _repository.Create(tag);

        //assert
        actual.Should().Be((Response.Created, 3));
    }

    [Fact]
    public void Create_returns_conflict_response_given_same_name() 
    {
        //Arrange
        var tag = new TagCreateDTO("Teddybear");

        //act
        var actual = _repository.Create(tag);

        //assert
        actual.Should().Be((Response.Conflict, 0));
    }

    [Fact]
    public void Delete_returns_deleted_response_given_tagId() 
    {
        var actual = _repository.Delete(1);

        actual.Should().Be(Response.Deleted);
    }

    [Fact]
    public void Read_returns_TagDTO_given_tagId() 
    {
        var actual = _repository.Read();

        var expected = new TagDTO(1, "Teddybear");

        actual.First().Should().Be(expected);
    }

    [Fact]
    public void Read_returns_all_tags() 
    {
        var actual = _repository.Read();
        actual.Should().BeEquivalentTo(new[] {
            new TagDTO(1, "Teddybear"), 
            new TagDTO(2, "Balloon")
        });
    }

    [Fact]
    public void Update_returns_response_given_tag_update_dto() 
    {
        var newTag = new TagUpdateDTO(1, "Candyfloss");
        var actual = _repository.Update(newTag);

        actual.Should().Be(Response.Updated);
    }

    [Fact]
    public void Delete_notfound_null_given_MaxValue_tagid() 
    {
        var actual = _repository.Delete(int.MaxValue);

        actual.Should().Be(Response.NotFound);
    }

    [Fact]
    public void Update_returns_notfound_given_maxvalue_tagID() 
    {
        var newTag = new TagUpdateDTO(int.MaxValue, "Teddybear");
        var actual = _repository.Update(newTag);

        actual.Should().Be(Response.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
