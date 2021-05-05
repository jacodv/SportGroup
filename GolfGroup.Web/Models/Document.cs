using System;
using GolfGroup.Api.Interfaces;
using MongoDB.Bson;

namespace GolfGroup.Api.Models
{
  public abstract class Document : IDocument
  {
    protected Document()
    {
      Id=ObjectId.GenerateNewId();
    }
    public ObjectId Id { get; set; }
    public DateTime CreatedAt => Id.CreationTime;
    public string CreatedBy { get; set; }
  }
}
