public class CpuCount
{
    public double Cores { get; }

    public CpuCount(double cores)
    {
        if (cores < 0.1)
            throw new ArgumentException("عدد الـ CPU خاصو يكون على الأقل 0.1");

        Cores = cores;
    }
}
