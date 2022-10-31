namespace Mvc.Models;

public class UserFile
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public DateTime? CreatedOn { get; set; }

    public string CreatedDate => CreatedOn?.ToShortDateString() ?? "-";
}