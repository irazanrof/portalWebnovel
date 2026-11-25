using System.Security.Cryptography;

namespace PortalWebnovels.Models;

public static class PasswordHasher
{
    private const string Prefixo = "PBKDF2";
    private const int Iteracoes = 100_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;

    public static string Gerar(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
        return $"{Prefixo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string? hashGuardado)
    {
        if (string.IsNullOrEmpty(hashGuardado))
            return false;

        var partes = hashGuardado.Split('$');
        if (partes.Length != 4 || partes[0] != Prefixo || !int.TryParse(partes[1], out int iteracoes))
            return false;

        try
        {
            byte[] salt = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
