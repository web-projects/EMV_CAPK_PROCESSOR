namespace EMVCapkProcessor.Processor
{
    public enum CapkSchemaIndex : int
    {
        Expiration = 0,
        HashAlgorithmIndicator = 1,
        PublicKeyAlgorithmIndicator = 2,
        RegisterApplicationProviderIdentifier = 3,  // RID
        CAPublicKeyIndex = 4,
        PublicKeyModulus = 5,
        PublicKeyExponent = 6,
        PublicKeyCheckSum = 7
    }
}
