﻿namespace BlogProject.src.Infra.Entitites.Base
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public bool IsDeleted { get; set; }
    }
}
