﻿using BlogProject.Domain.Entities.Base;

namespace BlogProject.Domain.Entities
{
    public class CommentEntity : BaseEntity
    {
        public string? Title { get; set; }
        public string Content { get; set; }

        public Guid PostId { get; set; }
        public virtual PostEntity Post { get; set; }


        // ParentComment yoksa null olabilir.
        public Guid? ParentCommentId { get; set; }
        public virtual CommentEntity? ParentComment { get; set; }

        public Guid AuthorId { get; set; }
        public virtual AppUser Author { get; set; }

        public virtual ICollection<CommentEntity>? Replies { get; set; }
        public virtual ICollection<LikeEntity>? Likes { get; set; }
    }
}
