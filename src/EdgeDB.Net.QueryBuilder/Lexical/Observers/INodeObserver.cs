namespace EdgeDB;

internal interface INodeObserver
{
    void OnAdd(ref LooseLinkedList<Value>.Node node);
    void OnRemove(ref LooseLinkedList<Value>.Node node);
}
