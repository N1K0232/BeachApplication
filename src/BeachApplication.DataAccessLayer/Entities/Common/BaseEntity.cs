﻿namespace BeachApplication.DataAccessLayer.Entities.Common;

public abstract class BaseEntity
{
    public virtual Guid Id { get; set; }

    public virtual DateTime CreationDate { get; set; }

    public virtual DateTime? LastModificationDate { get; set; }
}