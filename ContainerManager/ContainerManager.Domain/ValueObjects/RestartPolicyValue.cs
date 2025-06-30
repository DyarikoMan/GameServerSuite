public enum RestartPolicyEnum
{
    No,
    Always,
    UnlessStopped
}

public class RestartPolicyValue
{
    public RestartPolicyEnum Value { get; }

    public RestartPolicyValue(string value)
    {
        Value = value.ToLower() switch
        {
            "always" => RestartPolicyEnum.Always,
            "unless-stopped" => RestartPolicyEnum.UnlessStopped,
            _ => RestartPolicyEnum.No
        };
    }

    public override string ToString() => Value.ToString().ToLower();
}
