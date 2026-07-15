using System;

namespace BuildingBlocks.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; } = false;
        public string? CreatedBy { get; protected set; }
        public string? UpdatedBy { get; protected set; }

        public void MarkAsUpdated(string? updatedBy = null)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void MarkAsDeleted(string? deletedBy = null)
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deletedBy;
        }
    }
}
