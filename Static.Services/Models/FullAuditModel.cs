using System.ComponentModel.DataAnnotations;

namespace Static.Services.Models;

public class FullAuditModel : IIdentity
{
    [Key]
    public int Id { get; set; }
}


public interface IIdentity
{
    [Key]
    public int Id { get; set; }
}