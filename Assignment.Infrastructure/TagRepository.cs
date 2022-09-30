namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;

    public TagRepository(KanbanContext context)
    {
        _context = context;
    }
    
    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        Response response;
        var entity = new Tag(tag.Name);
        var tagExists = _context.Tags.FirstOrDefault(t => t.Name == tag.Name) != null;

        if(tagExists) 
        {
            return (Response.Conflict, 0);
        }
        _context.Tags.Add(entity);
        _context.SaveChanges();

        response = Response.Created;

        return (response, entity.Id); 
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        if (!_context.Tags.Any())
        {
            return null!;
        }
        
        var tags = from t in _context.Tags
                    select new TagDTO(t.Id, t.Name);

        return tags.ToArray();
    }

    public TagDTO Find(int tagId)
    {
        var ReadTag = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        return ReadTag == null ? null : new TagDTO(ReadTag.Id, ReadTag.Name);
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.Find(tag.Id);
        if(entity == null) return Response.NotFound;
        entity.Name = tag.Name;
        _context.SaveChanges();

        return Response.Updated;
    }

    public Response Delete(int tagId, bool force = false)
    {
        var tag = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        if(tag == null)
        {
            return Response.NotFound;
        } 
        
        bool AssignedToTask = false;
        foreach(var task in tag.WorkItems) {
            if (task.State == State.Active) {
                AssignedToTask = true;
                break;
            }
        }

        if(AssignedToTask && !force) 
        {
            return Response.Conflict;
        }
        
        _context.Tags.Remove(tag!);
        return Response.Deleted;
    }
}