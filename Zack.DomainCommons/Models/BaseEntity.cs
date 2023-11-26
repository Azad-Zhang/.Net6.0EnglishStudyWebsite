using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zack.DomainCommons.Models;


//项目要求：所有实体类，都是Guid类型，并且携带领域事件发布功能

public record BaseEntity : IEntity, IDomainEvents
{

    [NotMapped]
    private List<INotification> domainEvents = new();

    public Guid Id { get; protected set; } = Guid.NewGuid();

    public void AddDomainEvent(INotification eventItem)
    {
        domainEvents.Add(eventItem);
    }

    public void AddDomainEventIfAbsent(INotification eventItem)
    {
        if (!domainEvents.Contains(eventItem))
        {
            domainEvents.Add(eventItem);
        }
    }
    public void ClearDomainEvents()
    {
        domainEvents.Clear();
    }

    public IEnumerable<INotification> GetDomainEvents()
    {
        return domainEvents;
    }
}
