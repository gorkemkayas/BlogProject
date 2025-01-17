﻿namespace BlogProject.src.Infra.Entitites
{
    public class SavedPostEntity
    {
        public Guid Id { get; set; }
        public DateTime SavedDate { get; set; }

        public Guid UserId { get; set; }
        public virtual UserEntity User { get; set; }
    }
}
