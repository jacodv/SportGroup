using System;

namespace GolfGroup.Api.Models
{
  public class DocumentModel
  {
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
  }
}