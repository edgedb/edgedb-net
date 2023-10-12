namespace EdgeDB.Binary.Protocol;

internal enum ProtocolPhase
{
    Connection,
    Auth,
    Command,
    Dump,
    Termination,
    Errored
}
