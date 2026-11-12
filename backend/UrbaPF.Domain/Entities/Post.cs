namespace UrbaPF.Domain.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CondominiumId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Category { get; set; }
    public bool IsPinned { get; set; }
    public bool IsAnnouncement { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
