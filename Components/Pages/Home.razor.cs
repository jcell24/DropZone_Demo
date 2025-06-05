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
                new DropItemIf {Name = "IF", ZoneId = "Logic", Children = []},
            ];

        private void OnItemPicked(MudDragAndDropItemTransaction<DropItemBase> transaction)
        {
            _currentItem = transaction.Item;
            _showDiscard = transaction.SourceZoneIdentifier == "Collected";

            StateHasChanged();
        }
        
        // Business logic for managing 'Items', updates or removes the current item depending on the drop zone.
        private void OnItemDropped(MudItemDropInfo<DropItemBase> info)
        {
            if (info.Item == null) return;

            string fromZone = info.Item.ZoneId;
            string toZone = info.DropzoneIdentifier;

            // Only show discard option if the item is being moved from the "Collected" zone.
            if (fromZone == "Collected") _showDiscard = false;

            // Discard Item
            if (info.DropzoneIdentifier == "Discard" && info.Item.CanDiscard) _items.Remove(info.Item);

            else // Store Item
            {
                var newItem = info.Item.Clone();
                newItem.ZoneId = toZone;
                newItem.CanDiscard = true;
                _items.Add(newItem);
            }

            _currentItem = null;
            StateHasChanged();
        }
    }
}
