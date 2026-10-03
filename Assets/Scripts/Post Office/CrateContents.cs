// What's inside a crate of sorted mail. Misrouted parcels were sorted into the wrong district's bin,
// so they reach the wrong address when the crate is delivered.
public readonly struct CrateContents
{
    public readonly string district;
    public readonly int parcels;
    public readonly int misrouted;

    public CrateContents(string district, int parcels, int misrouted)
    {
        this.district = district;
        this.parcels = parcels;
        this.misrouted = misrouted;
    }

    public int CorrectlyAddressed => parcels - misrouted;

    // Sent over the network and stored in room properties as "North|5|1"
    public string Encode() => $"{district}|{parcels}|{misrouted}";

    public static CrateContents Decode(string encoded)
    {
        string[] parts = encoded.Split('|');
        return new CrateContents(parts[0], int.Parse(parts[1]), int.Parse(parts[2]));
    }
}
