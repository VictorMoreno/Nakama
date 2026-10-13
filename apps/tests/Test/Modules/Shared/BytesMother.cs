namespace Test.Modules.Shared;

public static class BytesMother
{
    public static byte[] Random() => MotherCreator.Random.Random.Bytes(16);
}
