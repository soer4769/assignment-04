namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }
    
    public (Response Response, int ItemId) Create(WorkItemCreateDTO workItem)
    {
        var entity = new WorkItem(workItem.Title);
        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == workItem.AssignedToId);

        if(assignedUser == null && workItem.AssignedToId != null) return (Response.BadRequest, 0);
        
        entity.State = State.New;
        entity.AssignedTo = assignedUser;
        entity.Tags = new List<Tag>();
        
        if(_context.Items.Any(t => t.Id == entity.Id)) return (Response.Conflict, 0);

        _context.Items.Add(entity);
        _context.SaveChanges();

        return (Response.Created, entity.Id);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved() 
    {
        return ReadByState(State.Removed);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        var tagQuery = from t in Read() where t.Tags.Contains(tag) select t;
        return tagQuery.Any() ? tagQuery.ToList() : null!;
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        var userQuery = from t in Read() where t.Id == userId select t;
        return userQuery.Any() ? userQuery.ToList() : null!;
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var stateQuery = from t in Read() where t.State == state select t;
        return stateQuery.Any() ? stateQuery.ToList() : null!;
    }
    
    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        if(!_context.Items.Any()) return null!;
        var tasks = from t in _context.Items select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(x => x.Name).ToList(), t.State);
        return tasks.ToList();
    }

    public Response Update(WorkItemUpdateDTO workitem)
    {
        var entity = _context.Items.Find(workitem.Id);
        if(entity == null) return Response.NotFound;
        
        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == workitem.AssignedToId);
        if(assignedUser == null) return Response.BadRequest;

        entity.Title = workitem.Title;
        entity.AssignedTo = assignedUser;
        entity.Tags = workitem.Tags.Select(x => new Tag(x)).ToList();
        entity.State = workitem.State;
        
        _context.SaveChanges();
        
        return Response.Updated;
    }

    public Response Delete(int workItemId)
    {
        var workitem = _context.Items.FirstOrDefault(u => u.Id == workItemId);
        
        if(workitem == null) return Response.NotFound;
        else if(workitem.State == State.New) _context.Items.Remove(workitem);
        else if (workitem.State == State.Active) workitem.State = State.Removed;
        else return Response.Conflict;
        
        return Response.Deleted;
    }

    public WorkItemDetailsDTO? Find(int workItemId)
    {
        if (!_context.Items.Any(t => t.Id == workItemId)) return null;
        
        var workitem = from t in _context.Items where t.Id == workItemId 
        select new WorkItemDetailsDTO(t.Id, t.Title, null, DateTime.UtcNow, t.AssignedTo.Name, t.Tags.Select(x => x.Name).ToList(), t.State, DateTime.UtcNow);
        return workitem.First();
    }
}