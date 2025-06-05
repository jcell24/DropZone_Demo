using MudBlazor;
using DropZone_Demo.Models;

namespace DropZone_Demo.Components.Pages
{
    public partial class Home
    {
        private bool _showDiscard = false;
        private bool _hoveringDiscard = false;
        private DropItemBase? _currentItem = null;

        // Master List, an example of what to provide to the MudDropContainer component.
        private readonly List<DropItemBase> _items =
            [
                new DropItemEndpoint { Name = "E_1", ServiceId = "ServiceA", ZoneId="ServiceA" },
                new DropItemEndpoint { Name = "E_2", ServiceId = "ServiceB", ZoneId="ServiceB" },
                new DropItemEndpoint { Name = "E_3", ServiceId = "ServiceC", ZoneId="ServiceC" },

                new DropItemLogic {Name = "IF", ZoneId = "Logic", Children = []},
            ];

        IEnumerable<DropItemBase> GetAllDescendants(DropItemBase node)
        {
            if (node is DropItemLogic logicItem)
            {
                foreach (var child in logicItem.Children)
                {
                    yield return child;
                    foreach (var desc in GetAllDescendants(child))
                    {
                        yield return desc;
                    }
                }
            }
        }

        private void OnItemPicked(MudDragAndDropItemTransaction<DropItemBase> transaction)
        {
            _currentItem = transaction.Item;
            _showDiscard = transaction.SourceZoneIdentifier == "Collected";

            StateHasChanged();
        }
        
        // Business logic for managing 'Items', updates or removes the current item depending on the drop zone.
        private void OnItemDropped(MudItemDropInfo<DropItemBase> info)
        {
            if (info.Item == null || GetAllDescendants(info.Item).Any(desc => desc.UniqueGuid.ToString() == info.DropzoneIdentifier)) return;

            string fromZone = info.Item.ZoneId;
            string toZone = info.DropzoneIdentifier;

            // Only show discard option if the item is being moved from the "Collected" zone.
            if (fromZone == "Collected") _showDiscard = false;

            // Discard item
            if (info.DropzoneIdentifier == "Discard" && info.Item.CanDiscard) _items.Remove(info.Item);

            // Store a new item
            else if (fromZone != toZone && !info.Item.CanDiscard)
            {
                // Creating new item
                var newItem = info.Item.Clone();
                newItem.UniqueGuid = Guid.NewGuid();
                newItem.ZoneId = toZone;
                newItem.CanDiscard = true;
                _items.Add(newItem);

                // Check if the new item is a child
                foreach (DropItemBase item in _items)
                {
                    // Add child to list
                    if (item is DropItemLogic logicItem && toZone == item.UniqueGuid.ToString()) dropItemLogic.Children.Add(newItem);
                }
            }

            // Relocate existing item & prevent ancestors from being dropped into descendants
            else if ((info.Item is DropItemLogic logicItem && (logicItem.UniqueGuid.ToString() != toZone && logicItem.Children.All(d => d.UniqueGuid.ToString() != toZone))) || info.Item is DropItemEndpoint endpoint)
            {
                info.Item.ZoneId = toZone;
            }

            _currentItem = null;
            StateHasChanged();
        }
    }
}
