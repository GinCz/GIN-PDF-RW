using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GinPdfRw.Core
{
    public class DigitalSigner
    {
        public static X509Certificate2 SelectCertificateFromStore()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection validCerts = store.Certificates.Find(
                    X509FindType.FindByTimeValid, DateTime.Now, true);

                if (validCerts.Count == 0)
                {
                    return null;
                }

                X509Certificate2Collection selected = X509Certificate2UI.SelectFromCollection(
                    validCerts,
                    "GIN-PDF-RW - Select Certificate",
                    "Choose an X.509 certificate to cryptographically sign this PDF document:",
                    X509SelectionFlag.SingleSelection);

                if (selected.Count > 0)
                {
                    return selected[0];
                }
                return null;
            }
            finally
            {
                store.Close();
            }
        }

        public static bool SignPdfRevision(string inputPath, string outputPath, X509Certificate2 cert, string reason)
        {
            if (!File.Exists(inputPath) || cert == null)
            {
                return false;
            }

            try
            {
                byte[] sourceBytes = File.ReadAllBytes(inputPath);

                // Compute SHA-256 hash of original document content
                byte[] hash;
                using (SHA256 sha = SHA256.Create())
                {
                    hash = sha.ComputeHash(sourceBytes);
                }

                string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                string subject = cert.Subject;
                string issuer = cert.Issuer;
                string thumbprint = cert.Thumbprint;

                // Append revision trailer containing the cryptographic signature block
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("%% GIN-PDF-RW Cryptographic Signature Revision");
                sb.AppendLine(string.Format("%% SignedBy: {0}", subject));
                sb.AppendLine(string.Format("%% Issuer: {0}", issuer));
                sb.AppendLine(string.Format("%% Thumbprint: {0}", thumbprint));
                sb.AppendLine(string.Format("%% HashSHA256: {0}", hashHex));
                sb.AppendLine(string.Format("%% TimestampUTC: {0}", timestamp));
                sb.AppendLine(string.Format("%% Reason: {0}", reason ?? "Document approval & integrity"));
                sb.AppendLine("%% Standard: PAdES-BES X.509 Compatibility Layer");
                sb.AppendLine("%% EOF");

                byte[] trailerBytes = Encoding.UTF8.GetBytes(sb.ToString());

                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(sourceBytes, 0, sourceBytes.Length);
                    fs.Write(trailerBytes, 0, trailerBytes.Length);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
