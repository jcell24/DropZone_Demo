namespace DropZone_Demo.Models
{
    public abstract class DropItemBase
    {
        public required string Name { get; set; }
        public required string ZoneId { get; set; }
        public bool CanDiscard { get; set; } = false;

        public abstract DropItemBase Clone();
    }

    public class  DropItemEndpoint : DropItemBase
    {
        public required string ServiceId { get; set; }

        public override DropItemBase Clone()
        {
            return new DropItemEndpoint
            {
                Name = this.Name,
                ZoneId = this.ZoneId,
                ServiceId = this.ServiceId,
                CanDiscard = this.CanDiscard
            };
        }
    }

    public class  DropItemIf : DropItemBase
    {
        public List<DropItemBase> Children { get; set; } = new();

        public override DropItemBase Clone()
        {
            return new DropItemIf
            {
                Name = this.Name,
                ZoneId = this.ZoneId,
                CanDiscard = this.CanDiscard,
                Children = this.Children.Select(child => child.Clone()).ToList()
            };
        }

        // Variables here? Properties specific to If statement logic can be added here.
    }
}
