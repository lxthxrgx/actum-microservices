namespace Archive.model
{
    public class TransferData
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;
        public bool isCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; } = null;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// this meen data(data from groups,sublease, guard by number group) transfer to archive
    /// </summary>
    public class TransferDataAll : TransferData
    {
        
    }

    public class TransferDataSublease : TransferData
    {
        
    }
    public class TransferDataGuard : TransferData
    {
        
    }

    /// <summary>
    /// im not sure but maybe this is used to transfer groups of data
    /// </summary>
    public class TransferDataGroups : TransferData
    {
        
    }
}
