using MudBlazor;
using DropZone_Demo.Models;

namespace DropZone_Demo.Components.Pages;

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

    private static IEnumerable<DropItemBase> GetAllDescendants(DropItemBase node)
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
        if (_currentItem != null)
        {
            _showDiscard = _currentItem.CanDiscard;
        }

        StateHasChanged();
    }
    
    // Business logic for managing 'Items', updates or removes the current item depending on the drop zone.
    private void OnItemDropped(MudItemDropInfo<DropItemBase> info)
    {
        if (info.Item == null || GetAllDescendants(info.Item).Any(desc => desc.UniqueGuid.ToString() == info.DropzoneIdentifier))
        {
            return;
        }

        _currentItem = info.Item;

        string fromZone = info.Item.ZoneId;
        string toZone = info.DropzoneIdentifier;

        // Only show discard option if the item is being moved from the "Collected" zone.
        _showDiscard = false;

        // Discard item
        if (info.DropzoneIdentifier == "Discard" && info.Item.CanDiscard)
        {
            _items.Remove(info.Item);

            DropItemLogic? parent = info.Item.Parent;
            while (parent != null)
            {
                parent.Children.Remove(info.Item);
                parent = parent.Parent;
            }
        }

        // Store a new item
        else if (fromZone != toZone && !info.Item.CanDiscard)
        {
            // Creating new item
            var newItem = info.Item.Clone();
            newItem.UniqueGuid = Guid.NewGuid();
            newItem.Parent = _items.FirstOrDefault(_items => _items?.UniqueGuid.ToString() == toZone, null) as DropItemLogic;
            newItem.ZoneId = toZone;
            newItem.CanDiscard = true;
            _items.Add(newItem);

            // Check if the new item is a child
            foreach (DropItemBase item in _items)
            {
                if (item is DropItemLogic logicItem && toZone == logicItem.UniqueGuid.ToString())
                {
                    // Add child
                    logicItem.Children.Add(newItem);

                    while (logicItem.Parent != null)
                    {
                        logicItem = logicItem.Parent;
                        logicItem.Children.Add(newItem);
                    }
                }
            }
        }
        // Relocate existing item and prevent parents from being consumed by children
        else
        {
            if (toZone == "Collected")
            {
                DropItemLogic? parent = info.Item.Parent;
                while (parent != null)
                {
                    parent.Children.Remove(info.Item);
                    parent = parent.Parent;
                }

                info.Item.ZoneId = toZone;
                info.Item.Parent = null;
            }
            else
            {
                // Update parent
                if (_items.FirstOrDefault(item => item.UniqueGuid.ToString() == toZone) is DropItemLogic nextParent)
                {
                    // Check item type
                    if (info.Item is DropItemLogic logicItem)
                    {
                        // Enhanced cycle detection - check if nextParent is anywhere in the hierarchy of logicItem
                        bool wouldCreateCycle = false;
                        var currentAncestor = logicItem;
                        var descendantGuids = GetAllDescendants(logicItem).Select(d => d.UniqueGuid).ToHashSet();

                        if (descendantGuids.Contains(nextParent.UniqueGuid))
                        {
                            wouldCreateCycle = true;
                        }

                        if (!wouldCreateCycle && logicItem.UniqueGuid != nextParent.UniqueGuid)
                        {
                            // Remove from old parent first
                            logicItem.Parent?.Children.Remove(logicItem);

                            logicItem.ZoneId = toZone;
                            logicItem.Parent = nextParent;
                            nextParent.Children.Add(logicItem);
                        }
                    }
                    // Handle all other item types
                    else
                    {
                        // Remove from old parent first
                        info.Item.Parent?.Children.Remove(info.Item);

                        // Add to new parent
                        info.Item.ZoneId = toZone;
                        info.Item.Parent = nextParent;
                        nextParent.Children.Add(info.Item);
                    }
                }
            }
        }

        _currentItem = null;
        StateHasChanged();
    }
}
