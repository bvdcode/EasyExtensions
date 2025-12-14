namespace EasyExtensions.Crypto.Tests;

public class KeyDerivationTests
{
    private const string Master1 = "master-key-1";
    private const string Master2 = "master-key-2";
    private const string PurposeA = "encryption-key";
    private const string PurposeB = "hmac-key";

    [Test]
    public void DeriveSubkey_Returns_Requested_Length([Values(0, 1, 16, 32, 48, 64, 100)] int len)
    {
        var bytes = KeyDerivation.DeriveSubkey(Master1, PurposeA, len);
        Assert.That(bytes, Is.Not.Null);
        Assert.That(bytes, Has.Length.EqualTo(len));
        if (len > 0)
        {
            // Not all zeros
            Assert.That(bytes.Any(b => b != 0), Is.True);
        }
    }

    [Test]
    public void Deterministic_For_Same_Inputs()
    {
        var a1 = KeyDerivation.DeriveSubkey(Master1, PurposeA, 48);
        var a2 = KeyDerivation.DeriveSubkey(Master1, PurposeA, 48);
        Assert.That(a1, Is.EqualTo(a2));
    }

    [Test]
    public void Different_Master_Produces_Different_Key()
    {
        var a = KeyDerivation.DeriveSubkey(Master1, PurposeA, 48);
        var b = KeyDerivation.DeriveSubkey(Master2, PurposeA, 48);
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Different_Purpose_Produces_Different_Key()
    {
        var a = KeyDerivation.DeriveSubkey(Master1, PurposeA, 48);
        var b = KeyDerivation.DeriveSubkey(Master1, PurposeB, 48);
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Base64_Matches_Raw_Encoding()
    {
        var bytes = KeyDerivation.DeriveSubkey(Master1, PurposeA, 32);
        var b64A = EasyExtensions.Crypto.KeyDerivation.DeriveSubkeyBase64(Master1, PurposeA, 32);
        var b64B = Convert.ToBase64String(bytes);
        Assert.That(b64A, Is.EqualTo(b64B));
    }

    [Test]
    public void Length32_Vs_Length64_Prefixes_NotDiffer_By_Design()
    {
        // length 32 uses HMAC(purpose) directly
        var l32 = KeyDerivation.DeriveSubkey(Master1, PurposeA, 32);
        // length 64 uses HMAC(purpose||1) + HMAC(purpose||2)
        var l64 = KeyDerivation.DeriveSubkey(Master1, PurposeA, 64);
        Assert.That(l32, Is.EqualTo(l64.Take(32).ToArray()));
    }

    [Test]
    public void Longer_Length_Extends_With_Deterministic_Blocks()
    {
        // For lengths > 32, result is concatenation of counter-based blocks starting at 1,
        // so prefix of 64 should match prefix of 96.
        var l64 = KeyDerivation.DeriveSubkey(Master1, PurposeA, 64);
        var l96 = KeyDerivation.DeriveSubkey(Master1, PurposeA, 96);
        Assert.That(l96.Take(64).ToArray(), Is.EqualTo(l64));
    }
}
