using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsegura.Servicios
{
    public class RedService
    {
        private string ApiKey = Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.User);
        private const string UrlServidor = "https://api.miapp-insegura.local/puntuaciones"; // Usar conexion segura https

        public void EnviarPuntuacion(string nombreUsuario, int puntuacion)
        {
            try
            {
                EnviarPuntuacionAsync(nombreUsuario, puntuacion).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                Console.WriteLine("No se ha podido conectar con el servidor (es normal si no tienes conexión real)");
                // Quitamos la respuesta del error para el usuario
            }
        }

        private async Task EnviarPuntuacionAsync(string nombreUsuario, int puntuacion)
        {
            using var cliente = new HttpClient();
            string url = $"{UrlServidor}?usuario={nombreUsuario}&puntos={puntuacion}&api_key={ApiKey}";

            Console.WriteLine($"Enviando puntuación a: {url}");
            HttpResponseMessage respuesta = await cliente.GetAsync(url);
            Console.WriteLine($"Respuesta del servidor: {(int)respuesta.StatusCode}");
        }
    }
}
