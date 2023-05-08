namespace ASP_201MVC.Data.Entity
{
    public class Rate
    {
        public Guid ItemId { get; set; } // Composite
        public Guid UserId { get; set; } // Primary
        public int Rating { get; set; }
    }
}
