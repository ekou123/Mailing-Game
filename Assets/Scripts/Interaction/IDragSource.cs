// Something you click (with empty hands) to get an item out of, like bagging up a sorting bin
// or pulling a crate out of the van. Needs a collider on the Bin layer.
public interface IDragSource
{
    void TakeItem();
}
