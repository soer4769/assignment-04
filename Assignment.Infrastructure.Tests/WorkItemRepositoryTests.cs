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
        context.AddRange(new WorkItem("Spaghetti"){Id = 1, AssignedTo = context.Users.Find(1), Tags = new List<Tag>{new Tag("eat cake"){Id = 0}}, State = State.New},
                         new WorkItem("Meatballs"){Id = 2, AssignedTo = context.Users.Find(1), State = State.Removed});
        context.SaveChanges();

        _context = context;
        _repository = new WorkItemRepository(_context);
    }

    [Fact]
    public void Create_returns_response_created_and_id_3_given_new_workitem()
    {
        // Arrange
        var workitem = new WorkItemCreateDTO("Chocolate", null, null, new List<string>());

        // Act 
        var actual = _repository.Create(workitem);

        // Assert
        actual.Should().Be((Response.Created, 3));
    }

    [Fact]
    public void Read_returns_all_workitems()
    {
        // Arrange 
        var expected = new[]{
            new WorkItemDTO(1, "Spaghetti", "Poul Poulsen", new List<string>{"eat cake"}.AsReadOnly(), State.New),
            new WorkItemDTO(2, "Meatballs", "Poul Poulsen", new List<string>().AsReadOnly(), State.Removed)
        };

        // Act
        var actual = _repository.Read();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadRemoved_returns_workitem_with_id_2()
    {
        // Arrange
        var expected = new[]{
            new WorkItemDTO(2, "Meatballs", "Poul Poulsen", new List<string>().AsReadOnly(), State.Removed)
        };

        // Act
        var actual = _repository.ReadRemoved();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadByUser_returns_user_given_user_id()
    {
        // Arrange
        var userId = 1;

        // Act
        var actual = _repository.ReadByUser(userId);

        // Assert
        actual.First().Id.Should().Be(userId);
    }

    [Fact]
    public void ReadByState_returns_workitem_with_id_1_given_state_new()
    {
        // Arrange
        var expected = new[]{
            new WorkItemDTO(1, "Spaghetti", "Poul Poulsen", new List<string>{"eat cake"}.AsReadOnly(), State.New)
        };

        // Act
        var actual = _repository.ReadByState(State.New);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadByTag_returns_tag_given_tag_name()
    {
        // Arrange
        var tag = new Tag("eat cake"){Id = 1};

        // Act
        var actual = _repository.ReadByTag(tag.Name);

        // Assert
        actual.First().Id.Should().Be(tag.Id);
    }

    [Fact]
    public void Update_returns_response_notfound_given_maxvalue_id()
    {
        // Arrange
        var workitem = new WorkItemUpdateDTO(int.MaxValue, "lol", null, null, new List<string>{"hello"}, State.New);

        // Act
        var actual = _repository.Update(workitem);

        // Assert
        actual.Should().Be(Response.NotFound);
    }
    
    [Fact]
    public void Update_returns_response_updated_given_updated_workitem()
    {
        // Arrange
        var workitem = new WorkItemUpdateDTO(1, "Work harder", _context.Users.Find(1)!.Id, null, new List<string>{"Max"}, State.Closed);

        // Act
        var actual = _repository.Update(workitem);

        // Assert
        actual.Should().Be(Response.Updated);
    }
    
    [Fact]
    public void Delete_returns_response_deleted_given_tag_id()
    {
        // Arrange
        var tagId = 1;

        // Act
        var actual = _repository.Delete(tagId);

        // Assert
        actual.Should().Be(Response.Deleted);
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
    public void Find_returns_workitem_given_workitem_id()
    {
        // Arrange
        //var workitem = new WorkItem("Meatballs"){Id = 2, AssignedTo = _context.Users.Find(1), State = State.Removed};
        var workitem = new WorkItemDetailsDTO(2, "Meatballs", null, DateTime.UtcNow, "Poul Poulsen", new List<string>{}.ToList(), State.Removed, DateTime.UtcNow);

        // Act
        var actual = _repository.Find(2);

        // Assert
        actual.Id.Should().Be(workitem.Id);
        actual.Title.Should().Be(workitem.Title);
        actual.Description.Should().Be(workitem.Description);
        actual.Created.Should().BeCloseTo(workitem.Created, precision: TimeSpan.FromSeconds(5));
        actual.AssignedToName.Should().Be(workitem.AssignedToName);
        actual.Tags.Should().BeEmpty();
        actual.State.Should().Be(workitem.State);
        actual.StateUpdated.Should().BeCloseTo(workitem.StateUpdated, precision: TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}