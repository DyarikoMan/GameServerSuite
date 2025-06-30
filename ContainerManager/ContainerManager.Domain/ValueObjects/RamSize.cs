public class RamSize
{
    public int ValueInMb { get; }

    public RamSize(int valueInMb)
    {
        if (valueInMb < 128)
            throw new ArgumentException("RAM خاصها تكون على الأقل 128MB");

        ValueInMb = valueInMb;
    }
}
