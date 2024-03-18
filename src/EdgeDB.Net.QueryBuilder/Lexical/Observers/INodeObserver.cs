namespace EdgeDB;

internal interface INodeObserver
{
    void OnAdd(LooseLinkedList<Value>.Node node);
    void OnRemove(LooseLinkedList<Value>.Node node);
}
