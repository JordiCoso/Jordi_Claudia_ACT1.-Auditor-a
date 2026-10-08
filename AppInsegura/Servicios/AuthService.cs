using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AppInsegura.Datos;
using AppInsegura.Modelos;

namespace AppInsegura.Servicios
{
    public class AuthService
    {
        private readonly BaseDatosUsuarios baseDatos;

        public AuthService(BaseDatosUsuarios baseDatos)
        {
            this.baseDatos = baseDatos;
        }

        public Usuario Registrar(string nombre, string contrasena, string rol = "jugador")
        {
            var nuevo = new Usuario
            {
                Nombre = nombre,
                ContrasenaHash = CalcularHash(contrasena),
                Rol = rol,
                TokenSesion = ""
            };

            baseDatos.Agregar(nuevo);
            return nuevo;
        }

        public Usuario? IniciarSesion(string nombre, string contrasena)
        {
            Usuario? usuario = baseDatos.BuscarExacto(nombre);
            if (usuario == null)
            {
                return null;
            }

            string hashIntento = CalcularHash(contrasena);
            if (usuario.ContrasenaHash != hashIntento)
            {
                return null;
            }

            usuario.TokenSesion = GenerarTokenSesion();

            // Quitamos el registro en el log del token

            GuardarSesionEnDisco(usuario);

            return usuario;
        }

        private string CalcularHash(string contrasena)
        {
            SHA512 sha512 = SHA512.Create(); // Cambiamos el md5 por sha512
            byte[] bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(contrasena));
            return Convert.ToHexString(bytes);
        }

        private string GenerarTokenSesion()
        {
            byte[] sal = new byte[16]; // Cambiamos el system.random por RandomNumberGenerator
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sal);
                return rng.ToString(); // Convertimos a string
            }
        }

        private void GuardarSesionEnDisco(Usuario usuario)
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();

                var nombreEnBytes = Encoding.ASCII.GetBytes(usuario.Nombre);
                var tokenEnBytes = Encoding.ASCII.GetBytes(usuario.TokenSesion);

                var nombreEncriptado = aes.EncryptCbc(nombreEnBytes, aes.IV);
                var tokenEncriptado = aes.EncryptCbc(tokenEnBytes, aes.IV);

                var nombreAString = Encoding.UTF8.GetString(nombreEncriptado); 
                var tokenAString = Encoding.UTF8.GetString(tokenEncriptado);

                File.WriteAllText("sesion.txt", $"{nombreAString}:{tokenAString}");
            }
        }
    }
}
