using InventarioWEB.Services;
namespace InventarioWEB.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensaje,
            byte[] archivoAdjunto,
            string nombreArchivo);
    }
}