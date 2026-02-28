namespace UrbaPF.Infrastructure.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid CondominiumId { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Category { get; set; }
    public bool IsPinned { get; set; }
    public bool IsAnnouncement { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ViewCount { get; set; }
}

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Category { get; set; }
    public bool IsPinned { get; set; }
    public bool IsAnnouncement { get; set; }
}

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int? Category { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsAnnouncement { get; set; }
    public int? Status { get; set; }
}
