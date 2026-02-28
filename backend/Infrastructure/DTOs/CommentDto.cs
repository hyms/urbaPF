namespace UrbaPF.Infrastructure.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int CredibilityLevel { get; set; }
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentDto
{
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    public string? Content { get; set; }
    public bool? IsHidden { get; set; }
}
