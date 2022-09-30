namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly WorkItemRepository _repository;

    public WorkItemRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();
        context.Add(new User("Poul Poulsen","poul@thepoul.dk"){Id = 1});

        var tag1 = new Tag("eat cake"){Id = 0};
        var task = new WorkItem("Spaghetti"){Id = 1, AssignedTo = context.Users.Find(1), Tags = new List<Tag>{tag1}, State = State.New};
        var task2 = new WorkItem("Meatballs"){Id = 2, AssignedTo = context.Users.Find(1), State = State.Removed};

        context.Add(task);
        context.Add(task2);
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }
    

    [Fact]
    public void Create_should_return_task()
    {
        // Arrange
        var task = new WorkItemCreateDTO("Chocolate", null, null, new List<string>());

        // Act 
        var actual = _repository.Create(task);

        // Assert
        actual.Should().Be((Response.Created, 3));
    }

    [Fact]
    public void Read_returns_id_1_and_2()
    {
        var actual = _repository.Read();
        actual.Should().BeEquivalentTo(new[]{
            new WorkItemDTO(1, "Spaghetti", "Poul Poulsen", new List<string>{"eat cake"}.AsReadOnly(), State.New),
            new WorkItemDTO(2, "Meatballs", "Poul Poulsen", new List<string>().AsReadOnly(), State.Removed)
        });
    }

    [Fact]
    public void ReadRemoved_returns_id_2()
    {
        var actual = _repository.ReadRemoved();
        actual.Should().BeEquivalentTo(new[]{
            new WorkItemDTO(2, "Meatballs", "Poul Poulsen", new List<string>().AsReadOnly(), State.Removed)
        });
    }

    [Fact]
    public void ReadByUser_returns_id_1()
    {
        var actual = _repository.ReadByUser(1);
        actual.First().Id.Should().Be(1);
    }

    [Fact]
    public void ReadByState_returns_id_1()
    {
        var actual = _repository.ReadByState(State.New);
        actual.Should().BeEquivalentTo(new[]{
            new WorkItemDTO(1, "Spaghetti", "Poul Poulsen", new List<string>{"eat cake"}.AsReadOnly(), State.New)
        });
    }

    [Fact]
    public void ReadByTag_returns_id_1()
    {
        var actual = _repository.ReadByTag("eat cake");
        actual.First().Id.Should().Be(1);
    }

    [Fact]
    public void Update_returns_notfound_given_maxvalue_id()
    {
        var newWorkItem = new WorkItemUpdateDTO(int.MaxValue, "lol", null, null, new List<string>{"hello"}, State.New);
        var actual = _repository.Update(newWorkItem);

        actual.Should().Be(Response.NotFound);
    }
    
    [Fact]
    public void Update_returns_update_response()
    {
        var newWorkItem = new WorkItemUpdateDTO(1, "Work harder", _context.Users.Find(1)!.Id, null, new List<string>{"Max"}, State.Closed);
        var actual = _repository.Update(newWorkItem);

        actual.Should().Be(Response.Updated);
    }
    
    [Fact]
    public void Delete_returns_deleted_response_given_userId()
    {
        var actual = _repository.Delete(1);
        actual.Should().Be(Response.Deleted);
    }
    
    [Fact]
    public void Delete_returns_notfound_null_given_MaxValue_userId() {
        
        var actual = _repository.Delete(int.MaxValue);
        actual.Should().Be(Response.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}