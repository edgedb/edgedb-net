namespace EdgeDB;

internal interface IWriteable
{
    void Write(QueryWriter writer);
}
