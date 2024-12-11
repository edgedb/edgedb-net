namespace EdgeDB;

internal interface INodeObserver
{
    void OnAdd(LooseLinkedList<Token>.Node node);
    void OnRemove(LooseLinkedList<Token>.Node node);
}
